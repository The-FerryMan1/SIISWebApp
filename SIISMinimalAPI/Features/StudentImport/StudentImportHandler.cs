using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;
using SIISMinimalAPI.Features.Logs;
using SIISMinimalAPI.Features.Shared.Enums;
using SIISMinimalAPI.Features.Shared.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace SIISMinimalAPI.Features.StudentImport;

public class StudentImportHandler(AppDbContext context, ILogService logService) : IStudentImportService
{
    private readonly AppDbContext _context = context;
    private readonly ILogService _logService = logService;
    private static readonly Regex PhoneRegex = new(@"^(\+63|0)\d{10}$", RegexOptions.Compiled);
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".xlsx",
        ".xls"
    };

    public async Task<StudentImportResultDto> ImportAsync(IFormFile file, string userId, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            throw new ArgumentException("Excel file is required.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            throw new ArgumentException("Invalid file type. Use .xlsx or .xls.");
        }

        await using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, ct);
        memoryStream.Position = 0;

        using var workbook = new XLWorkbook(memoryStream);
        if (!workbook.Worksheets.Any())
        {
            throw new ArgumentException("The Excel file must contain at least one worksheet.");
        }

        var worksheet = workbook.Worksheets.First();
        var headers = GetHeaders(worksheet);
        var result = new StudentImportResultDto();
        var importedEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;

        var existingSchools = await _context.Students
            .Where(s => !s.IsDeleted)
            .Select(s => s.SchoolName)
            .Distinct()
            .ToListAsync(ct);

        var schoolNameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var schoolFingerprints = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var school in existingSchools)
        {
            if (!string.IsNullOrWhiteSpace(school))
            {
                var normalized = NormalizeSchoolName(school);
                schoolNameMap[normalized] = school;
                var fingerprint = GetSchoolNameFingerprint(school);
                schoolFingerprints[fingerprint] = school;
            }
        }

        for (var rowNumber = 2; rowNumber <= lastRow; rowNumber++)
        {
            var skipRow = false;
            result.TotalRows++;

            if (IsRowEmpty(worksheet.Row(rowNumber)))
            {
                result.TotalRows--;
                continue;
            }

            var rowEmail = GetCellValue(worksheet, headers, "Email", rowNumber);
            try
            {
                var student = BuildStudentFromRow(worksheet, headers, rowNumber, out var rowEmailValue);
                rowEmail = rowEmailValue;

                if (string.IsNullOrWhiteSpace(student.Email))
                {
                    throw new ArgumentException("Email is required.");
                }

                if (await _context.Students.AnyAsync(s => s.Email.ToLower() == student.Email.ToLower(), ct))
                {
                    result.SkippedCount++;
                    result.Errors.Add(new StudentImportErrorDto
                    {
                        RowNumber = rowNumber,
                        Email = rowEmail,
                        Message = "Skipped: email already exists in the database."
                    });
                    continue;
                }

                if (importedEmails.Contains(student.Email))
                {
                    result.SkippedCount++;
                    result.Errors.Add(new StudentImportErrorDto
                    {
                        RowNumber = rowNumber,
                        Email = rowEmail,
                        Message = "Skipped: duplicate email within this import file."
                    });
                    continue;
                }

                importedEmails.Add(student.Email);

                var normalizedSchoolName = NormalizeSchoolName(student.SchoolName);
                var schoolFingerprint = GetSchoolNameFingerprint(student.SchoolName);

                if (schoolNameMap.TryGetValue(normalizedSchoolName, out var existingSchoolName) && existingSchoolName != normalizedSchoolName)
                {
                    var studentsToUpdate = await _context.Students
                        .Where(s => s.SchoolName == existingSchoolName && !s.IsDeleted)
                        .ToListAsync(ct);

                    foreach (var s in studentsToUpdate)
                    {
                        s.SchoolName = normalizedSchoolName;
                    }

                    schoolNameMap[normalizedSchoolName] = normalizedSchoolName;
                    schoolNameMap.Remove(existingSchoolName);
                    schoolFingerprints[schoolFingerprint] = normalizedSchoolName;
                }
                else if (schoolFingerprints.TryGetValue(schoolFingerprint, out var matchedSchool) && matchedSchool != normalizedSchoolName)
                {
                    var studentsToUpdate = await _context.Students
                        .Where(s => s.SchoolName == matchedSchool && !s.IsDeleted)
                        .ToListAsync(ct);

                    foreach (var s in studentsToUpdate)
                    {
                        s.SchoolName = normalizedSchoolName;
                    }

                    schoolNameMap[normalizedSchoolName] = normalizedSchoolName;
                    schoolNameMap.Remove(matchedSchool);
                    schoolFingerprints[schoolFingerprint] = normalizedSchoolName;
                }
                else
                {
                    foreach (var kvp in schoolNameMap.ToList())
                    {
                        if (GetSchoolNameSimilarity(kvp.Key, normalizedSchoolName) > 0.7)
                        {
                            var studentsToUpdate = await _context.Students
                                .Where(s => s.SchoolName == kvp.Value && !s.IsDeleted)
                                .ToListAsync(ct);

                            foreach (var s in studentsToUpdate)
                            {
                                s.SchoolName = normalizedSchoolName;
                            }

                            schoolNameMap[normalizedSchoolName] = normalizedSchoolName;
                            schoolNameMap.Remove(kvp.Key);
                            schoolFingerprints[schoolFingerprint] = normalizedSchoolName;
                            break;
                        }
                    }
                }

                student.SchoolName = normalizedSchoolName;

                var startDate = GetRequiredDate(worksheet, headers, "InternshipStartDate", rowNumber);
                var estimatedEndDate = GetRequiredDate(worksheet, headers, "EstimatedInternshipEndDate", rowNumber);

                var officeDeployment = GetCellValue(worksheet, headers, "OfficeDeployment", rowNumber);
                long? officeId = null;
                if (!string.IsNullOrWhiteSpace(officeDeployment))
                {
                    var officeName = ExtractOfficeName(officeDeployment);
                    var office = await _context.Offices
                        .FirstOrDefaultAsync(t => t.OfficeName == officeName, ct);

                    if (office != null)
                    {
                        officeId = office.Id;
                    }
                }

                student.Placement = new Placement
                {
                    OfficeId = officeId,
                    StartDate = startDate,
                    EstimatedEndDate = estimatedEndDate,
                    AccumulatedHours = 0,
                    PlacementStatus = PlacementStatusEnum.Ongoing,
                    Progresses = new List<SIISMinimalAPI.Features.Shared.Models.Progress>
                    {
                        new SIISMinimalAPI.Features.Shared.Models.Progress
                        {
                            TrainingHoursRendered = 0,
                            TrainingHoursForWeek = 0,
                            RemainingHours = student.TotalInternshipHours,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now
                        }
                    }
                };

                await _context.Students.AddAsync(student, ct);
                result.ImportedCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new StudentImportErrorDto
                {
                    RowNumber = rowNumber,
                    Email = rowEmail,
                    Message = ex.Message
                });
                skipRow = true;
            }

            if (!skipRow)
            {
                // continue to next row
            }
        }

        if (result.ImportedCount > 0)
        {
            await _context.SaveChangesAsync(ct);
        }

        _logService.WriteAsync(
            "Import",
            "Student",
            null,
            userId,
            $"Imported {result.ImportedCount} students, skipped {result.SkippedCount}, errors {result.Errors.Count}"
        );

        return result;
    }

    private static Dictionary<string, int> GetHeaders(IXLWorksheet worksheet)
    {
        var headerRow = worksheet.Row(1);
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var cell in headerRow.CellsUsed())
        {
            var headerName = NormalizeHeader(cell.GetString());
            if (!string.IsNullOrWhiteSpace(headerName) && !map.ContainsKey(headerName))
            {
                map[headerName] = cell.Address.ColumnNumber;
            }
        }

        return map;
    }

    private static string NormalizeHeader(string header)
    {
        return header?.Trim().Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase).ToLowerInvariant() ?? string.Empty;
    }

    private static string? GetCellValue(IXLWorksheet worksheet, Dictionary<string, int> headers, string key, int rowNumber)
    {
        if (headers.TryGetValue(NormalizeHeader(key), out var column))
        {
            return worksheet.Cell(rowNumber, column).GetString().Trim();
        }

        foreach (var alias in GetAliases(key))
        {
            if (headers.TryGetValue(NormalizeHeader(alias), out column))
            {
                return worksheet.Cell(rowNumber, column).GetString().Trim();
            }
        }

        return null;
    }

    private static IEnumerable<string> GetAliases(string key)
    {
        var normalized = NormalizeHeader(key);
        yield return key;

        switch (normalized)
        {
            case "email":
                yield return "Email Address";
                break;
            case "lastname":
                yield return "Last Name";
                break;
            case "firstname":
                yield return "First Name";
                break;
            case "middlename":
                yield return "Middle Name";
                break;
            case "contactnumber":
                yield return "Contact Number";
                yield return "Contact Number (faculty-in-charge)";
                break;
            case "address":
                yield return "Address";
                break;
            case "dateofbirth":
                yield return "Age";
                break;
            case "gender":
                yield return "Gender";
                break;
            case "gradelevel":
                yield return "Grade Level of the Intern";
                yield return "Grade Level";
                break;
            case "schoolname":
                yield return "Name of School";
                break;
            case "schooladdress":
                yield return "School Address";
                break;
            case "schoolcontactperson":
                yield return "Name of the contact person (faculty-in-charge)";
                break;
            case "schoolcontactpersonemail":
                yield return "Email address contact person (faculty-in-charge)";
                break;
            case "schoolcontactpersonphone":
                yield return "Contact Number (faculty-in-charge)";
                break;
            case "internshipnature":
                yield return "Nature of internship";
                yield return "On-the-Job-Training";
                yield return "Work Immersion";
                yield return "OJT";
                yield return "Apprenticeship";
                yield return "Internship";
                break;
            case "strand":
                yield return "Course Strand (for Senior High School)";
                break;
            case "degree":
                yield return "Course Degree (for college students)";
                break;
            case "totalinternshiphours":
                yield return "Total number of internship hours";
                break;
            case "officedeployment":
                yield return "Office Deployment";
                break;
            case "internshipstartdate":
                yield return "Internship Start Date";
                break;
            case "estimatedinternshipenddate":
                yield return "Estimated Internship End Date";
                break;
        }
    }

    private static bool IsRowEmpty(IXLRow row)
    {
        return !row.CellsUsed().Any(cell => !string.IsNullOrWhiteSpace(cell.GetString()));
    }

    private Student BuildStudentFromRow(IXLWorksheet worksheet, Dictionary<string, int> headers, int rowNumber, out string rowEmail)
    {
        rowEmail = GetCellValue(worksheet, headers, "Email", rowNumber) ?? string.Empty;

        var email = rowEmail;
        var lastName = GetRequiredString(worksheet, headers, "LastName", rowNumber);
        var firstName = GetRequiredString(worksheet, headers, "FirstName", rowNumber);
        var middleName = GetCellValue(worksheet, headers, "MiddleName", rowNumber) ?? string.Empty;
        var contactNumberRaw = GetCellValue(worksheet, headers, "ContactNumber", rowNumber);
        var contactNumber = string.IsNullOrWhiteSpace(contactNumberRaw) ? string.Empty : contactNumberRaw.Trim();
        var address = GetRequiredString(worksheet, headers, "Address", rowNumber);
        var dateOfBirth = GetOptionalDate(worksheet, headers, "DateOfBirth", rowNumber);
        var gender = ParseEnum<GennderEnum>(GetRequiredString(worksheet, headers, "Gender", rowNumber), "Gender", rowNumber);
        var gradeLevelRaw = GetCellValue(worksheet, headers, "GradeLevel", rowNumber);
        var gradeLevel = string.IsNullOrWhiteSpace(gradeLevelRaw)
            ? GradeLevelEnum.SeniorHighSchool
            : ParseGradeLevel(gradeLevelRaw, "GradeLevel", rowNumber);
        var schoolName = GetRequiredString(worksheet, headers, "SchoolName", rowNumber);
        var schoolAddress = GetRequiredString(worksheet, headers, "SchoolAddress", rowNumber);
        var schoolContactPerson = GetCellValue(worksheet, headers, "SchoolContactPerson", rowNumber) ?? string.Empty;
        var schoolContactPersonEmail = GetCellValue(worksheet, headers, "SchoolContactPersonEmail", rowNumber) ?? string.Empty;
        var schoolContactPersonPhone = GetCellValue(worksheet, headers, "SchoolContactPersonPhone", rowNumber) ?? string.Empty;
        var internshipNatureRaw = GetCellValue(worksheet, headers, "InternshipNature", rowNumber);
        var internshipNature = string.IsNullOrWhiteSpace(internshipNatureRaw)
            ? InternshipNatureEnum.OnTheJobTraining
            : ParseEnum<InternshipNatureEnum>(internshipNatureRaw, "InternshipNature", rowNumber);
        var strandRaw = GetCellValue(worksheet, headers, "Strand", rowNumber);
        var degreeRaw = GetCellValue(worksheet, headers, "Degree", rowNumber);

        string? strand = null;
        string? degree = null;

        if (gradeLevel == GradeLevelEnum.SeniorHighSchool)
        {
            strand = string.IsNullOrWhiteSpace(strandRaw) ? null : strandRaw;
            degree = null;
        }
        else if (gradeLevel == GradeLevelEnum.College)
        {
            strand = null;
            degree = string.IsNullOrWhiteSpace(degreeRaw) ? null : degreeRaw;
        }

        var parsedStrand = string.IsNullOrWhiteSpace(strand) ? null : (StrandEnum?)ParseEnumOrDefault(strand, StrandEnum.STEM);
        var parsedDegree = string.IsNullOrWhiteSpace(degree) ? null : (DegreeEnum?)ParseEnumOrDefault(degree, DegreeEnum.BSIT);
        var totalInternshipHours = GetFlexibleInt(worksheet, headers, "TotalInternshipHours", rowNumber);

        ValidateEmail(email, rowNumber);
        if (!string.IsNullOrWhiteSpace(contactNumber))
        {
            var normalizedContact = NormalizePhone(contactNumber);
            if (string.IsNullOrEmpty(normalizedContact))
            {
                throw new ArgumentException($"ContactNumber '{contactNumber}' is not a valid phone number in row {rowNumber}.");
            }
            contactNumber = normalizedContact;
        }
        else
        {
            contactNumber = string.Empty;
        }
        if (!string.IsNullOrWhiteSpace(schoolContactPersonEmail) && schoolContactPersonEmail.Contains("@"))
        {
            ValidateEmail(schoolContactPersonEmail, rowNumber, "SchoolContactPersonEmail");
        }
        else if (!string.IsNullOrWhiteSpace(schoolContactPersonEmail) && !IsPlaceholder(schoolContactPersonEmail))
        {
            schoolContactPersonEmail = string.Empty;
        }
        if (!string.IsNullOrWhiteSpace(schoolContactPersonPhone))
        {
            var normalizedPhone = NormalizePhone(schoolContactPersonPhone);
            if (!string.IsNullOrEmpty(normalizedPhone))
            {
                schoolContactPersonPhone = normalizedPhone;
            }
            else if (!IsPlaceholder(schoolContactPersonPhone))
            {
                schoolContactPersonPhone = string.Empty;
            }
        }

        return new Student
        {
            Email = email,
            LastName = lastName,
            FirstName = firstName,
            MiddleName = middleName,
            ContactNumber = contactNumber,
            Address = address,
            DateOfBirth = dateOfBirth,
            Gender = gender,
            GradeLevel = gradeLevel,
            SchoolName = schoolName,
            SchoolAddress = schoolAddress,
            SchoolContactPerson = schoolContactPerson,
            SchoolContactPersonEmail = schoolContactPersonEmail,
            SchoolContactPersonPhone = schoolContactPersonPhone,
            InternshipNature = internshipNature,
            Strand = parsedStrand,
            Degree = parsedDegree,
            TotalInternshipHours = totalInternshipHours,
            Application = new SIISMinimalAPI.Features.Shared.Models.Application
            {
                Uuid = Guid.NewGuid(),
                Status = ApplicationStatusEnum.Approved
            }
        };
    }

    private string GetRequiredString(IXLWorksheet worksheet, Dictionary<string, int> headers, string key, int rowNumber)
    {
        var value = GetCellValue(worksheet, headers, key, rowNumber);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{key} is required in row {rowNumber}.");
        }

        return value;
    }

    private DateOnly GetOptionalDate(IXLWorksheet worksheet, Dictionary<string, int> headers, string key, int rowNumber)
    {
        var raw = GetCellValue(worksheet, headers, key, rowNumber);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return DateOnly.FromDateTime(DateTime.Today.AddYears(-18));
        }

        if (DateOnly.TryParse(raw, out var dateValue))
        {
            return dateValue;
        }

        if (DateTime.TryParse(raw, out var dateTime))
        {
            return DateOnly.FromDateTime(dateTime);
        }

        if (int.TryParse(raw, out var age) && age > 0 && age < 150)
        {
            return DateOnly.FromDateTime(DateTime.Today.AddYears(-age));
        }

        return DateOnly.FromDateTime(DateTime.Today.AddYears(-18));
    }

    private DateOnly GetRequiredDate(IXLWorksheet worksheet, Dictionary<string, int> headers, string key, int rowNumber)
    {
        var raw = GetCellValue(worksheet, headers, key, rowNumber);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return DateOnly.FromDateTime(DateTime.Today.AddDays(7));
        }

        if (DateOnly.TryParse(raw, out var dateValue))
        {
            return dateValue;
        }

        if (DateTime.TryParse(raw, out var dateTime))
        {
            return DateOnly.FromDateTime(dateTime);
        }

        return DateOnly.FromDateTime(DateTime.Today.AddDays(7));
    }

    private static string ExtractOfficeName(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        var trimmed = raw.Trim();
        var separatorIndex = trimmed.LastIndexOf(" - ", StringComparison.Ordinal);
        if (separatorIndex >= 0 && separatorIndex < trimmed.Length - 3)
        {
            return trimmed[(separatorIndex + 3)..].Trim();
        }

        return trimmed;
    }

    private int GetRequiredInt(IXLWorksheet worksheet, Dictionary<string, int> headers, string key, int rowNumber)
    {
        var raw = GetRequiredString(worksheet, headers, key, rowNumber);
        if (TryParseFlexibleInt(raw, out var intValue))
        {
            return intValue;
        }

        throw new ArgumentException($"{key} has invalid number format in row {rowNumber}.");
    }

    private int GetFlexibleInt(IXLWorksheet worksheet, Dictionary<string, int> headers, string key, int rowNumber)
    {
        var raw = GetCellValue(worksheet, headers, key, rowNumber);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return 0;
        }

        if (TryParseFlexibleInt(raw, out var intValue))
        {
            return intValue;
        }

        return 0;
    }

    private static bool TryParseFlexibleInt(string raw, out int intValue)
    {
        var cleaned = new string(raw.Trim().Where(c => char.IsDigit(c) || c == '.').ToArray());

        if (int.TryParse(cleaned, out intValue))
        {
            return true;
        }

        if (double.TryParse(cleaned, out var doubleValue))
        {
            intValue = (int)Math.Round(doubleValue);
            return true;
        }

        intValue = 0;
        return false;
    }

    private static GradeLevelEnum ParseGradeLevel(string? raw, string displayName, int rowNumber)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new ArgumentException($"{displayName} is required in row {rowNumber}.");
        }

        var trimmed = raw.Trim();
        if (Enum.TryParse<GradeLevelEnum>(trimmed, true, out var value))
        {
            return value;
        }

        switch (trimmed.ToLowerInvariant())
        {
            case "seniorhighschool":
            case "senior high school":
            case "senior high":
            case "shs":
            case "0":
                return GradeLevelEnum.SeniorHighSchool;
            case "college":
            case "university":
            case "tertiary":
            case "1":
                return GradeLevelEnum.College;
            default:
                throw new ArgumentException($"Invalid {displayName} value '{raw}' in row {rowNumber}.");
        }
    }

    private static T ParseEnum<T>(string? raw, string displayName, int rowNumber) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new ArgumentException($"{displayName} is required in row {rowNumber}.");
        }

        var normalized = NormalizeEnumInput(raw);
        if (Enum.TryParse<T>(normalized, true, out var value))
        {
            return value;
        }

        if (int.TryParse(raw.Trim(), out var numericValue) && Enum.IsDefined(typeof(T), numericValue))
        {
            return (T)Enum.ToObject(typeof(T), numericValue);
        }

        throw new ArgumentException($"Invalid {displayName} value '{raw}' in row {rowNumber}.");
    }

    private static T ParseEnumOrDefault<T>(string? raw, T defaultValue) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return defaultValue;
        }

        var normalized = NormalizeEnumInput(raw);
        if (Enum.TryParse<T>(normalized, true, out var value))
        {
            return value;
        }

        if (int.TryParse(raw.Trim(), out var numericValue) && Enum.IsDefined(typeof(T), numericValue))
        {
            return (T)Enum.ToObject(typeof(T), numericValue);
        }

        return defaultValue;
    }

    private static string NormalizeEnumInput(string raw)
    {
        return new string(raw.Trim().Where(c => char.IsLetterOrDigit(c)).ToArray());
    }

    private static void ValidateEmail(string? email, int rowNumber, string fieldName = "Email")
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException($"{fieldName} is required in row {rowNumber}.");
        }

        var trimmed = email.Trim();
        if (IsPlaceholder(trimmed))
        {
            return;
        }

        if (!new EmailAddressAttribute().IsValid(trimmed))
        {
            throw new ArgumentException($"{fieldName} '{email}' is not a valid email in row {rowNumber}.");
        }
    }

    private static void ValidatePhone(string? phone, int rowNumber, string fieldName = "ContactNumber")
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new ArgumentException($"{fieldName} is required in row {rowNumber}.");
        }

        if (!IsValidPhone(phone))
        {
            throw new ArgumentException($"{fieldName} '{phone}' is not a valid Philippine mobile number in row {rowNumber}.");
        }
    }

    private static void ValidatePhoneLenient(string? phone, int rowNumber, string fieldName = "ContactNumber")
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return;
        }

        var trimmed = phone.Trim();
        if (IsPlaceholder(trimmed))
        {
            return;
        }

        if (!IsValidPhone(trimmed))
        {
            throw new ArgumentException($"{fieldName} '{phone}' is not a valid Philippine mobile number in row {rowNumber}.");
        }
    }

    private static string NormalizePhone(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        var trimmed = raw.Trim();
        
        if (trimmed.Contains('/'))
        {
            var parts = trimmed.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(p => NormalizePhone(p))
                .Where(p => !string.IsNullOrEmpty(p))
                .ToArray();
            
            return parts.Length > 0 ? parts[0] : string.Empty;
        }

        var digits = new string(trimmed.Where(char.IsDigit).ToArray());

        if (digits.Length == 12 && digits.StartsWith("63"))
        {
            digits = "0" + digits[2..];
        }

        if (digits.Length == 12 && digits.StartsWith("09"))
        {
            return digits;
        }

        if (digits.Length == 11 && digits.StartsWith("63"))
        {
            digits = "0" + digits[2..];
        }

        if (digits.Length == 11 && digits.StartsWith("09"))
        {
            return digits;
        }

        if (digits.Length == 10 && digits.StartsWith("9"))
        {
            digits = "0" + digits;
        }

        if (digits.Length == 11 && digits.StartsWith("09"))
        {
            return digits;
        }

        return string.Empty;
    }

    private static bool IsPlaceholder(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var normalized = value.Trim().ToLowerInvariant();
        return normalized is "n/a" or "na" or "none" or "." or "n/a" or "na" or "-" or "—";
    }

    private static bool IsValidPhone(string phone)
    {
        var digits = new string(phone.Trim().Where(char.IsDigit).ToArray());
        
        if (digits.Length == 12 && digits.StartsWith("63"))
        {
            digits = "0" + digits[2..];
        }

        if (digits.Length == 12 && digits.StartsWith("09"))
        {
            return true;
        }

        if (digits.Length == 11 && digits.StartsWith("63"))
        {
            digits = "0" + digits[2..];
        }

        if (digits.Length == 11 && digits.StartsWith("09"))
        {
            return true;
        }

        if (digits.Length == 10 && digits.StartsWith("9"))
        {
            digits = "0" + digits;
        }

        if (digits.Length == 11 && digits.StartsWith("09"))
        {
            return true;
        }

        return false;
    }

    private static string NormalizeSchoolName(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        var trimmed = raw.Trim();
        var words = trimmed.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
        var normalizedWords = words.Select(w => char.ToUpperInvariant(w[0]) + w.Substring(1).ToLowerInvariant());
        return string.Join(" ", normalizedWords);
    }

    private static string GetSchoolNameFingerprint(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        var normalized = raw.ToLowerInvariant();
        var words = normalized.Split(new[] {' ', ',', '.', '-', '/', '&', '(', ')'}, StringSplitOptions.RemoveEmptyEntries);
        Array.Sort(words, StringComparer.Ordinal);
        return string.Join(" ", words);
    }

    private static double GetSchoolNameSimilarity(string school1, string school2)
    {
        if (string.IsNullOrWhiteSpace(school1) || string.IsNullOrWhiteSpace(school2))
        {
            return 0.0;
        }

        var words1 = new HashSet<string>(school1.ToLowerInvariant().Split(new[] {' ', ',', '.', '-', '/', '&', '(', ')'}, StringSplitOptions.RemoveEmptyEntries), StringComparer.Ordinal);
        var words2 = new HashSet<string>(school2.ToLowerInvariant().Split(new[] {' ', ',', '.', '-', '/', '&', '(', ')'}, StringSplitOptions.RemoveEmptyEntries), StringComparer.Ordinal);

        if (words1.Count == 0 && words2.Count == 0)
        {
            return 1.0;
        }

        var intersection = new HashSet<string>(words1, StringComparer.Ordinal);
        intersection.IntersectWith(words2);

        var union = new HashSet<string>(words1, StringComparer.Ordinal);
        union.UnionWith(words2);

        return (double)intersection.Count / union.Count;
    }
}
