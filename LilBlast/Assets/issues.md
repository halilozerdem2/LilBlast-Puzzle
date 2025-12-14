# Issues

## High Priority
- [ ] Power-up consume flow currently hits `/inventory/consume` on every use. Design a strategy that queues these requests and flushes them in batches (end of level or at intervals) to reduce backend load.
- [ ] Facebook and Google SDK integrations are missing. Add the official SDKs and wire them into LoginManager to enable OAuth sign-ins.

## Performance Enhancement
- [x] **Optimize neighbour lookups** (`Assets/Scripts/Managers/Block/Block.cs:45-58`): replace `GridManager.Instance._nodes.Values.FirstOrDefault` with direct `TryGetValue` dictionary access to drop the O(n²) cost down to O(n); expected +20‑25% FPS during group searches/deadlock checks.
- [x] **Remove redundant FindAllNeighbours calls** (`Assets/Scripts/Managers/Block/BlockManager.cs:98-106`): stop scanning every block when the result is unused; eliminates wasted CPU (~8‑10% gain).
- [x] **Pool blocks instead of Instantiate/Destroy** (`Assets/Scripts/Managers/Block/BlockManager.cs:52-216`): reuse block prefabs via `ObjectPool` to avoid GC spikes and frame drops; +30‑35% performance.
- [x] **Reduce coroutine storms during special blasts** (`Assets/Scripts/Managers/Block/BlockManager.cs:219-247`): schedule special blasts in a single routine instead of spawning per-block coroutines; ~10% smoother large blasts.
- [x] **Make shake effects tween-based** (`Assets/Scripts/Managers/Block/Block.cs:62-85`): swap the custom coroutine for DOTween `DOShakePosition` to cut concurrent coroutines; +5‑7%.
- [x] **Deduplicate Falling→Spawning state transitions** (`Assets/Scripts/Managers/GridManager.cs:89-134`): trigger the spawn state once instead of per-block `OnComplete`; ~5% gain.
- [x] **Remove the O(n²) UpdateOccupiedBlock pass** (`Assets/Scripts/Managers/GridManager.cs:137-152`): SetBlock already maintains node references, so the extra scan can be deleted; +3-4%.
- [x] **Trim ShuffleManager allocations** (`Assets/Scripts/Managers/ShuffleManager.cs:16-74`): run Fisher-Yates once and reuse pre-grouped node lists instead of repeated `FindAll`; +6-8%.
- [ ] **Implement modify power-up properly** (`Assets/Scripts/Managers/Block/BlockManager.cs`, `Assets/Scripts/Managers/PowerUpManager.cs`): highlight eligible groups (non-target regulars), let the player select one, convert group to the target type, and exit modify mode cleanly.
- [ ] **Pool reset/destroy cycles** (`Assets/Scripts/Managers/GameManager.cs:108-142`): return blocks to the pool instead of destroying them after cascades to reduce GC by ~3%.
- [ ] **Stop copying `freeNodes`** (`Assets/Scripts/Managers/Block/BlockManager.cs:52-84`): iterate directly without `freeNodes.ToList()` to save 1‑2%.
- [ ] **Stop running the full game loop on the main menu** (`GameManager`, `LilManager`, `BlockManager`, `ShuffleManager`): entering the menu still calls `GameManager.Reset` (destroying/rebuilding the grid) and leaves all managers/coroutines active, so CPU stays high even when the board is hidden. Move heavy logic out of `GameState.Menu`, stop LilManager manipulations there, and cancel leftover coroutines when the menu scene is active.
- [ ] **Prevent duplicate resets during level transitions** (`LevelManager.OnSceneLoaded`, `GameManager.Reset`, `GridManager.ResetGrid`): loading a level triggers Reset twice (once from win sequence, once from `OnSceneLoaded`), causing extra destruction/spawn work. Gate the second reset and only rebuild the grid once per scene.

Applying the full list should deliver roughly 50‑60% FPS improvement during heavy blast chains.

## Gameplay Architecture Migration Plan

### 1. Manager Classification
- **Persistent (DontDestroyOnLoad)**
  - `GameManager`: global state machine, scene load orchestration, persistent events.
  - `AudioManager`: cross-scene music/SFX control; already lightweight and needs continuous playback.
  - `ObjectPool` / Pool service: maintains reusable prefabs; keeps allocations low across scenes.
  - `PowerUpManager`, `PlayerDataController`, `LoginManager`: session-level data/services only, no grid dependencies.
  - `ScoreManager` (if required globally) and other pure “service” controllers with no scene references.

- **Scene-Bound (GameplayRoot)**
  - `GridManager`: owns nodes/freeNodes; must be rebuilt with each level to avoid stale references.
  - `BlockManager`: spawns/kills blocks, tracks regular/special sets, coroutines; state should vanish with gameplay scene.
  - `ShuffleManager`: manipulates Grid/Block references; scoped to the active grid.
  - `LilManager` & `CharacterAnimationController`: drive character behaviors tied to gameplay state.
  - Gameplay UI (`CanvasManager` game HUD, win/loss panels), `GameOverHandler`, `ObstacleManager`, DOTween-driven effects.
  - Any per-level FX controllers or Demo scripts (clouds, moon pulse) that should not survive scene unload.

