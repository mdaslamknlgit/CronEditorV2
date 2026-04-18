# CronEditor V2

A modern Blazor Server application for managing cron schedules, built with .NET 8.

## Features

- **Visual Cron Builder** — Build cron expressions field-by-field with real-time preview and description
- **Expression Editor** — Manually edit cron expressions with live validation and description
- **Schedule Manager** — Full CRUD for named cron schedules with active/inactive status
- **Preset Schedules** — 17 common presets (every minute, hourly, daily, weekly, monthly, etc.)
- **Real-time Validation** — Instant feedback on expression validity
- **Human-readable Descriptions** — Automatically generates plain-English descriptions of expressions
- **Responsive Design** — Works on desktop and mobile

## Technology Stack

- **.NET 8** — Latest LTS release
- **Blazor Server** with Interactive Server rendering
- **Bootstrap 5** — UI components and responsive grid
- **Bootstrap Icons** — Icon library

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Running the Application

```bash
cd CronEditorV2
dotnet run
```

Then open your browser to `http://localhost:5123`.

### Building

```bash
dotnet build
```

## Project Structure

```
CronEditorV2/
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor        # Root layout with sidebar
│   │   └── NavMenu.razor           # Navigation sidebar
│   ├── Modals/
│   │   ├── CronBuilderModal.razor  # Visual cron expression builder
│   │   └── CronExpressEditorModal.razor  # Direct expression editor
│   ├── App.razor                   # Root component
│   ├── Routes.razor                # Router
│   └── _Imports.razor              # Global using statements
├── Pages/
│   ├── CronSchedule.razor          # Main schedule management page
│   ├── Counter.razor               # Demo counter page
│   ├── Home.razor                  # Landing page
│   └── Error.razor                 # Error page
├── Services/
│   └── CronBuilderService.cs       # Business logic & in-memory storage
├── wwwroot/
│   ├── css/app.css                 # Custom styles
│   └── app.js                      # JavaScript helpers
├── Program.cs                      # App startup
└── CronEditorV2.csproj
```

## Cron Expression Format

```
┌───────────── minute (0 - 59)
│ ┌───────────── hour (0 - 23)
│ │ ┌───────────── day of the month (1 - 31)
│ │ │ ┌───────────── month (1 - 12)
│ │ │ │ ┌───────────── day of the week (0 - 7, Sunday = 0 or 7)
│ │ │ │ │ ┌───────────── year (optional, 1970-2099)
* * * * * *
```

### Special Characters

| Character | Meaning |
|-----------|---------|
| `*`       | Any value |
| `*/n`     | Every n units |
| `a-b`     | Range from a to b |
| `a,b,c`   | List of values |

## License

MIT
