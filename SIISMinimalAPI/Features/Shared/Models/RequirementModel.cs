namespace SIISMinimalAPI.Features.Shared.Models
{
    public class RequirementModel
    {
        public long Id { get; set; }

        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        public long StudentId { get; set; }
        public StudentModel Student { get; set; }

    }
}
