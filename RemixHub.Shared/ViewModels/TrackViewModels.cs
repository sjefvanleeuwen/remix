using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RemixHub.Shared.ViewModels
{
    public class TrackFilterViewModel
    {
        public string Keyword { get; set; }
        public int? GenreId { get; set; }
        public int? MinBpm { get; set; }
        public int? MaxBpm { get; set; }
        public int? MinDuration { get; set; } // In minutes
        public int? MaxDuration { get; set; } // In minutes
        public string Key { get; set; }
        public int? InstrumentTypeId { get; set; }
        public DateTime? FromDate { get; set; }
        public string SortBy { get; set; } = "newest"; // newest, oldest, title, artist
        public int? Page { get; set; } = 1;
        public int? PageSize { get; set; } = 20;
    }

    public class TracksResponseViewModel
    {
        public List<TrackViewModel> Tracks { get; set; } = new List<TrackViewModel>();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }

    public class TrackViewModel
    {
        public int TrackId { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public int? GenreId { get; set; } // Changed from int to int? if it wasn't already
        public string GenreName { get; set; }
        public int? SubgenreId { get; set; }
        public string SubgenreName { get; set; }
        public string Description { get; set; }
        public int DurationSeconds { get; set; }
        public int? Bpm { get; set; }
        public bool IsApproved { get; set; } // Changed from bool to int to match the Track model
        public string MusicalKey { get; set; }
        public DateTime UploadDate { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
    }

    public class TrackDetailViewModel : TrackViewModel
    {
        public int BitRate { get; set; }
        public int SampleRate { get; set; }
        public List<StemViewModel> Stems { get; set; } = new List<StemViewModel>();
        public List<RemixViewModel> Remixes { get; set; } = new List<RemixViewModel>();
        public RemixDetailViewModel OriginalTrack { get; set; }
    }

    public class RemixDetailViewModel
    {
        public int TrackId { get; set; }
        public string Title { get; set; }
        public string UserName { get; set; }
        public string RemixReason { get; set; }
        public string StemsUsed { get; set; }
    }

    public class RemixViewModel
    {
        public int TrackId { get; set; }
        public string Title { get; set; }
        public string UserName { get; set; }
        public string RemixReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class StemViewModel
    {
        public int StemId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int InstrumentTypeId { get; set; }
        public string InstrumentTypeName { get; set; }
    }

    public class TrackUploadViewModel
    {
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
        
        [Required(ErrorMessage = "Please select a track file")]
        public object TrackFile { get; set; }
    }

    public class StemUploadViewModel
    {
        [Required(ErrorMessage = "Please select a stem file")]
        public object StemFile { get; set; } // Changed from IFormFile to object

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Instrument type is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select an instrument type")]
        public int InstrumentTypeId { get; set; }

        public string Description { get; set; }
    }

    public class RemixCreateViewModel
    {
        [Required(ErrorMessage = "Please select a remix file")]
        public object RemixFile { get; set; } // Changed from IFormFile to object

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; }

        public int? GenreId { get; set; }
        
        public int? SubgenreId { get; set; }

        [Required(ErrorMessage = "Please explain what inspired your remix")]
        public string RemixReason { get; set; }

        public string StemsUsed { get; set; }

        public string Description { get; set; }
    }

    // Client-side versions of the upload models with browser-compatible file types
    namespace Client
    {
        // These classes are used only on the client for Blazor UI file handling
        // The regular models (using IFormFile) are used for server API endpoints

        public class TrackUploadViewModel
        {
            [Required(ErrorMessage = "Please select a track file")]
            public object TrackFile { get; set; } // Will be IBrowserFile on client

            [Required(ErrorMessage = "Title is required")]
            [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
            public string Title { get; set; }

            [Required(ErrorMessage = "Artist name is required")]
            [StringLength(100, ErrorMessage = "Artist name cannot exceed 100 characters")]
            public string Artist { get; set; }

            public string Album { get; set; }

            [Required(ErrorMessage = "Genre is required")]
            [Range(1, int.MaxValue, ErrorMessage = "Please select a genre")]
            public int GenreId { get; set; }

            public int? SubgenreId { get; set; }

            public string Description { get; set; }

            public int? Bpm { get; set; }

            public string MusicalKey { get; set; }
        }

        public class StemUploadViewModel
        {
            [Required(ErrorMessage = "Please select a stem file")]
            public object StemFile { get; set; } // Will be IBrowserFile on client

            [Required(ErrorMessage = "Name is required")]
            [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
            public string Name { get; set; }

            [Required(ErrorMessage = "Instrument type is required")]
            [Range(1, int.MaxValue, ErrorMessage = "Please select an instrument type")]
            public int InstrumentTypeId { get; set; }

            public string Description { get; set; }
        }

        public class RemixCreateViewModel
        {
            [Required(ErrorMessage = "Please select a remix file")]
            public object RemixFile { get; set; } // Will be IBrowserFile on client

            [Required(ErrorMessage = "Title is required")]
            [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
            public string Title { get; set; }

            public int? GenreId { get; set; }
            
            public int? SubgenreId { get; set; }

            [Required(ErrorMessage = "Please explain what inspired your remix")]
            public string RemixReason { get; set; }

            public string StemsUsed { get; set; }

            public string Description { get; set; }
        }
    }
}
