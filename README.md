# Tower Defense — Project 4

A browser-based Tower Defense game built with **Blazor Server** (.NET 8).

## How to Build and Run

```bash
dotnet build
dotnet run --project src/TowerDefense.UI
```

Then open http://localhost:5000 in your browser.

## How to Run Unit Tests

```bash
dotnet test src/TowerDefense.Tests 
```

There are 14 passing unit tests covering PathFinder, Tower, Enemy, WaveManager, and SaveGameService.

## Tech Stack

| Layer | Technology |
|---|---|
| UI | Blazor Server (ASP.NET Core) |
| Language | C# 12 / .NET 8 |
| Styling | Plain CSS |
| Testing | xUnit |
| Serialization | System.Text.Json |

**New framework learned:** Blazor Server — specifically the pattern of using `System.Threading.Timer` to drive a real-time game loop and pushing state updates to the browser via `InvokeAsync(StateHasChanged)`.

## Key Features

- 20×12 grid map with a winding enemy path
- 3 tower types: Arrow, Mage, Cannon (each with different damage/range/cooldown)
- 3 enemy types: Goblin (fast), Troll (tanky), Boss Orc (boss on wave 10)
- 10 escalating waves
- Click to place towers, click placed towers to sell for half price
- Save/Load game state to `data/savegame.json`
- Leaderboard persisted to `data/leaderboard.csv`

## Data Structures Used

- `Dictionary<(int,int), Cell>` — O(1) grid cell lookup
- `Dictionary<Guid, Tower>` — tower registry by ID
- `PriorityQueue<Enemy, int>` — targeting: towers prioritize enemies furthest along the path
- `SortedSet<ScoreEntry>` — leaderboard auto-sorted by score descending
- `Queue<WaveDefinition>` — ordered pending wave queue
- `Queue<Enemy>` — per-wave ordered spawn queue

## File I/O

- **Save file:** `data/savegame.json` — full `GameState` serialized with `System.Text.Json`
- **Leaderboard:** `data/leaderboard.csv` — scores as CSV, top 100 entries kept
- All file operations wrapped in `try/catch` with user-friendly error messages shown as toast notifications

## UML Diagram

See `uml/TowerDefense.asta` (or `uml/TowerDefense.png` for the exported image).

The diagram covers: `Tower` and `Enemy` inheritance hierarchies, `GameEngine` composition, service relationships (`WaveManager`, `PathFinder`, `SaveGameService`, `LeaderboardService`), and data model classes.

## Project Structure

```
src/
  TowerDefense.Core/     # Domain models and services (no UI dependency)
    Models/              # Cell, Tower, Enemy, Projectile, GameState, ScoreEntry
    Services/            # GameEngine, WaveManager, PathFinder, SaveGameService, LeaderboardService
  TowerDefense.UI/       # Blazor Server app
    Pages/               # Index, Game, Leaderboard
    Components/          # GameBoard, TowerPanel, WaveHUD
  TowerDefense.Tests/    # xUnit tests
```

## Citations / External Resources

- [Blazor Server documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [BFS pathfinding algorithm](https://en.wikipedia.org/wiki/Breadth-first_search)
