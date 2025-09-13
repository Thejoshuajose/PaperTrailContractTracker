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
