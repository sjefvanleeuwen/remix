using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RemixHub.Shared.Models
{
    public class Remix
    {
        [Key]
        public int RemixId { get; set; }
        
        [Required]
        public int OriginalTrackId { get; set; }
        
        [Required]
        public int RemixTrackId { get; set; }
        
        [Required, StringLength(500)]
        public string RemixReason { get; set; }
        
        [StringLength(500)]
        public string StemsUsed { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("OriginalTrackId")]
        public virtual Track OriginalTrack { get; set; }
        
        [ForeignKey("RemixTrackId")]
        public virtual Track RemixTrack { get; set; }
    }
}
