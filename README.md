AD_CW1_23050386
=================

Small .NET solution for an academic journal app built with a layered architecture and a MAUI front-end.

Key points
- **Tech:** .NET 9, Entity Framework Core, .NET MAUI (MacCatalyst), SQLite
- **Projects:** `Application`, `Domain`, `Infrastructure`, `Maui`
- **Purpose:** Demonstrates layered architecture, EF Core migrations, and a MAUI UI.

Quick start (macOS)
- Restore and build solution (use IDE or):

  - dotnet restore
  - dotnet build

- Run the MAUI app (use IDE or):

  dotnet run --project Maui

Repository layout
- `Application` — application services and DTOs.
- `Domain` — entities and domain models.
- `Infrastructure` — EF Core DbContext, migrations, persistence.
- `Maui` — UI, startup, and platform assets.

Notes
- The MAUI project targets MacCatalyst for macOS development.

