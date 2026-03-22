# ThommyKalkulator

Modern rewrite of a small calculation tool for a 3D printing business.

The original version existed as a Python / PySide project.  
This repository contains the new development state as a structured **C# / .NET 8 WPF application** with a clear separation of Domain, Application, Infrastructure, and UI.

## Status

**Current state:** early functional development stage / work in progress

Already implemented:

- WPF desktop UI
- MVVM using `CommunityToolkit.Mvvm`
- structured multi-project architecture
- JSON-based local data persistence
- CSV export
- PDF export using `QuestPDF`
- multiple pages for projects, materials, calculation, settings, and appearance

Planned / still in progress:

- additional business logic refinements
- UI/UX improvements
- validation
- meaningful automated tests
- packaging / release workflow

## Project Structure

```text
ThommyKalkulator/
├── src/
│   ├── ThommyKalkulator.Domain
│   ├── ThommyKalkulator.Application
│   ├── ThommyKalkulator.Infrastructure
│   └── ThommyKalkulator.WPF
└── tests/
    └── ThommyKalkulator.Tests
```

### Layers

- **Domain**  
  Business models and value objects

- **Application**  
  Application services, use cases, and interfaces

- **Infrastructure**  
  Persistence and export logic, e.g. JSON, CSV, and PDF

- **WPF**  
  UI with views and view models

- **Tests**  
  Placeholder for upcoming unit tests

## Technologies

- .NET 8
- WPF
- CommunityToolkit.Mvvm
- QuestPDF

## Running Locally

### Requirements

- Visual Studio 2022 or newer
- .NET 8 SDK
- Windows

### Start

1. Clone the repository
2. Open `ThommyKalkulator.slnx`
3. Set `ThommyKalkulator.WPF` as the startup project
4. Run the application

## Data Storage

The application currently stores its data locally as a JSON file under:

```text
%LocalAppData%\ThommyKalkulator\data.json
```

## Project Goal

This project is a modern technical rebuild of the original tool, with a focus on:

- better maintainability
- clearer architecture
- easier extensibility
- a more modern desktop UI
- a better foundation for testing and future releases

## Notes

This repository represents an **active development state**, not a finished product.  
The structure, features, and UI will continue to evolve over time.
