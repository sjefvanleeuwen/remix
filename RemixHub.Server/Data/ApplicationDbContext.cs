using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RemixHub.Shared.Models;

namespace RemixHub.Server.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Track> Tracks { get; set; }
        public DbSet<Stem> Stems { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<InstrumentType> InstrumentTypes { get; set; }
        public DbSet<Remix> Remixes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Track relationships
            builder.Entity<Track>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tracks)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Track entity relationships for primary genre
            builder.Entity<Track>()
                .HasOne(t => t.Genre)
                .WithMany(g => g.Tracks)
                .HasForeignKey(t => t.GenreId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Track entity relationships for subgenre
            builder.Entity<Track>()
                .HasOne(t => t.Subgenre)
                .WithMany(g => g.SubgenreTracks)
                .HasForeignKey(t => t.SubgenreId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Remix relationships
            builder.Entity<Remix>()
                .HasOne(r => r.OriginalTrack)
                .WithMany(t => t.RemixesOfThis)
                .HasForeignKey(r => r.OriginalTrackId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Remix>()
                .HasOne(r => r.RemixTrack)
                .WithOne()
                .HasForeignKey<Remix>(r => r.RemixTrackId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Genre entity relationships
            builder.Entity<Genre>()
                .HasOne(g => g.ParentGenre)
                .WithMany(g => g.Subgenres)
                .HasForeignKey(g => g.ParentGenreId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure InstrumentType entity relationships
            builder.Entity<InstrumentType>()
                .HasOne(i => i.ParentInstrumentType)
                .WithMany(i => i.SubInstrumentTypes)
                .HasForeignKey(i => i.ParentInstrumentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed initial data
            SeedData(builder);
        }

        private void SeedData(ModelBuilder builder)
        {
            // Seed main genres
            builder.Entity<Genre>().HasData(
                new Genre { GenreId = 1, Name = "Electronic", Description = "Electronic music" },
                new Genre { GenreId = 2, Name = "Rock", Description = "Rock music" },
                new Genre { GenreId = 3, Name = "Hip-Hop", Description = "Hip-Hop music" },
                new Genre { GenreId = 4, Name = "Jazz", Description = "Jazz music" },
                new Genre { GenreId = 5, Name = "Classical", Description = "Classical music" }
            );

            // Seed subgenres
            builder.Entity<Genre>().HasData(
                new Genre { GenreId = 6, Name = "House", Description = "House music", ParentGenreId = 1 },
                new Genre { GenreId = 7, Name = "Techno", Description = "Techno music", ParentGenreId = 1 },
                new Genre { GenreId = 8, Name = "Alternative Rock", Description = "Alternative rock music", ParentGenreId = 2 },
                new Genre { GenreId = 9, Name = "Trap", Description = "Trap music", ParentGenreId = 3 }
            );

            // Seed instrument categories
            builder.Entity<InstrumentType>().HasData(
                new InstrumentType { InstrumentTypeId = 1, Name = "Drums", Description = "Percussion instruments" },
                new InstrumentType { InstrumentTypeId = 2, Name = "Bass", Description = "Bass instruments" },
                new InstrumentType { InstrumentTypeId = 3, Name = "Keys", Description = "Keyboard instruments" },
                new InstrumentType { InstrumentTypeId = 4, Name = "Guitar", Description = "Guitar instruments" },
                new InstrumentType { InstrumentTypeId = 5, Name = "Vocals", Description = "Vocal tracks" },
                new InstrumentType { InstrumentTypeId = 6, Name = "Synth", Description = "Synthesizers" }
            );

            // Seed instrument subcategories
            builder.Entity<InstrumentType>().HasData(
                new InstrumentType { InstrumentTypeId = 7, Name = "Kick", Description = "Bass drum", ParentInstrumentTypeId = 1 },
                new InstrumentType { InstrumentTypeId = 8, Name = "Snare", Description = "Snare drum", ParentInstrumentTypeId = 1 },
                new InstrumentType { InstrumentTypeId = 9, Name = "Hi-hat", Description = "Hi-hat cymbal", ParentInstrumentTypeId = 1 },
                new InstrumentType { InstrumentTypeId = 10, Name = "Bass Guitar", Description = "Electric bass guitar", ParentInstrumentTypeId = 2 },
                new InstrumentType { InstrumentTypeId = 11, Name = "Synth Bass", Description = "Synthesized bass", ParentInstrumentTypeId = 2 },
                new InstrumentType { InstrumentTypeId = 12, Name = "Piano", Description = "Acoustic or electric piano", ParentInstrumentTypeId = 3 },
                new InstrumentType { InstrumentTypeId = 13, Name = "Electric Guitar", Description = "Electric guitar", ParentInstrumentTypeId = 4 },
                new InstrumentType { InstrumentTypeId = 14, Name = "Acoustic Guitar", Description = "Acoustic guitar", ParentInstrumentTypeId = 4 },
                new InstrumentType { InstrumentTypeId = 15, Name = "Lead Vocals", Description = "Main vocal track", ParentInstrumentTypeId = 5 },
                new InstrumentType { InstrumentTypeId = 16, Name = "Backing Vocals", Description = "Background vocals", ParentInstrumentTypeId = 5 },
                new InstrumentType { InstrumentTypeId = 17, Name = "Lead Synth", Description = "Lead synthesizer", ParentInstrumentTypeId = 6 },
                new InstrumentType { InstrumentTypeId = 18, Name = "Pad", Description = "Synthesizer pad", ParentInstrumentTypeId = 6 }
            );
        }
    }
}
