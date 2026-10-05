# Testing odl3d

## Requirements

- .NET 10 SDK
- Native libraries for T1: FreeType and GLFW
- A display for the GLFW hidden-window smoke test. On headless Linux, use Xvfb.

The native loaders search `bin/` first, then use the operating system's library search path. The repository's `bin/` directory contains the Windows DLLs. On Linux, install the system libraries and Xvfb:

```sh
sudo apt-get install libfreetype6 libglfw3 xvfb fonts-dejavu-core
```

On macOS, install the libraries with Homebrew:

```sh
brew install freetype glfw
```

The CI workflow installs these dependencies and runs the GLFW window test under Xvfb on Linux.

## Run Tests

Run both current test tiers from the repository root:

```sh
dotnet test odl3d.sln
```

Or run one tier:

```sh
dotnet test tests/unit/odl3d.Tests.Unit.csproj
dotnet test tests/native/odl3d.Tests.Native.csproj
```

On headless Linux, wrap the T1 command:

```sh
xvfb-run -a dotnet test tests/native/odl3d.Tests.Native.csproj
```

## Coverage

Collect line and branch data for both tiers with:

```sh
dotnet test odl3d.sln --settings tests/coverage.runsettings --collect:"XPlat Code Coverage"
```

This creates a `coverage.cobertura.xml` report under each test project's `TestResults` directory. To combine the reports into a readable summary and HTML page, install ReportGenerator once:

```sh
dotnet tool install --global dotnet-reportgenerator-globaltool
reportgenerator "-reports:tests/**/TestResults/**/coverage.cobertura.xml" "-targetdir:tests/coverage-report" "-reporttypes:Html;TextSummary"
```

Read `tests/coverage-report/Summary.txt` for the quick result or open `tests/coverage-report/index.html` for file-by-file details. The GitHub Actions workflow also uploads a `merged-coverage` artifact containing a merged Cobertura report, HTML report, and text summary.

Coverage currently measures the referenced `odl3d` assembly, not a filtered T0/T1-only source set. The plan's 100% line-coverage target for tier-owned files is not yet enforced as a CI threshold; use per-file results when assessing tier progress.

## Future Work

- Complete T0 CPU coverage, including deterministic model/image/font/shader fixtures, loaders, text geometry, scene logic, and input-state tests.
- Complete T1's platform matrix: exercise the Windows/Unix FreeType layouts, macOS GLFW binding, and Metal selectors/device availability.
- Add rendering prerequisites (offscreen targets and pixel readback), then implement shared backend conformance and integration tests.
- Add golden-image comparisons, benchmarks, soak tests, fuzzing, and hardware-GPU nightly coverage.

See [tests/TEST_PLAN.md](tests/TEST_PLAN.md) for the detailed checklist and rollout plan.