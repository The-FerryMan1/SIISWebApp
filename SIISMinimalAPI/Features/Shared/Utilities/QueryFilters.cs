using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Features.Shared.Enums;
using SIISMinimalAPI.Features.Shared.Models;

namespace SIISMinimalAPI.Features.Shared.Utilities;

public static class QueryFilterExtensions
{
    public static IQueryable<Student> ApplyFilters(this IQueryable<Student> query, CommonFilterOptions filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.Name))
        {
            var name = filters.Name.Trim();
            query = query.Where(t =>
                (t.FirstName != null && t.FirstName.Contains(name)) ||
                (t.LastName != null && t.LastName.Contains(name)));
        }

        if (!string.IsNullOrWhiteSpace(filters.School))
        {
            var school = filters.School.Trim();
            query = query.Where(t => t.SchoolName != null && t.SchoolName.Contains(school));
        }

        if (filters.DateFrom.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= filters.DateFrom.Value);
        }

        if (filters.DateTo.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= filters.DateTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.Office))
        {
            var office = filters.Office.Trim();
            query = query.Where(t => t.Placement != null && t.Placement!.Office != null && t.Placement.Office.OfficeName == office);
        }

        if (!string.IsNullOrWhiteSpace(filters.Status) && Enum.TryParse<ApplicationStatusEnum>(filters.Status, true, out var status))
        {
            query = query.Where(t => t.Application != null && t.Application.Status == status);
        }

        return query;
    }

    public static IQueryable<Models.Application> ApplyFilters(this IQueryable<Models.Application> query, CommonFilterOptions filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.Name))
        {
            var name = filters.Name.Trim();
            query = query.Where(t =>
                (t.Student != null && t.Student.FirstName != null && t.Student.FirstName.Contains(name)) ||
                (t.Student != null && t.Student.LastName != null && t.Student.LastName.Contains(name)));
        }

        if (!string.IsNullOrWhiteSpace(filters.School))
        {
            var school = filters.School.Trim();
            query = query.Where(t => t.Student != null && t.Student.SchoolName != null && t.Student.SchoolName.Contains(school));
        }

        if (filters.DateFrom.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= filters.DateFrom.Value);
        }

        if (filters.DateTo.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= filters.DateTo.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.Status) && Enum.TryParse<ApplicationStatusEnum>(filters.Status, true, out var status))
        {
            query = query.Where(t => t.Status == status);
        }

        return query;
    }

    public static IQueryable<Placement> ApplyFilters(this IQueryable<Placement> query, CommonFilterOptions filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.Name))
        {
            var name = filters.Name.Trim();
            query = query.Where(t =>
                (t.Student != null && t.Student.FirstName != null && t.Student.FirstName.Contains(name)) ||
                (t.Student != null && t.Student.LastName != null && t.Student.LastName.Contains(name)));
        }

        if (!string.IsNullOrWhiteSpace(filters.School))
        {
            var school = filters.School.Trim();
            query = query.Where(t => t.Student != null && t.Student.SchoolName != null && t.Student.SchoolName.Contains(school));
        }

        if (filters.DateFrom.HasValue)
        {
            var dateOnly = DateOnly.FromDateTime(filters.DateFrom.Value);
            query = query.Where(t => t.StartDate >= dateOnly);
        }

        if (filters.DateTo.HasValue)
        {
            var dateOnly = DateOnly.FromDateTime(filters.DateTo.Value);
            query = query.Where(t => t.StartDate <= dateOnly);
        }

        if (!string.IsNullOrWhiteSpace(filters.Office))
        {
            var office = filters.Office.Trim();
            query = query.Where(t => t.Office != null && t.Office.OfficeName == office);
        }

        return query;
    }

    public static IQueryable<Requirement> ApplyFilters(this IQueryable<Requirement> query, CommonFilterOptions filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.Name))
        {
            var name = filters.Name.Trim();
            query = query.Where(t =>
                (t.Student != null && t.Student.FirstName != null && t.Student.FirstName.Contains(name)) ||
                (t.Student != null && t.Student.LastName != null && t.Student.LastName.Contains(name)));
        }

        if (!string.IsNullOrWhiteSpace(filters.School))
        {
            var school = filters.School.Trim();
            query = query.Where(t => t.Student != null && t.Student.SchoolName != null && t.Student.SchoolName.Contains(school));
        }

        if (filters.DateFrom.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= filters.DateFrom.Value);
        }

        if (filters.DateTo.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= filters.DateTo.Value);
        }

        return query;
    }

    public static IQueryable<Office> ApplyFilters(this IQueryable<Office> query, CommonFilterOptions filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.Name))
        {
            var name = filters.Name.Trim();
            query = query.Where(t => t.OfficeName != null && t.OfficeName.Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(filters.Office))
        {
            var office = filters.Office.Trim();
            query = query.Where(t => t.OfficeName != null && t.OfficeName.Contains(office));
        }

        if (filters.DateFrom.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= filters.DateFrom.Value);
        }

        if (filters.DateTo.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= filters.DateTo.Value);
        }

        return query;
    }
}
