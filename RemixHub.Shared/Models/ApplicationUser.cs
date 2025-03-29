using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace RemixHub.Shared.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Make DisplayName, Bio, AvatarUrl, and SocialLinks nullable
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? SocialLinks { get; set; }
        
        // Add IsVerified property
        public bool IsVerified { get; set; }
        
        // Keep required fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastActive { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();
        public virtual ICollection<Remix> Remixes { get; set; } = new List<Remix>();
    }
}
