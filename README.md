# PaperTrail Contract Tracker

A local-first WPF application for tracking contracts, parties, attachments and reminders.

## Getting Started

```bash
dotnet restore
dotnet build
# create initial migration
# dotnet ef migrations add InitialCreate -p PaperTrail.Core -s PaperTrail.App
# update database
dotnet ef database update -p PaperTrail.Core -s PaperTrail.App
```

Run the application:

```bash
dotnet run --project PaperTrail.App
```

Run tests:

```bash
dotnet test
```

## Features

- Advanced filtering with search, multi-status and tag selection, renewal date ranges and saved views.
- Attachment management with hash based de-duplication.
- Reminder engine toasts due reminders only once.
- JSON backup and restore of contracts, parties, attachments (metadata only) and reminders.

## Paths

Data and files are stored under `%LOCALAPPDATA%/PaperTrailContractTracker`.

## Licensing

Some features such as CSV export and backup/restore require a Pro licence which can be entered in the Settings window.

## Troubleshooting

Logs are written under the application data folder.  To reset saved filter views delete `views.json` from that location.
