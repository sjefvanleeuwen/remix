# 🎶 RemixHub — Collaborative Music Sharing & Remix Platform

---

## 🧱 Technology Stack

- **Frontend**: Blazor WebAssembly (.NET 9.0)
- **Backend**: ASP.NET Core Web API
- **Database**: SQLite (Entity Framework Core)
- **Authentication**: ASP.NET Core Identity with roles
- **Email Service**: SMTP (SendGrid or MailKit)
- **CAPTCHA**: Google reCAPTCHA v2 or v3
- **Audio Metadata Extraction**: TagLibSharp or equivalent
- **Audio Playback**: HTML5 audio with JS interop for previews
- **Storage**: Local file system or S3-compatible service

---

## 📁 Supported File Types and Metadata

- **Accepted Audio File Types**: MP3, FLAC, WAV
- **Metadata Automatically Extracted Upon Upload**:
  - Title
  - Artist
  - Album
  - Genre
  - Duration
  - Bit rate
  - Sample rate
  - BPM (if available)
  - Musical key

---

## 🧭 Page Structure

### Public Pages

- Home
- Explore
- Track Details
- Search
- Login
- Register (with CAPTCHA and email verification)
- Verify Email
- Forgot Password

### Authenticated User Pages

- Dashboard
- Upload Track (with stems)
- Manage My Tracks
- Remix Track
- Edit Profile

### Admin Pages

- Admin Dashboard
- User Management
- Track Moderation
- Content Reports
- Site Settings

---

## 🎼 Hierarchical Genre Selection

Users must select a **primary genre** and may optionally select a **subgenre** when uploading a track. Genres are organized as a tree structure and displayed using a collapsible dropdown selector.

Example structure:

- Electronic  
  - House  
    - Deep House  
    - Progressive House  
  - Techno  
    - Detroit Techno  
    - Acid Techno  

- Rock  
  - Alternative Rock  
  - Hard Rock  
  - Punk Rock  

- Hip-Hop  
  - Boom Bap  
  - Trap  
  - Lofi  

Genres are stored in a tree structure in the database and validated server-side. Admins can add, edit, or remove genres and subgenres via the admin panel.

---

## 🪕 Stem Upload with Instrument Type Categorization

Each stem file uploaded by a user must be tagged with a specific **instrument type** selected from a hierarchical instrument taxonomy. This ensures consistency and enables powerful filtering and remixing options.

Example instrument hierarchy:

- Percussion  
  - Drums  
  - Hi-Hats  
  - Cymbals  

- Bass  
  - Electric Bass  
  - Synth Bass  

- Guitar  
  - Acoustic Guitar  
  - Electric Guitar  

- Vocals  
  - Lead Vocals  
  - Backing Vocals  
  - Harmony  

- Synth  
  - Pads  
  - Leads  
  - Arpeggios  

- FX  
  - Risers  
  - Impacts  
  - Glitches  

Users select an instrument type per stem during upload using a two-level dropdown interface. All instrument types are admin-managed and validated on the backend.

---

## 🔐 Authentication and Registration

- User registration form includes:
  - Username
  - Email
  - Password
  - CAPTCHA (Google reCAPTCHA)
- After registration:
  - User receives verification email with secure link
  - User remains inactive until email is verified
- Email used for:
  - Verification
  - Password reset
  - Notifications
- Secure password reset flow with email link
- Roles include: User, Admin

---

## 🔄 Remix Workflow

- Each uploaded track can be remixed by other users
- Remixes must reference the original track (relational model)
- Track page shows remix lineage and attribution
- Remixing requires uploading new files with a remix reason and stem usage declaration
- Original authors receive credit and notification

---

## 🔎 Search and Filtering

Search supports:

- Keyword (title, tags, artist)
- Genre (with hierarchical filtering)
- BPM range
- Duration range
- Key
- Instrument type
- Upload date
- Popularity
- Remix count

---

## 📊 Admin Dashboard Features

- Track moderation queue (approve, edit, remove)
- User management (ban, edit, reset, promote)
- Report center (flagged content, abuse)
- Genre and instrument taxonomy editor
- Email settings and system templates
- Site metrics: user count, track uploads, remix stats, flagged content

---

## 📬 Email System

- All transactional email sent via SMTP (SendGrid or MailKit)
- Used for:
  - Registration verification
  - Password resets
  - Admin alerts
- Email templates managed by admins
- Optional global announcement tool for users

---

## 📈 User Dashboard Features

- Overview of uploaded tracks
- Remix history (both remixed and remixes received)
- Profile stats
- Upload and remix buttons
- Profile editing (bio, avatar, links)

---

## 🛠 Future Features (Optional)

- Commenting on tracks and remixes
- Track likes or favorites
- Remix challenges and contests
- Collaboration requests
- Real-time stem preview and waveform
- Public API access
- Progressive Web App (PWA)
- Mobile upload support
- Visual remix lineage tree

---