# External Assets — what to download / make, and where it goes

Checklist of things that must come from **outside the codebase** (the internet, a recording, or an
account). Everything here is CC0 / royalty-free-friendly — safe for a capstone. Nothing here is a code
bug; it's content and accounts.

Legend: ✅ done for you · ⬇️ still to download · 🎙️ must be recorded · 🔑 account/key.

---

## 1. Sound effects & music

Your 14 clips were **organized into folders, renamed, and auto-wired into the experiment scenes** for
you. Current layout:

```
Assets/Audio/SFX/    ui_click, success, wrong, complete, star, bounce, creak, thud, impact, rope_loop
Assets/Audio/Music/  bg_music, ambient_loop
Assets/Audio/VO/English/  vo_no
```

### ✅ Already wired (no action needed) — every experiment scene
- **UI feedback** (`UISoundKit` in all 8 scenes): `ui_click`→click, `success`→correct, `wrong`→wrong,
  `complete`→complete, `ambient_loop`→ambient.
- **SimpleMachines_Lever**: `creak`→LeverController, `thud`→LeverController, `impact`→WeightBlock,
  `star`→StarDisplay.
- **StarDisplay** star chime in Lever / Pulley / InclinedPlane.

### ⬇️/🖐️ Still needs a hand
| Item | Why | What to do |
|------|-----|-----------|
| **Heartbeat lub + dub** | `HeartBeat.lubClip/dubClip` have no file | Download 1 heartbeat clip ([Freesound](https://freesound.org) "heartbeat", CC0) → `Audio/SFX/heart_lub.mp3` + `heart_dub.mp3`, assign in the BloodCirc heart scene |
| **Block slide loop** | `InclinedPlaneController.blockSlideSource` | Download "stone slide loop" (CC0) → `Audio/SFX/slide_loop.mp3`, assign to that AudioSource (looping) |
| **Bounce** | `BouncePad` not present in the wired scenes | `bounce.mp3` is ready — assign it to `BouncePad.bounceClip` in whichever scene uses a bounce pad |
| **Rope pulley** | `PulleyController.ropeAudioSource` had no AudioSource assigned | In SimpleMachines_Pulley, assign an AudioSource to that field; set its clip to `rope_loop.mp3`, looping |
| **Background music** | `AudioManager.PlayMusic(...)` is never called with a clip | Call it (e.g. from a scene-start script) with `bg_music.mp3`, or assign on the Managers' music source |
| **ScriptableObject clips** | `PuzzleDefinition.SuccessClip/FailureClip`, `QuizDefinition.*`, `ModuleDefinition.CompletionJingle` | Assign `success`/`wrong`/`complete` to those SO assets in `Assets/…` where they're authored |

**Import settings for any new clip (Quest-friendly):** short SFX → Load Type **Decompress On Load**,
**Force To Mono**; loops/music → **Streaming** / **Compressed In Memory**, Vorbis ~60 %.

---

## 2. Voice-over narration 🎙️ (record — not downloadable)

Localized spoken lines. Fields on `CharacterDefinition` SOs and `TownCharacterNPC`:

| Clips | Field | Folder |
|-------|-------|--------|
| Intro / Hint / Success / Failure, **English** | `CharacterDefinition.*Clips_EN`, `TownCharacterNPC.introClip_EN/outroClip_EN` | `Audio/VO/English` |
| Same, **Kinyarwanda** | `..._RW` | `Audio/VO/Kinyarwanda` |

- **English**: a TTS (e.g. ElevenLabs) is fine, or record yourself. (`vo_no.mp3` is already here as a sample.)
- **Kinyarwanda**: TTS quality is poor — **record a native speaker**. This has real lead time; start early.

---

## 3. Sentry account 🔑 (crash reporting)

1. Create a free project at **https://sentry.io** → platform **Unity**.
2. Copy the **DSN** (`https://…@…ingest.sentry.io/…`).
3. Unity: **Tools ▸ Sentry** → paste the DSN. (Saves to `Assets/Resources/Sentry/SentryOptions.asset`.)
4. *(Optional native symbol upload)* create an auth token → GitHub secret `SENTRY_AUTH_TOKEN`.

Until the DSN is pasted the SDK stays disabled — the app runs fine. **Note:** the Sentry package still
needs to be added (Window ▸ Package Manager ▸ + ▸ *Add package from git URL* →
`https://github.com/getsentry/unity.git#4.7.0`) — I left this for you so the package resolve doesn't
run mid-session; it's a 1-minute step.

---

## 4. Manual Editor steps left to you (risky to automate)

- **OpenXR render mode → Single Pass Instanced** (Project Settings ▸ XR Plug-in Management ▸ OpenXR ▸
  *Android* ▸ Render Mode). Big GPU win, **but verify your custom VR shaders still render in both eyes** —
  test in the headset; revert to Multi-Pass if a shader breaks.
- **Fixed Foveated Rendering** → enable in the same OpenXR Android settings, set a level.
- **BloodCirc_ArterialFlow mesh fix** — see `INCOMPLETE.md` P0 (highest crash risk): extract the embedded
  mesh, Read/Write **off**, mesh compression on, collider **Convex**.

---

_Part of the Sentry + performance + crash-hardening pass. Pair with `INCOMPLETE.md`._