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

Applying the full list should deliver roughly 50‑60% FPS improvement during heavy blast chains.
