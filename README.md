# Warframe Mission Tracker

An ASP.NET MVC 5 web application that provides live Warframe game data alongside a full user management system and admin panel.

---

## Features

### Live Warframe Dashboard
Pulls real-time data from the [Warframe Status API](https://api.warframestat.us/) and displays:
- **Sortie** — daily mission chain with faction and modifiers
- **Archon Hunt** — weekly boss missions and shard reward
- **Void Trader (Baro Ki'Teer)** — location, inventory, and arrival/departure countdown
- **Nightwave** — active challenges with reputation values and expiry timers
- **Void Fissures** — active fissures filtered by tier, with live countdown timers
- **Invasions** — ongoing invasions with faction rewards and completion percentage
- **Incarnon Genesis** — current weekly weapon rotation (8-week cycle)
- **Arbitration** — current node, mission type, and enemy faction
- **Cetus & Orb Vallis Cycles** — day/night and warm/cold cycle timers
- **Darvo's Daily Deal** — discounted item with sale price and stock

All data is cached in memory with per-endpoint TTLs and falls back to the last known good data if the API is unavailable.

### User Accounts
- Register and login with email/password
- Email confirmation required before login
- Forgot password / reset password via email link
- Profile page — edit first name, last name, phone number, and profile picture
- Change password
- File uploads attached to your account (images, video, audio, documents — up to 50 MB)
- Notification preferences — subscribe/unsubscribe from Warframe event alerts

### Admin Panel
Accessible only to users in the **Admin** role.

- **Dashboard** — total users, total file uploads, total subscriptions, recent activity log
- **User Management** — paginated user list with search (username/email) and column sorting
- **User Details** — view profile, file uploads, and subscriptions for any user
- **Edit User** — modify username, email, and email confirmation status
- **Role Management** — promote users to Admin or remove the Admin role
- **Force Confirm Email** — manually confirm a user's email
- **Delete User** — removes the user and all associated data
- **Activity Logs** — full audit trail of every admin action

---

## Technology Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET MVC 5 (.NET Framework 4.8) |
| Language | C# |
| ORM | Entity Framework 6 (Code First) |
| Auth | ASP.NET Identity 2 |
| Database | SQL Server LocalDB |
| Frontend | Razor Views, Bootstrap 5, jQuery |
| Validation | jQuery Unobtrusive Validation + DataAnnotations |
| External API | [warframestat.us](https://api.warframestat.us/) |
| HTTP Client | System.Net.Http.HttpClient |
| JSON | Newtonsoft.Json |

---

## Project Structure

```
WarframeUpdate/
├── Controllers/
│   ├── AdminController.cs       # Admin panel actions
│   ├── AccountController.cs     # Register, Login, Password reset
│   ├── ManageController.cs      # Profile, password, settings
│   ├── HomeController.cs        # Dashboard
│   ├── FileUploadController.cs  # File upload/download
│   └── PreferencesController.cs # Event subscriptions
│
├── Models/
│   ├── AccountViewModels.cs     # Login, Register, Password ViewModels
│   ├── ExtendedModels.cs        # UserProfile, FileUpload, EventSubscription
│   ├── WarframeModels.cs        # API response models + DashboardViewModel
│   └── IdentityModels.cs        # ApplicationUser, ApplicationDbContext
│
├── Views/
│   ├── Home/Index.cshtml        # Main dashboard
│   ├── Admin/                   # Admin panel views
│   ├── Account/                 # Auth views
│   ├── Manage/                  # Profile/settings views
│   └── Shared/_Layout.cshtml   # Site layout + nav
│
├── Services/
│   └── WarframeService.cs       # API fetching + in-memory cache
│
├── Content/
│   └── Site.css                 # Custom dark HUD theme
│
└── Filters/
    └── AdminAuthorize.cs        # [AdminAuthorize] filter
```

---

## Getting Started

### Prerequisites
- Visual Studio 2019 or later
- .NET Framework 4.8
- SQL Server LocalDB (included with Visual Studio)

### Setup

1. **Clone the repository**
   ```
   git clone https://github.com/yourname/WarframeUpdate.git
   ```

2. **Open the solution**
   Open `WarframeUpdate.sln` in Visual Studio.

3. **Restore NuGet packages**
   Right-click the solution → **Restore NuGet Packages**

4. **Apply database migrations**
   Open the Package Manager Console and run:
   ```
   Update-Database
   ```

5. **Run the application**
   Press **F5** or click **IIS Express** to start.

### Creating the first Admin account

1. Register a new account through the app
2. Confirm the email (check the link in the debug output or use the admin panel to force-confirm)
3. Open the Package Manager Console and run:
   ```sql
   -- Find your user ID first, then assign the Admin role via the app
   ```
   Or manually assign the Admin role through SQL Server Object Explorer.

---

## API Data & Caching

The app queries `https://api.warframestat.us/pc/` for all game data. Each endpoint has its own cache TTL:

| Endpoint | Cache Duration |
|---|---|
| Sortie, Nightwave, Deals | 10 minutes |
| Void Trader, Archon Hunt | 30 minutes |
| Invasions | 5 minutes |
| Fissures | 30 seconds |
| Cetus / Vallis cycles | 2 minutes |
| Arbitration | 1 minute |

If an API call fails or returns an error, the last cached response is served and the dashboard shows an offline indicator.

---

## License

This project was built as a university assignment. Warframe and all related assets are property of [Digital Extremes](https://www.digitalextremes.com/).
