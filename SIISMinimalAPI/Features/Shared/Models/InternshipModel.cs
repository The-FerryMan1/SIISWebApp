using SIISMinimalAPI.Features.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace SIISMinimalAPI.Features.Shared.Models
{
    public class InternshipModel
    {
        public long Id { get; set; }
        [Required, EnumDataType(typeof(InternshipNatureEnum))]
        public InternshipNatureEnum InternshipNature { get; set; }
        [EnumDataType(typeof(StrandEnum))]
        public StrandEnum? Strand { get; set; }
        [EnumDataType(typeof(DegreeEnum))]
        public DegreeEnum? Degree { get; set; }
        [DataType(DataType.Date)]
        public DateOnly StartDate { get; set; }
        [DataType(DataType.Date)]
        public DateOnly EstimatedEndDate { get; set; }
        public int InternshipTotalHours { get; set; }
        public int AccumulatedHours { get; set; } = 0;

        public bool IsDeleted { get; set; } = false;
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public long StudentId { get; set; }
        public StudentModel Student { get; set; }
    }
}
