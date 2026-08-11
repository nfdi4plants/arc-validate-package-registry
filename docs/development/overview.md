# Development setup

## Contents

- [Prerequisites](#prerequisites)
- [Development container](#development-container)
- [Documentation](#documentation)
- [Libraries](#libraries)
- [Registry service with Docker Compose](#registry-service-with-docker-compose)
- [Registry service with `dotnet watch`](#registry-service-with-dotnet-watch)
- [Changing the metadata or database model](#changing-the-metadata-or-database-model)

## Prerequisites

- .NET SDK pinned by `global.json` (currently .NET 10)
- Docker and Docker Compose
- Node.js and `uv` when developing the portable model or checking Python
  validation packages

The main solution contains the registry service, portable model/codecs,
generated client and interop, staging infrastructure, CLI, and their tests.
`PackageStagingArea.slnx` contains the staging-area checks.

## Development container

The repository includes a VS Code development container with the complete
cross-platform toolchain: .NET 10 and F# Interactive, Node.js 24, Python 3.12,
`uv`, the repository-local .NET tools, Docker Compose, GitHub CLI, and OpenCode.
It also installs the Codex and Claude Code extensions, the locally used
Markdown extension set, and focused extensions for the languages and file
formats in this repository.

In VS Code, install the Dev Containers extension, open the repository, and run
**Dev Containers: Reopen in Container**. The first creation restores the .NET
tool manifest and locked Python environment. Confirm the main tools with:

```shell
dotnet --version
dotnet fsi --version
node --version
python --version
uv --version
docker compose version
gh --version
opencode --version
```

Run `gh auth login` to authenticate GitHub CLI. Start `opencode` and use
`/connect` to configure a model provider. Codex and Claude Code prompt for their
own sign-in when first opened in VS Code. CLI configuration, authentication,
Python environments, and caches are kept in the `avpr-devcontainer-data`
Docker volume so they survive a container rebuild without copying host
credentials into the container.

Docker runs inside the development container so that the existing Compose
files and their bind mounts behave consistently on Windows, Linux, and macOS.
This daemon is isolated from the host Docker daemon, and its child containers,
images, and volumes are separate. Docker-in-Docker requires the development
container to run with elevated container privileges.

## Documentation

Long-form documentation lives under `docs/` as ordinary Markdown. The registry
service publishes those same files and renders them at `/docs`; no separate
documentation generator or generated copy is involved.

Keep links between documentation pages relative and include the `.md` suffix,
for example `[testing changes](testing.md)`. That single link form works both in
the deployed documentation site and while browsing the repository on GitHub.
Use an absolute production URL only for runtime-only destinations such as
Swagger or `/_version`.

## Libraries

`ValidationPackage.Model` is the codec-free domain contract shared by .NET and
Fable consumers. `ValidationPackage.Codecs` owns portable YAML/frontmatter and
JSON conversion, while `AVPR.Staging` owns repository traversal, normalized
content, and hashing. `AVPRClient` is the generated .NET registry client;
`AVPRClient.Interop` provides opt-in mappings to the portable model.

Build or test them through the main solution:

```shell
./build.sh TestSolution
```

Use `.\build.cmd TestSolution` on Windows. Other repository-wide targets are
documented in [testing changes](testing.md).

Portable model tests are a regular Pyxpecto executable. Run the same test
source on .NET, JavaScript, and Python using the commands in
[testing changes](testing.md). Python dependencies are declared in the root
`pyproject.toml`, locked in `uv.lock`, and installed with `uv sync --locked`.

For a package release:

1. Bump the package version in the corresponding `.fsproj` or `.csproj`.
2. Update that project's `RELEASE_NOTES.md`.
3. Run the focused and solution-level checks described in
   [testing changes](testing.md).
4. Push the reviewed commit, then manually dispatch its package-specific
   release workflow from GitHub Actions. Model and Codecs publish the same
   build to NuGet, npm, and PyPI; Client and Interop publish to NuGet.

## Registry service with Docker Compose

From the repository root, start the same application, PostgreSQL, and Adminer
stack used by the Visual Studio Docker Compose project:

```shell
docker compose up --build
```

The development override maps the service's port to a dynamically selected host
port. Find it with:

```shell
docker compose port packageregistryservice 8080
```

If the command prints `0.0.0.0:54321`, browse to
`http://localhost:54321/swagger`. Adminer is available at
`http://localhost:8080`.

Common commands:

```shell
# Start in the background
docker compose up --build --detach

# Inspect containers and assigned ports
docker compose ps

# Follow service logs
docker compose logs --follow packageregistryservice

# Stop while retaining containers
docker compose stop

# Remove Compose containers
docker compose down
```

`ASPNETCORE_ENVIRONMENT` is `Development` in the Compose override, so the
service applies migrations and seeds the local database during startup.
Production startup does not apply migrations automatically.

## Registry service with `dotnet watch`

For a faster VS Code edit/rebuild loop, run only the dependencies in Docker and
run the service on the host. In PowerShell:

```powershell
docker compose up --detach package_db adminer

$env:ConnectionStrings__PostgressConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=employee"

dotnet watch --project src/PackageRegistryService/PackageRegistryService.csproj run --launch-profile http
```

The host service is then available at `http://localhost:5099`. Stop `dotnet
watch` with Ctrl+C and stop the dependency containers with `docker compose
stop`.

## Changing the metadata or database model

A validation-package metadata field commonly crosses several projects. Search
for every use and update the relevant layers:

- `src/ValidationPackage.Model/` and, for serialized metadata,
  `src/ValidationPackage.Codecs/`;
- `src/PackageRegistryService/Models/ValidationPackage.cs`;
- Entity Framework ownership in `ValidationPackageDb.cs`;
- database seeding in `DataInitializer.cs`;
- generated client code and `AVPRClient.Interop` mappings;
- website rendering when the field is user-facing;
- fixtures, hashes, contract tests, and documentation.

Generate migrations with the EF tooling rather than writing them from scratch,
then inspect the generated operations and model snapshot. Production migration
SQL is applied manually before deploying the matching image revision.

See [testing changes](testing.md) for the required verification path and
[operations and releases](../operations/releases.md) for publication details.
