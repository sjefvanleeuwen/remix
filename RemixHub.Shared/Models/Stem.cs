using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RemixHub.Shared.Models
{
    public class Stem
    {
        [Key]
        public int StemId { get; set; }
        
        [Required]
        public int TrackId { get; set; }
        
        [Required, StringLength(100)]
        public string Name { get; set; }
        
        [Required]
        public int InstrumentTypeId { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        [Required]
        public string FilePath { get; set; }
        
        public int DurationSeconds { get; set; }
        
        public long FileSize { get; set; }
        
        [MaxLength(10)]
        public string FileFormat { get; set; }
        
        [Required]
        public DateTime UploadDate { get; set; }
        
        // Navigation properties
        [ForeignKey("TrackId")]
        public virtual Track Track { get; set; }
        
        [ForeignKey("InstrumentTypeId")]
        public virtual InstrumentType InstrumentType { get; set; }
    }
}
