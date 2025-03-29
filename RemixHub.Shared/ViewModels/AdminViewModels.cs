using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RemixHub.Shared.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastActive { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public int TrackCount { get; set; }
        public int RemixCount { get; set; }
    }

    public class UpdateRoleViewModel
    {
        [Required]
        public string Role { get; set; }
    }

    public class GenreViewModel
    {
        public int GenreId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public int? ParentGenreId { get; set; }
        
        public string ParentGenreName { get; set; }
    }

    public class InstrumentTypeViewModel
    {
        public int InstrumentTypeId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        
        public string Description { get; set; }
        
        public int? ParentInstrumentTypeId { get; set; }
        
        public string ParentInstrumentTypeName { get; set; }
    }

    public class SiteStatsViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalTracks { get; set; }
        public int TotalRemixes { get; set; }
        public int PendingTracks { get; set; }
        public int ActiveUsersLast7Days { get; set; }
        public int NewUsersLast7Days { get; set; }
        public int NewTracksLast7Days { get; set; }
        public Dictionary<string, int> TracksByGenre { get; set; } = new Dictionary<string, int>();
    }
}
