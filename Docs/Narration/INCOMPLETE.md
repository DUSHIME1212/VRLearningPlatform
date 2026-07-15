# Incomplete Work & Stability Punch-List

Status of the project as of the Sentry + performance + crash-hardening pass. Grouped by priority.
`[x]` = done in this pass · `[ ]` = still to do · 🖐️ = needs you (Editor UI / content / account).

---

## P0 — Crash risks for the capstone defense

### Done in this pass ✅
- [x] **Texture streaming enabled** with a 512 MB budget on all quality tiers
  (`ProjectSettings/QualitySettings.asset`) — caps resident texture RAM (was uncapped across ~2.8 GB of textures).
- [x] **URP MSAA 4→2** and **main-light shadowmap 4096→1024**
  (`Assets/Settings/Project Configuration/Performance URP Config.asset`).
- [x] **Manager null-guards** so a missing service can't NRE mid-demo
  (`AudioManager`, `ProgressTracker`, `SessionManager`).
- [x] **`LowMemoryHandler`** — frees assets on OS low-memory signal instead of being killed.
- [x] **`QuestTextureCapper`** editor tool (Tools ▸ VRLearning ▸ Cap Model Textures for Quest) — optional
  extra headroom; caps model textures to ASTC ≤1024/512. Run it deliberately (reimports many textures).

### Still to do 🖐️
- [ ] **`BloodCirc_ArterialFlow.unity` — highest crash risk.** The scene file is **20.3 MB** because a
  ~21 MB mesh is baked *inline* with **Read/Write ON** (doubles RAM) + a **non-convex MeshCollider** that
  cooks on scene load (main-thread stall → ANR/OOM the moment you enter). **Fix:** open the scene → select
  the mesh → extract to a real model/`.asset` → **Read/Write Off** + **Mesh Compression = Medium/High** →
  collider **Convex** or a primitive. *Fallback:* demo `BloodCirc_HeartPump` / `BloodCirc_Capillary` instead.
- [ ] **OpenXR → Single Pass Instanced + Fixed Foveated Rendering** (Android tab). Big GPU win but verify
  custom VR shaders still render in both eyes. See `EXTERNAL-ASSETS.md §4`.
- [ ] **Rehearse on the real Quest** from cold boot; keep it charged/cool; **record a fallback video** of
  the full flow so a crash on stage never ends the demo.

---

## P1 — Silent data loss / fake success (features that look done but aren't)

- [ ] **Analytics events are dropped.** `AnalyticsLogger.Flush()` calls `DataRepository.FlushToSQLite()`
  (which never touches its `_buffer`) then clears the buffer — every `AnalyticsEvent` is lost. Needs a
  `DataRepository` method that persists `AnalyticsEvent`. `Assets/Scripts/Analytics/AnalyticsLogger.cs:41`.
- [ ] **Cloud sync is 100 % stubbed.** `CloudSyncService.UploadScores` is `Debug.Log + yield return null`;
  `ProgressTracker.TrySyncToCloud` clears pending scores as if uploaded. *Scores are safe locally
  (encrypted JSON) — only the cloud leg is fake.* No real network code exists anywhere.
  `Assets/Scripts/Analytics/CloudSyncService.cs:28`, `Assets/Scripts/Core/ProgressTracker.cs:46`.

## P2 — Broken no-ops (called but do nothing)

- [ ] **AR overlays never track or get removed** — `ARWorksheetManager.UpdateOverlay/RemoveOverlay` are
  empty; `SpawnOverlay` leaks/staled overlays. `Assets/Scripts/Modules/ARWorksheet/ARWorksheetManager.cs:45`.
- [ ] **Run-button gating does nothing** — `RunButton.SetInteractable` is empty but is called to
  enable/disable the Run button. `Assets/Scripts/Modules/SimulationBuilder/RunButton.cs:9`.
- [ ] **Scenario props never spawn** — `ConceptScenario.Activate` is a no-op.
  `Assets/Scripts/Data/ConceptScenario.cs:26`.

## P3 — Hygiene

- [ ] **Dead duplicate `GameManager`.** Its scene names (`MainMenu/CodeWorld/...`) aren't in Build
  Settings; the live flow uses `SceneNavigator` + `SessionRunner`. Delete `GameManager` or reconcile.
- [ ] **`SessionRunner.homeScene = "Starting Scene"`** doesn't match any build scene (hub is `BasicScene`).
  Fixed in this pass to point at the real hub — verify. `Assets/Scripts/Core/SessionRunner.cs:16`.
- [ ] Misleading **"SQLite"** naming (store is AES-encrypted JSON) — `DataRepository.FlushToSQLite()`.
- [ ] **Encryption key in PlayerPrefs** → Android Keystore for production. `EncryptionService.cs:14`.
- [ ] `CloudSyncService` namespace drift (`VRLearning` vs `VRLearning.Core`); `VideoScreen` placeholder source.

---

## Sentry & logging (added this pass)

- [x] **`VRLog`** central logger; `DataRepository` catches now preserve stack traces via `VRLog.Exception`.
- [x] **`PerformanceMonitor`** — console `[Perf] fps=… mem=…` every 5 s + frame-spike warnings.
- [ ] **Sentry DSN** — paste it (see `EXTERNAL-ASSETS.md §3`); SDK is wired but disabled until then.

## Content & accounts to fetch

See **`EXTERNAL-ASSETS.md`** for the full download/record/account checklist (sounds, voice-over, Sentry key).