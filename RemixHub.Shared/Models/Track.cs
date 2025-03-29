using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RemixHub.Shared.Models
{
    public class Track
    {
        [Key]
        public int TrackId { get; set; }
        
        [Required, StringLength(100)]
        public string Title { get; set; }
        
        [StringLength(100)]
        public string Artist { get; set; }
        
        [StringLength(100)]
        public string Album { get; set; }
        
        [Required]
        public int GenreId { get; set; }
        
        public int? SubgenreId { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        [Required]
        public string FilePath { get; set; }
        
        public long FileSize { get; set; }
        
        [StringLength(10)]
        public string FileFormat { get; set; }
        
        public int DurationSeconds { get; set; }
        
        public int? Bpm { get; set; }
        
        [StringLength(10)]
        public string MusicalKey { get; set; }
        
        public int BitRate { get; set; }
        
        public int SampleRate { get; set; }
        
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
        
        public bool IsApproved { get; set; }
        
        public DateTime? ApprovedDate { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        
        [ForeignKey("GenreId")]
        public virtual Genre Genre { get; set; }
        
        [ForeignKey("SubgenreId")]
        public virtual Genre Subgenre { get; set; }
        
        public virtual ICollection<Stem> Stems { get; set; } = new List<Stem>();
        
        // Relationships for remixes
        public virtual ICollection<Remix> RemixesOfThis { get; set; } = new List<Remix>();
        
        [InverseProperty("OriginalTrack")]
        public virtual ICollection<Remix> Remixes { get; set; } = new List<Remix>();
    }
}
