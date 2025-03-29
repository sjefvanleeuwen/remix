﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RemixHub.Server.Data;

#nullable disable

namespace RemixHub.Server.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.0-preview.4.24267.1");

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClaimType")
                        .HasColumnType("TEXT");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("TEXT");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClaimType")
                        .HasColumnType("TEXT");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<string>("RoleId")
                        .HasColumnType("TEXT");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("RemixHub.Shared.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("AvatarUrl")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Bio")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsVerified")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastActive")
                        .HasColumnType("TEXT");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("TEXT");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("TEXT");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("TEXT");

                    b.Property<string>("SocialLinks")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("RemixHub.Shared.Models.Genre", b =>
                {
                    b.Property<int>("GenreId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<int?>("ParentGenreId")
                        .HasColumnType("INTEGER");

                    b.HasKey("GenreId");

                    b.HasIndex("ParentGenreId");

                    b.ToTable("Genres");

                    b.HasData(
                        new
                        {
                            GenreId = 1,
                            Description = "Electronic music",
                            Name = "Electronic"
                        },
                        new
                        {
                            GenreId = 2,
                            Description = "Rock music",
                            Name = "Rock"
                        },
                        new
                        {
                            GenreId = 3,
                            Description = "Hip-Hop music",
                            Name = "Hip-Hop"
                        },
                        new
                        {
                            GenreId = 4,
                            Description = "Jazz music",
                            Name = "Jazz"
                        },
                        new
                        {
                            GenreId = 5,
                            Description = "Classical music",
                            Name = "Classical"
                        },
                        new
                        {
                            GenreId = 6,
                            Description = "House music",
                            Name = "House",
                            ParentGenreId = 1
                        },
                        new
                        {
                            GenreId = 7,
                            Description = "Techno music",
                            Name = "Techno",
                            ParentGenreId = 1
                        },
                        new
                        {
                            GenreId = 8,
                            Description = "Alternative rock music",
                            Name = "Alternative Rock",
                            ParentGenreId = 2
                        },
                        new
                        {
                            GenreId = 9,
                            Description = "Trap music",
                            Name = "Trap",
                            ParentGenreId = 3
                        });
                });

            modelBuilder.Entity("RemixHub.Shared.Models.InstrumentType", b =>
                {
                    b.Property<int>("InstrumentTypeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<int?>("ParentInstrumentTypeId")
                        .HasColumnType("INTEGER");

                    b.HasKey("InstrumentTypeId");

                    b.HasIndex("ParentInstrumentTypeId");

                    b.ToTable("InstrumentTypes");

                    b.HasData(
                        new
                        {
                            InstrumentTypeId = 1,
                            Description = "Percussion instruments",
                            Name = "Drums"
                        },
                        new
                        {
                            InstrumentTypeId = 2,
                            Description = "Bass instruments",
                            Name = "Bass"
                        },
                        new
                        {
                            InstrumentTypeId = 3,
                            Description = "Keyboard instruments",
                            Name = "Keys"
                        },
                        new
                        {
                            InstrumentTypeId = 4,
                            Description = "Guitar instruments",
                            Name = "Guitar"
                        },
                        new
                        {
                            InstrumentTypeId = 5,
                            Description = "Vocal tracks",
                            Name = "Vocals"
                        },
                        new
                        {
                            InstrumentTypeId = 6,
                            Description = "Synthesizers",
                            Name = "Synth"
                        },
                        new
                        {
                            InstrumentTypeId = 7,
                            Description = "Bass drum",
                            Name = "Kick",
                            ParentInstrumentTypeId = 1
                        },
                        new
                        {
                            InstrumentTypeId = 8,
                            Description = "Snare drum",
                            Name = "Snare",
                            ParentInstrumentTypeId = 1
                        },
                        new
                        {
                            InstrumentTypeId = 9,
                            Description = "Hi-hat cymbal",
                            Name = "Hi-hat",
                            ParentInstrumentTypeId = 1
                        },
                        new
                        {
                            InstrumentTypeId = 10,
                            Description = "Electric bass guitar",
                            Name = "Bass Guitar",
                            ParentInstrumentTypeId = 2
                        },
                        new
                        {
                            InstrumentTypeId = 11,
                            Description = "Synthesized bass",
                            Name = "Synth Bass",
                            ParentInstrumentTypeId = 2
                        },
                        new
                        {
                            InstrumentTypeId = 12,
                            Description = "Acoustic or electric piano",
                            Name = "Piano",
                            ParentInstrumentTypeId = 3
                        },
                        new
                        {
                            InstrumentTypeId = 13,
                            Description = "Electric guitar",
                            Name = "Electric Guitar",
                            ParentInstrumentTypeId = 4
                        },
                        new
                        {
                            InstrumentTypeId = 14,
                            Description = "Acoustic guitar",
                            Name = "Acoustic Guitar",
                            ParentInstrumentTypeId = 4
                        },
                        new
                        {
                            InstrumentTypeId = 15,
                            Description = "Main vocal track",
                            Name = "Lead Vocals",
                            ParentInstrumentTypeId = 5
                        },
                        new
                        {
                            InstrumentTypeId = 16,
                            Description = "Background vocals",
                            Name = "Backing Vocals",
                            ParentInstrumentTypeId = 5
                        },
                        new
                        {
                            InstrumentTypeId = 17,
                            Description = "Lead synthesizer",
                            Name = "Lead Synth",
                            ParentInstrumentTypeId = 6
                        },
                        new
                        {
                            InstrumentTypeId = 18,
                            Description = "Synthesizer pad",
                            Name = "Pad",
                            ParentInstrumentTypeId = 6
                        });
                });

            modelBuilder.Entity("RemixHub.Shared.Models.Remix", b =>
                {
                    b.Property<int>("RemixId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("TEXT");

                    b.Property<int>("OriginalTrackId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("RemixReason")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<int>("RemixTrackId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("StemsUsed")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<int?>("TrackId")
                        .HasColumnType("INTEGER");

                    b.HasKey("RemixId");

                    b.HasIndex("OriginalTrackId");

                    b.HasIndex("RemixTrackId")
                        .IsUnique();

                    b.HasIndex("TrackId");

                    b.ToTable("Remixes");
                });

            modelBuilder.Entity("RemixHub.Shared.Models.Stem", b =>
                {
                    b.Property<int>("StemId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<int>("DurationSeconds")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("InstrumentTypeId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<int>("TrackId")
                        .HasColumnType("INTEGER");

                    b.HasKey("StemId");

                    b.HasIndex("InstrumentTypeId");

                    b.HasIndex("TrackId");

                    b.ToTable("Stems");
                });

            modelBuilder.Entity("RemixHub.Shared.Models.Track", b =>
                {
                    b.Property<int>("TrackId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Album")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("ApprovedDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Artist")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<int>("BitRate")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("Bpm")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<int>("DurationSeconds")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("GenreId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsApproved")
                        .HasColumnType("INTEGER");

                    b.Property<string>("MusicalKey")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("TEXT");

                    b.Property<int>("SampleRate")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SubgenreId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("UploadDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("TrackId");

                    b.HasIndex("GenreId");

                    b.HasIndex("SubgenreId");

                    b.HasIndex("UserId");

                    b.ToTable("Tracks");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("RemixHub.Shared.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("RemixHub.Shared.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RemixHub.Shared.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("RemixHub.Shared.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("RemixHub.Shared.Models.Genre", b =>
                {
                    b.HasOne("RemixHub.Shared.Models.Genre", "ParentGenre")
                        .WithMany("Subgenres")
                        .HasForeignKey("ParentGenreId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("ParentGenre");
                });

            modelBuilder.Entity("RemixHub.Shared.Models.InstrumentType", b =>
                {
                    b.HasOne("RemixHub.Shared.Models.InstrumentType", "ParentInstrumentType")
                        .WithMany("SubInstrumentTypes")
                        .HasForeignKey("ParentInstrumentTypeId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("ParentInstrumentType");
                });

            modelBuilder.Entity("RemixHub.Shared.Models.Remix", b =>
                {
                    b.HasOne("RemixHub.Shared.Models.Track", "OriginalTrack")
                        .WithMany("RemixesOfThis")
                        .HasForeignKey("OriginalTrackId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("RemixHub.Shared.Models.Track", "RemixTrack")
                        .WithOne()
                        .HasForeignKey("RemixHub.Shared.Models.Remix", "RemixTrackId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RemixHub.Shared.Models.Track", null)
                        .WithMany("Remixes")
                        .HasForeignKey("TrackId");

                    b.Navigation("OriginalTrack");

                    b.Navigation("RemixTrack");
                });

            modelBuilder.Entity("RemixHub.Shared.Models.Stem", b =>
                {
                    b.HasOne("RemixHub.Shared.Models.InstrumentType", "InstrumentType")
                        .WithMany("Stems")
                        .HasForeignKey("InstrumentTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("RemixHub.Shared.Models.Track", "Track")
                        .WithMany("Stems")
                        .HasForeignKey("TrackId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("InstrumentType");

                    b.Navigation("Track");
                });

            modelBuilder.Entity("RemixHub.Shared.Models.Track", b =>
                {
                    b.HasOne("RemixHub.Shared.Models.Genre", "Genre")
                        .WithMany("Tracks")
                        .HasForeignKey("GenreId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("RemixHub.Shared.Models.Genre", "Subgenre")
                        .WithMany("SubgenreTracks")
                        .HasForeignKey("SubgenreId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("RemixHub.Shared.Models.ApplicationUser", "User")
                        .WithMany("Tracks")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Genre");

                    b.Navigation("Subgenre");

                    b.Navigation("User");
                });

            modelBuilder.Entity("RemixHub.Shared.Models.ApplicationUser", b =>
                {
                    b.Navigation("Tracks");
                });

            modelBuilder.Entity("RemixHub.Shared.Models.Genre", b =>
                {
                    b.Navigation("SubgenreTracks");

                    b.Navigation("Subgenres");

                    b.Navigation("Tracks");
                });

            modelBuilder.Entity("RemixHub.Shared.Models.InstrumentType", b =>
                {
                    b.Navigation("Stems");

                    b.Navigation("SubInstrumentTypes");
                });

            modelBuilder.Entity("RemixHub.Shared.Models.Track", b =>
                {
                    b.Navigation("Remixes");

                    b.Navigation("RemixesOfThis");

                    b.Navigation("Stems");
                });
#pragma warning restore 612, 618
        }
    }
}
