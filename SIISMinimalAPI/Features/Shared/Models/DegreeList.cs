namespace SIISMinimalAPI.Features.Shared.Models
{
   public class DegreeList
    {
        public long Id { get; set; }
        public string DegreeName { get; set; } = null!;
        public string NormalizedDegreeName { get; set; } = null!;
        
    }
}