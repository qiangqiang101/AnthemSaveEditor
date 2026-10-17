# Anthem Save Editor

A small Windows desktop app (VB.NET, WinForms, .NET 10) for viewing and editing the save files of the
Azimuth PC build of Forza Horizon 2. Saves live under
`...\data\titles\<title id>\saves\player-1\User_...\revisions\<revision>\`.

> **Unofficial and experimental.** The file formats were reverse-engineered from sample saves. Always keep your
> own copy of your saves. This project is not affiliated with Microsoft, Playground Games or the Azimuth project.

## What it edits

Each revision folder contains a `data` folder and a `commit` file. The app understands these files in `data`:

| File | Format | Editor tab |
| --- | --- | --- |
| `PlayerDatabase` | 8-byte header + SQLite 3 database | Browse tables, edit cells |
| `ProfileData` | `ssmc` header + typed name/value tree | Filter and edit values (Credits, XP, stats, options...) |
| `SaveState` | ASCII XML (objects and properties) | Filter and edit property values |
| `VersionFlags` | One little-endian `uint32` | Edit the number |
| `BinarySaveState` | Binary Drivatar / radio / road data | Not edited; copied unchanged |

Edits are written back in the original format: the original header, file size and (for `SaveState`) formatting
are preserved, so only the values you changed differ.

## The `commit` manifest

The game verifies each revision against its `commit` file, which lists every file in `data` with its SHA-256 and size:

```
Azimuth complete save revision v2
blobs 5
<sha256> <size> BinarySaveState
...
```

A save whose files don't match the manifest is rejected by the game. The editor regenerates `commit`
automatically whenever it saves.

## Usage

1. **Close the game.** It writes new revisions on its own.
2. Run the editor and choose **File > Open save folder**, then pick a revision's `data` folder (usually the newest one).
   You can also pass the folder as a command-line argument: `SaveEditor.exe "<path to data>"`.
3. Edit values in the tabs.
4. Save with one of:
   - **Save as new revision** (safest): writes the next revision number and leaves the loaded one untouched.
     Delete the new folder to go back.
   - **Save all**: overwrites the loaded revision in place.
   - **Save all to folder**: writes the files to another folder.
5. **Export PlayerDatabase as .sqlite** produces a plain SQLite file you can open in any SQLite tool.

Before overwriting anything, the editor copies the original files to
`%LOCALAPPDATA%\SaveEditor\Backups\<timestamp>\<revision>\`. Backups are deliberately kept out of the save folder.

## Download

Prebuilt self-contained Windows x64 builds (no .NET install needed) are attached to
[Releases](../../releases).

## Build from source

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download) on Windows.

```bash
dotnet build src/src.vbproj -c Release
```

Single-file, self-contained executable:

```bash
dotnet publish src/src.vbproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
```

GitHub Actions (`.github/workflows/build.yml`) runs only when you publish a GitHub Release: it builds the exe and
attaches `SaveEditor.exe` to that release. To make one, go to Releases > Draft a new release, create a tag such as
`v1.0.0`, and click Publish release.

## Project layout

| File | Purpose |
| --- | --- |
| `src/frmMain.vb`, `frmMain.Designer.vb` | UI, open/save logic |
| `src/PlayerDb.vb` | `PlayerDatabase` header handling and SQLite access |
| `src/ProfileFile.vb` | `ProfileData` parser and writer |
| `src/SaveStateFile.vb` | `SaveState` in-place XML value editing |
| `src/CommitManifest.vb` | Builds the `commit` manifest |
| `src/pdb_convert.py` | Standalone script to unpack and repack `PlayerDatabase` |

## Known limitations

- The game's behaviour with edited saves hasn't been exhaustively tested; the manifest hashing matches the game's
  own files byte for byte, but the game may validate other things.
- `ProfileData` contains an opaque binary section after the value tree; it is preserved but not editable.
- Grown databases are rejected: `PlayerDatabase` and `ProfileData` must fit their original file size.
