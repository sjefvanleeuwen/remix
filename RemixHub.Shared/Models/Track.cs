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

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(100)]
        public string Artist { get; set; }

        [StringLength(100)]
        public string Album { get; set; }

        [ForeignKey("Genre")]
        public int? GenreId { get; set; } // Changed from int to int?

        [ForeignKey("Subgenre")]
        public int? SubgenreId { get; set; } // Already nullable

        public string Description { get; set; }

        public int? Bpm { get; set; } // Already nullable

        [StringLength(10)]
        public string MusicalKey { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTime UploadDate { get; set; }

        [Required]
        public string FilePath { get; set; }
        
        // Adding missing properties
        public long FileSize { get; set; }
        
        [StringLength(10)]
        public string FileFormat { get; set; }

        public int DurationSeconds { get; set; } // Already nullable

        public int BitRate { get; set; } // Already nullable

        public int SampleRate { get; set; } // Already nullable

        public int Status { get; set; } // Keep this non-nullable as it's an enum value
        
        // Property to determine approval status based on Status value
        [NotMapped]
        public bool IsApproved => Status == (int)TrackStatus.Approved;

        // Navigation properties
        public virtual Genre Genre { get; set; }
        public virtual Genre Subgenre { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Stem> Stems { get; set; }
        
        // For remixes
        public int? OriginalTrackId { get; set; }
        [ForeignKey("OriginalTrackId")]
        public virtual Track OriginalTrack { get; set; }
        public virtual ICollection<Track> RemixCollection { get; set; }
        
        public string RemixReason { get; set; }
        public string StemsUsed { get; set; }
        public DateTime ApprovedDate { get; set; }
    }
    
    // Enum to define possible track statuses
    public enum TrackStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }
}
