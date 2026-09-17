# Contributing to Mailist

Contributions are highly welcome. Please open an issue before implementing a feature to discuss your plans.

Mailist's source code is split into the backend (located in `server`) and the frontend (located in `webapp`).
The following instructions are written for Windows but generally also apply to Linux development setups.

## Tech Stack

Backend

- ASP.NET Core
- Entity Framework Core
- MySQL / MariaDB
- Docker

Frontend

- Vue 3
- TypeScript
- PrimeVue
- Tailwind CSS

## Development Setup

Backend

- Visual Studio 2026
- .NET SDK 10.0
- EF Core CLI Tools _(e.g. `dotnet tool install -g dotnet-ef`)_
- MySQL or MariaDB _(e.g. from_ [_PSModules_](https://github.com/daniel-lerch/psmodules)_)_

Frontend

- Visual Studio Code
- Vue Language Features (Volar) Extension
- NodeJS 24 LTS

During development the frontend running on the Vue CLI development server will use _http://localhost:10501_ as API endpoint.
That means the backend can be running in Visual Studio with Debugger attached.

<details>
  <summary>Who this document is for</summary>

  - **Written for:** [Mike (Software Developer)](../../personas/mike-developer.md)
  - **Not written for:** [Victor (Server Administrator)](../../personas/victor-server-administrator.md)
</details>
