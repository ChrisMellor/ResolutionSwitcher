# ResolutionSwitcher

ResolutionSwitcher is a Windows tray app that watches for running game processes and switches a chosen display to a saved resolution while that game is open. When no matching profile is active, it restores the display's original mode.

## Features

- Runs in the system tray and keeps watching after the main window is closed.
- Creates per-game profiles that map a process name to a display and resolution.
- Supports optional refresh-rate matching.
- Restores the original display mode when the game closes, the watcher is paused, or configuration becomes invalid.
- Registers itself to start automatically for the current Windows user and launches in the background at sign-in.

## Requirements

- Windows 10 or Windows 11
- .NET 8 SDK for local development
- A display/driver combination that supports the resolution you want to apply

## Quick Start

Run the desktop app locally:

```powershell
dotnet run --project .\src\ResolutionSwitcher.Desktop\ResolutionSwitcher.Desktop.csproj
```

Build the solution:

```powershell
dotnet build .\ResolutionSwitcher.slnx -c Release
```

Publish a self-contained Windows executable:

```powershell
dotnet publish .\src\ResolutionSwitcher.Desktop\ResolutionSwitcher.Desktop.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -p:EnableCompressionInSingleFile=true `
  -p:DebugType=None `
  -p:DebugSymbols=false `
  -o .\artifacts\publish
```

The published executable is written to `artifacts\publish\ResolutionSwitcher.Desktop.exe`.

## How It Works

ResolutionSwitcher evaluates profiles from top to bottom. The first profile whose process is currently running wins.

For the active profile, the app:

1. Captures the current resolution of the target display.
2. Resolves the requested width, height, and optional refresh rate against the display's supported modes.
3. Applies the requested mode.
4. Restores the original mode when that profile is no longer active.

If no profile matches, the watcher stays idle and leaves your displays in their normal state.

## Using the App

1. Launch the app.
2. Add a profile.
3. Enter a profile name.
4. Choose the game process by typing it, browsing to the `.exe`, or picking from currently running processes.
5. Choose the target display.
6. Pick a supported display mode or enter the width, height, and optional refresh rate manually.
7. Order profiles by priority using `Move up` and `Move down`.
8. Leave `Watcher enabled` checked.
9. Close the window when you are done. The app continues running from the tray.

Notes:

- Only the executable name is stored. Full paths and `.exe` are normalized automatically.
- A refresh rate of `0` in the editor means "auto". The app will use the highest supported refresh rate for that resolution.
- Closing the window does not exit the app. Use the tray icon's `Exit` command to stop it completely.
- `Scan now` in the tray forces an immediate re-check without waiting for the next poll interval.

## Configuration File

Configuration is stored here:

```text
%LocalAppData%\ResolutionSwitcher\resolution-switcher.json
```

On a typical machine that resolves to:

```text
C:\Users\<you>\AppData\Local\ResolutionSwitcher\resolution-switcher.json
```

The app creates this file automatically on first run.

### Example

```json
{
  "watcherEnabled": true,
  "pollIntervalSeconds": 3,
  "profiles": [
    {
      "name": "Elden Ring",
      "processName": "eldenring.exe",
      "displayName": null,
      "width": 2560,
      "height": 1440,
      "refreshRate": 144
    },
    {
      "name": "Retro Game",
      "processName": "retroarch",
      "displayName": "\\\\.\\DISPLAY2",
      "width": 1920,
      "height": 1080,
      "refreshRate": null
    }
  ]
}
```

### Configuration Notes

- `watcherEnabled` toggles monitoring on and off.
- `pollIntervalSeconds` must be a positive integer. The UI edits this in the `1-60` second range.
- `profiles` are evaluated in list order.
- `displayName: null` means the primary display.
- `refreshRate: null` means "use the highest supported refresh rate for this resolution".

If the config file is invalid, the app warns the user, loads a blank in-memory configuration, and lets the user save a corrected file.

## Tray Behavior

When running, the tray menu provides:

- Current watcher status
- `Open settings`
- `Scan now`
- `Open config folder`
- `Exit`

The app also writes itself to the current user's Windows `Run` registry key so it can start automatically with the `--background` argument.

## Project Structure

```text
src/
  ResolutionSwitcher.Domain/          Core models
  ResolutionSwitcher.Application/     Use cases, validation, watcher logic
  ResolutionSwitcher.Infrastructure/  Windows display, process, config, startup integration
  ResolutionSwitcher.Desktop/         Windows Forms UI and tray application
```

## Development Notes

- This is a Windows-only application. The desktop project targets `net8.0-windows`.
- If the tray app is already running from a previous debug session, stop it before rebuilding. Otherwise Windows can keep the output DLLs locked.
- Release publishing is also automated in `.github/workflows/release.yml`, which uploads `ResolutionSwitcher.Desktop.exe` to a GitHub release.

## License

See `LICENSE.txt`.
