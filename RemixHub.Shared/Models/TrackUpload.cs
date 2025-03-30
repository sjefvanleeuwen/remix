using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace RemixHub.Shared.Models
{
    public class TrackUpload
    {
        [Required(ErrorMessage = "Please select a track file")]
        public IFormFile TrackFile { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
        public string Title { get; set; }
        
        [Required(ErrorMessage = "Artist name is required")]
        [StringLength(100, ErrorMessage = "Artist name cannot be longer than 100 characters")]
        public string Artist { get; set; }
        
        [StringLength(100, ErrorMessage = "Album name cannot be longer than 100 characters")]
        public string Album { get; set; }
        
        [Required(ErrorMessage = "Genre is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a genre")]
        public int GenreId { get; set; }
        
        public int? SubgenreId { get; set; }
        
        public string Description { get; set; }
        
        [Range(1, 999, ErrorMessage = "BPM must be between 1 and 999")]
        public int? Bpm { get; set; }
        
        [StringLength(10, ErrorMessage = "Musical key cannot be longer than 10 characters")]
        public string MusicalKey { get; set; }
    }

    public class StemUpload
    {
        [Required(ErrorMessage = "Please select a stem file")]
        public IFormFile StemFile { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Instrument type is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select an instrument type")]
        public int InstrumentTypeId { get; set; }

        public string Description { get; set; }
    }

    public class RemixCreate
    {
        [Required(ErrorMessage = "Please select a remix file")]
        public IFormFile RemixFile { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Please explain your remix approach")]
        public string RemixReason { get; set; }

        public string StemsUsed { get; set; }

        public int? GenreId { get; set; }

        public int? SubgenreId { get; set; }

        public string Description { get; set; }
    }
}
