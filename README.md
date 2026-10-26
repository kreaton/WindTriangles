# WindTriangles

A sailing navigation aid that calculates the **compass course to steer**. You enter the desired course and distance, the boat speed, the current (set and drift), the wind (direction and speed), the leeway, the variation and the deviation. Press **Calculate**, and the app shows:

- the compass course, speed over ground and time to go
- each correction on the way from true course to compass course
- true and apparent wind angles, with a warning when the course is inside the no-go zone
- the current triangle and the wind triangle drawn as vectors on a compass rose with degree markings

It is a Blazor WebAssembly app (.NET 10) that runs entirely in the browser, with no server-side code. The layout adapts to PC, iPad and iPhone.

See [guide.md](guide.md) for the principles behind the calculation, with a worked example.

## Running locally

```bash
dotnet run --project src/WindTriangles
```

Then open http://localhost:5141.

## Versioning

The version is `Major.Minor.Build`. Major and minor are set by hand in `Directory.Build.props`. The build number is the GitHub Actions run number (0 for local builds). The footer shows the version, the build time (Danish time) and the git commit, for example `(c) Per Hejndorf, 2026 · v1.0.7 · 26.10.2026 14:32 · a1b2c3d`.

## CI/CD

`.github/workflows/deploy.yml` runs on every push to `main` (or manually from the Actions tab). It publishes a Release build with the run number as the version, then uploads `wwwroot` to the web server over FTP. Only changed files are sent after the first run.

It needs these repository secrets (Settings › Secrets and variables › Actions):

| Secret | Example |
|--------|---------|
| `FTP_HOST` | `ftp.example.com` |
| `FTP_USERNAME` | |
| `FTP_PASSWORD` | |
| `FTP_FOLDER` | `webroots/www/windtriangles` (no trailing slash) |

The app is meant to be hosted under `/windtriangles/` (`<base href="windtriangles" />` in `wwwroot/index.html`, the same convention as the other apps).

## Project layout

```
src/WindTriangles/
  Navigation/Vec.cs                     Vector and angle helpers
  Navigation/NavigationInput.cs         Input values
  Navigation/WindTriangleCalculator.cs  The calculation: current, leeway, variation, deviation, apparent wind
  Components/CompassPlot.razor          SVG compass rose with the vector triangles
  Components/NumberField.razor          Number input that accepts both "1.5" and "1,5"
  Components/EastWestToggle.razor       E/W switch for variation and deviation
  Pages/Home.razor                      The page: inputs, results, plot and footer
  BuildInfo.cs                          Version, build time and commit for the footer
```
