using SIISMinimalAPI.Features.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace SIISMinimalAPI.Features.Shared.Models
{
    public class OfficeModel
    {
        public long Id { get; set; }

        public OfficeNameEnum Name { get; set; }

        [StringLength(255)]
        public string? CurrentOIC { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public ICollection<StudentModel> Students { get; set; } = [];
    }
}
