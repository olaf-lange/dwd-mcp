# DWD MCP Server

An MCP (Model Context Protocol) server that exposes the [Deutscher Wetterdienst (DWD) API](https://dwd.api.bund.dev/) as tools for LLM-based agents. Built with .NET 10 and the [Model Context Protocol C# SDK](https://github.com/modelcontextprotocol/csharp-sdk).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Build

```bash
dotnet build
```

## Run

```bash
dotnet run --project src/DwdMcp
```

The server communicates over **stdio** (stdin/stdout) using the MCP protocol. It is not meant to be run interactively — it should be launched by an MCP client (e.g. VS Code, Claude Desktop).

## Available Tools

| Tool | Description |
|---|---|
| `GetStationOverview` | Weather data (forecasts, daily summaries, warnings) for one or more DWD stations by station ID |
| `GetCrowdWeatherReports` | Current crowd-sourced weather reports from DWD app users |
| `GetNowcastWarnings` | Nowcast severe weather warnings (German) |
| `GetNowcastWarningsEnglish` | Nowcast severe weather warnings (English) |
| `GetMunicipalityWarnings` | Municipality-level severe weather warnings (German) |
| `GetMunicipalityWarningsEnglish` | Municipality-level severe weather warnings (English) |
| `GetCoastalWarnings` | Coastal severe weather warnings (German) |
| `GetCoastalWarningsEnglish` | Coastal severe weather warnings (English) |
| `GetSeaWarningText` | High-sea weather warnings as text |
| `GetAlpineWeatherForecast` | Alpine weather forecast as text |
| `GetAvalancheWarnings` | Avalanche warnings |

## Configuration

Base URLs and timeouts can be customized in `src/DwdMcp/appsettings.json`:

```json
{
  "DwdApi": {
    "StationBaseUrl": "https://app-prod-ws.warnwetter.de/v30",
    "WarningBaseUrl": "https://s3.eu-central-1.amazonaws.com/app-prod-static.warnwetter.de/v16",
    "TimeoutSeconds": 30
  }
}
```

## Usage with VS Code

Add the server to your VS Code `settings.json`:

```json
{
  "mcp": {
    "servers": {
      "dwd": {
        "type": "stdio",
        "command": "dotnet",
        "args": ["run", "--project", "/absolute/path/to/dwd-mcp/src/DwdMcp"]
      }
    }
  }
}
```

## Usage with Claude Desktop

Add to your `claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "dwd": {
      "command": "dotnet",
      "args": ["run", "--project", "/absolute/path/to/dwd-mcp/src/DwdMcp"]
    }
  }
}
```

## Example Prompts

Once connected, you can ask your LLM agent:

- *"What's the current weather forecast for Frankfurt? (station ID 10865)"*
- *"Are there any severe weather warnings in Germany right now?"*
- *"Show me the current avalanche warnings for the Alps."*
- *"What's the coastal weather warning for the North Sea?"*

## Station IDs

The `GetStationOverview` tool requires DWD station IDs (Stationskennungen). You can find them at:
- [DWD Station List](https://www.dwd.de/DE/leistungen/klimadatendeutschland/stationsliste.html)

Common examples: `10865` (Frankfurt), `10382` (Berlin-Tegel), `G005`.

## License

MIT