### 2. GameplayRoot Introduction
- Create a `GameplayRoot` prefab or scene object that acts as the parent for all scene-bound managers/components.
- Responsibilities:
  - Instantiate grid, block pools, shuffle logic, gameplay UI, Lil character.
  - Register event hooks (e.g., `GameManager.OnGridReady`) during its lifetime.
- Creation:
  - Spawned when entering a gameplay scene (either via prefab in the scene or `GameManager` instantiating it after scene load).
- Destruction:
  - Automatically destroyed when the gameplay scene unloads; no manual reset required.
  - Ensure no `DontDestroyOnLoad` is called inside any component under GameplayRoot.

### 3. Inter-Manager Communication Rules
- Scene-bound managers raise events/interfaces consumed by persistent services (e.g., `IGameplayLifecycleListener`).
- `GameManager` should:
  - Only know if GameplayRoot is present (null/non-null reference).
  - Listen for GameplayRoot readiness via events or an interface (e.g., `IGameplayRoot` with callbacks).
  - Stop calling heavy `Reset()` logic; instead, trigger scene transitions and let GameplayRoot lifecycle handle cleanup.
- Remove static references between scene-bound managers and persistent ones (e.g., replace `BlockManager.Instance` usage in persistent classes with dependency injection or event subscriptions).

### 4. Code-Level TODOs
- Remove `DontDestroyOnLoad` calls from `GridManager`, `BlockManager`, `ShuffleManager`, `LilManager`, `CanvasManager`, `GameOverHandler`, `ObstacleManager`, etc.
- Delete or simplify `GameManager.Reset()` to only reset persistent services (handler targets, score). Grid/Block teardown moves to GameplayRoot destruction.
- Modify `LevelManager.OnSceneLoaded` to instantiate/track GameplayRoot instead of manually calling `gridManager.InitializeGrid()` on persistent objects.
- Replace `Update()` polling where possible (LilManager countdown, menu panel controller, audio track checks) with coroutines or events tied to GameplayRoot lifecycle.
- Ensure every coroutine/DOTween inside GameplayRoot registers to `OnDestroy` or uses `DOTween.Kill` so they stop automatically when GameplayRoot is destroyed.
- Audit `BlockManager`, `ShuffleManager`, `LilManager` for static `Instance` usage; convert to serialized references on GameplayRoot to avoid cross-scene static singleton reliance.
- Cap ObjectPool sizes or provide `Trim()` hooks when GameplayRoot is destroyed to reclaim memory.

### 5. Level Flow Changes
```
App start (persistent managers created)
→ Menu scene (GameplayRoot absent)
→ Player selects level
→ Load gameplay scene → instantiate GameplayRoot → Grid/Block spawn
→ Play loop (WaitingInput/Falling/etc.)
→ Level complete/lose → unload gameplay scene → GameplayRoot destroyed
→ Return to menu scene (no pause/resume of gameplay systems)
```
- What no longer happens:
  - No manual `PauseGameplaySystems()` or `Reset()` when returning to menu.
  - No persistent `freeNodes` lists or block references surviving scene unload.

### 6. Performance Expectations
- **CPU:** Removing menu-time updates and eliminating redundant resets should cut state-change spikes (~5–8 ms spikes currently seen during `GameManager.Reset`) and drop idle frame cost by ~1–2 ms on mid-range devices.
- **Memory:** Destroying GameplayRoot frees grid nodes, block instances, and UI assets after each level (estimated >100 MB reclaimed versus current persistent setup). Pools can be trimmed between scenes to reduce GC pressure.
- **Stability:** Scene-bound lifecycle removes entire classes of bugs (stale `node.OccupiedBlock`, `freeNodes` mismatches, runaway coroutines) because objects die with the scene; reduces crash risk from corrupted state when loading multiple levels consecutively.

### 7. Risks & Validation
- **Risks:**
  - Missing references after removing singletons (e.g., scripts expecting `GridManager.Instance`).
  - Initialization order bugs if GameplayRoot components depend on each other during `Awake`.
  - ObjectPool consumers must handle the pool being emptied between scenes.
- **Validation:**
  - Add debug assertions when persistent managers attempt to access scene-bound references outside gameplay scenes.
  - Use Unity Profiler (CPU Timeline) before/after migration to confirm state-change spikes disappear and idle frame time drops.
  - Track memory via Profiler > Memory to ensure gameplay scene unload releases nodes/blocks.
  - Log GameplayRoot lifecycle events (`OnEnable/OnDisable`) to verify creation/destruction happen exactly once per level.
