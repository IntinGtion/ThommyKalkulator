# ThommyKalkulator.Setup

Dieses Projekt erzeugt ein MSI-Setup fuer den WPF-Kalkulator.

## Voraussetzungen

- .NET 8 SDK
- Internetzugang fuer den ersten NuGet-Restore
- Optional fuer Visual Studio: FireGiant HeatWave Community Edition, damit SDK-basierte WiX-Projekte direkt in Visual Studio bearbeitet werden koennen

## Build

### Direkt ueber die WiX-Projektdatei

```powershell
dotnet build .\installer\ThommyKalkulator.Setup\ThommyKalkulator.Setup.wixproj -c Release
```

### Ueber das Hilfsskript

```powershell
.\scripts\Build-Installer.ps1
```

## Ergebnis

Das MSI liegt danach unter:

```text
installer\ThommyKalkulator.Setup\bin\Release\
```

## Hinweise

- Vor dem MSI-Build wird automatisch ein `dotnet publish` des WPF-Projekts nach `installer\ThommyKalkulator.Setup\bin\<Konfiguration>\PublishedApp` ausgefuehrt.
- Die App wird als x64 und self-contained veroeffentlicht.
- Die Standard-WiX-Oberflaeche ist auf `de-DE` gestellt.
- Der Lizenztext ist aktuell nur eine einfache Testversion-Platzhalterdatei (`InstallerLicense.rtf`) und sollte vor einem oeffentlichen Release ersetzt werden.
- Benutzerdaten unter `%LocalAppData%\ThommyKalkulator` werden bei einer Deinstallation absichtlich nicht geloescht.
