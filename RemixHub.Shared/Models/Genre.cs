using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RemixHub.Shared.Models
{
    public class Genre
    {
        [Key]
        public int GenreId { get; set; }
        
        [Required, StringLength(50)]
        public string Name { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        public int? ParentGenreId { get; set; }
        
        // Navigation properties
        [ForeignKey("ParentGenreId")]
        public virtual Genre ParentGenre { get; set; }
        
        public virtual ICollection<Genre> Subgenres { get; set; } = new List<Genre>();
        
        public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();
        
        [InverseProperty("Subgenre")]
        public virtual ICollection<Track> SubgenreTracks { get; set; } = new List<Track>();
    }
}
