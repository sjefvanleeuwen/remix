# RemixHub

RemixHub is a collaborative music sharing and remix platform built with Blazor WebAssembly and ASP.NET Core. The platform allows musicians to upload tracks, share stems, and create remixes with proper attribution.

## 🧱 Technology Stack

- **Frontend**: Blazor WebAssembly (.NET 9.0)
- **Backend**: ASP.NET Core Web API
- **Database**: SQLite (Entity Framework Core)
- **Authentication**: ASP.NET Core Identity with JWT tokens
- **Audio Metadata**: TagLibSharp

## 📋 Prerequisites

- .NET 9.0 SDK
- Visual Studio 2022 or Visual Studio Code
- A modern web browser

## 🚀 Getting Started

### Clone the Repository

```bash
git clone https://github.com/yourusername/remixhub.git
cd remixhub
```

### Configure the Application

1. Update the SMTP settings in `RemixHub.Server/appsettings.json` for email functionality:

```json
"Email": {
  "SmtpServer": "your-smtp-server.com",
  "SmtpPort": 587,
  "SmtpUsername": "your-username",
  "SmtpPassword": "your-password",
  "SenderEmail": "noreply@yourdomain.com",
  "SenderName": "RemixHub"
}
```

2. Update the JWT configuration in the same file:

```json
"JwtKey": "your-secure-key-at-least-16-characters-long",
"JwtIssuer": "RemixHub",
"JwtAudience": "RemixHub",
"JwtExpireDays": 7
```

3. (Optional) If you're using Google reCAPTCHA, add your secret key:

```json
"RecaptchaSecretKey": "your-recaptcha-secret-key"
```

### Database Setup

First, install the Entity Framework Core tools globally:

```bash
dotnet tool install --global dotnet-ef
```

Or if you already have it installed, ensure it's the latest version:

```bash
dotnet tool update --global dotnet-ef
```

Now you can create the database:

```bash
cd RemixHub.Server
dotnet ef database update
```

### Running the Application

#### Option 1: Using Visual Studio

1. Open the solution file `RemixHub.sln` in Visual Studio
2. Set the startup projects to both `RemixHub.Server` and `RemixHub.Client`
3. Press F5 to start debugging

#### Option 2: Using Command Line

First, start the server:

```bash
cd RemixHub.Server
dotnet run
```

Then, in a separate terminal, start the client:

```bash
cd RemixHub.Client
dotnet run
```

### Accessing the Application

- API Server: https://localhost:7001
- Blazor WebAssembly Client: https://localhost:5001

## 🔑 Default Admin Account

When you first run the application, you can create an admin account by registering a regular user and then updating the role in the database:

```sql
-- Run this in SQLite after creating your first user
UPDATE AspNetUserRoles 
SET RoleId = (SELECT Id FROM AspNetRoles WHERE Name = 'Admin')
WHERE UserId = 'your-user-id';
```

## 📁 Project Structure

- **RemixHub.Server**: ASP.NET Core Web API backend
- **RemixHub.Client**: Blazor WebAssembly frontend
- **RemixHub.Shared**: Shared models and view models

## 🎵 Key Features

- **User Management**: Registration, login, profile management
- **Track Management**: Upload, edit, delete tracks with metadata
- **Stem Management**: Upload and manage individual instrument tracks
- **Remix Creation**: Create remixes from original tracks with proper attribution
- **Genre Categorization**: Hierarchical genre system with main genres and subgenres
- **Instrument Categorization**: Categorize stems by instrument type
- **Search & Discovery**: Find tracks by various criteria
- **Admin Panel**: Moderate content, manage users, genres, and settings
- **Responsive UI**: Mobile-friendly design using Bootstrap

## 🔒 Security Features

- JWT authentication for API access
- Role-based authorization
- Email verification
- Password reset functionality
- Input validation

## 🔧 Technical Implementation

- Clean Architecture principles
- Repository pattern for data access
- Dependency Injection
- Asynchronous programming
- Entity Framework Core for data persistence
- Blazor WebAssembly for client-side UI
- JWT for stateless authentication

## 📊 Future Enhancements

- Audio waveform visualization
- In-browser audio editing
- Social features (comments, following, notifications)
- Rating and review system
- Collaborative projects
- Advanced audio analysis
- Integration with external music platforms

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.
