# PaperTrail Contract Tracker

A local-first WPF application for tracking contracts, parties, attachments and reminders.

## Getting Started

```bash
dotnet restore
dotnet build
```

Run the application:

```bash
dotnet run --project PaperTrail.App
```

Run tests (if any):

```bash
dotnet test
```

## MongoDB

The application stores all data in a MongoDB database named `FIWB-PaperTrail`. The connection string is
read from the `MONGODB_URI` environment variable. You can set this variable in your shell or by creating a
`.env` file. See `.env.example` for the expected format.
Collections in the database are `Attachments`, `ImportedContracts`, `Parties`, `PreviousContracts` and `Reminders`.

## Features

- Advanced filtering with search, multi-status and tag selection, renewal date ranges and saved views.
- Attachment management with hash based de-duplication.
- Reminder engine toasts due reminders only once.
- JSON backup and restore of imported and previous contracts, parties, attachments (metadata only) and reminders.

## Paths

Data and files are stored under `%LOCALAPPDATA%/PaperTrailContractTracker`.

## Licensing

Some features such as CSV export and backup/restore require a Pro licence which can be entered in the Settings window.

## Troubleshooting

Logs are written under the application data folder.  To reset saved filter views delete `views.json` from that location.
