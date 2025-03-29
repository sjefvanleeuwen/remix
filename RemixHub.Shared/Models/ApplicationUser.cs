using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace RemixHub.Shared.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayName { get; set; }
        public string Bio { get; set; }
        public string AvatarUrl { get; set; }
        public string SocialLinks { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastActive { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();
    }
}
