# ThommyKalkulator

Moderner Rewrite eines kleinen Kalkulationsprogramms für ein 3D-Druck-Unternehmen.

Der ursprüngliche Stand lag als Python-/PySide-Projekt vor.  
Dieses Repository enthält den neuen Entwicklungsstand als strukturierte **C#/.NET 8 WPF-Anwendung** mit klarer Trennung von Domain, Application, Infrastructure und UI.

## Status

**Aktueller Stand:** früher funktionaler Entwicklungsstand / Work in Progress

Bereits vorhanden:

- WPF-Desktop-Oberfläche
- MVVM mit `CommunityToolkit.Mvvm`
- Projektstruktur mit separaten Schichten
- JSON-basierte Datenspeicherung
- CSV-Export
- PDF-Export mit `QuestPDF`
- mehrere Seiten für Projekte, Materialien, Kalkulation, Einstellungen und Erscheinungsbild

Noch offen bzw. im Ausbau:

- weitere fachliche Feinlogik
- UI/UX-Feinschliff
- Validierung
- belastbare automatisierte Tests
- Packaging / Release-Prozess

## Projektstruktur

```text
ThommyKalkulator/
├── src/
│   ├── ThommyKalkulator.Domain
│   ├── ThommyKalkulator.Application
│   ├── ThommyKalkulator.Infrastructure
│   └── ThommyKalkulator.WPF
└── tests/
    └── ThommyKalkulator.Tests