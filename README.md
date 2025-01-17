# Tour de App tým h
<p align="center">
    <img src="./githubresources/hlogo.png" />
</p>

## TdA25
### Členové týmu h

| **Člen týmu**  | **Pozice**        |
|----------------|-------------------|
| Martin Fiala   | Fullstack         |
| Klára Futejová | Frontend          |
| Jonáš Holub    | Design a frontend |

### Využité technologie
- Server: **ASP.NET Core**  (with Minimal APIs)
- Prezentace: **Blazor Web App** (Server + WASM)
- ORM: **Entity Framework Core**
- Databáze: **SQLite**

## Dokumentace
### Spouštění
> Při spuštění se spustí EF Core migrace, což sice není dobrá praxe,
ale pro účely této aplikace je to dostačující.

#### Visual studio:
Since it is a Blazor web app, with both rendering options (Server and WASM), the `h.Server` project must be launched.

In vs, there are two options:
* Either the `h.Server` and run the `https` launch profile,
* or better select `docker-compose`.

#### Docker:
The project can be run in a docker container. The `docker-compose` file is included in the project.

#### CLI:
The project can be run from the CLI. The `h.Server` project must be launched
with the https launch profile:
```bash
$ dotnet run --project src\h.Server\h.Server.csproj --launch-profile https
```

### Architektura
<img src="./githubresources/architecture.png" />

