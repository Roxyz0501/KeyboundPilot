# Validation — 2026-10-28 JST

## Executed

- Clean nonincremental Release build against locally installed Dalamud SDK/API 15: 0 warnings, 0 errors.
- 29 headless checks pass: 3-second arming, first-key delay, inclusive upper endpoint, fixed equal endpoints, no catch-up burst, ordered characters, no early next-key delivery, cancellation, lost focus, unsupported text, failed delivery, disappearance of a target, stale word timeout, range clamping, x64 Win32 INPUT struct size (40 bytes), changed-only autosave, reverting a change, backup retention/recovery and bounded save retries.
- Tests supply a fake delivery delegate. **No real keyboard events were emitted by the tests.** Native key mapping, 10ms key hold/release, foreground switching and OS key acceptance are still unverified end-to-end.
- Localization static checks: 37 keys × 7 nonempty translations; directly referenced UI keys exist. Canonical shared-tab source is byte-identical to workspace template. Live visual/font validation has not run.
- ZIP reopened: CRC test passes, extracted DLL equals built DLL, author/version match. Contains only DLL, manifest, license, notice, README and local publication state.

## Measured storage fixture

8 successful configuration commits: 480 logical payload bytes; 420 bytes of prior valid configurations created as backups across all commits. Only the latest 3 valid generations remain. Unchanged interval and reverted edits add 0 commits. These are synthetic fixture measurements, not SSD physical-write measurements. Disposal after the final injected save failure is allowed one additional final attempt; the automatic retry check is capped at 3 attempts.

## Access budget (design bounds, not live measurements)

- Stopped: zero automatic word scans; zero external network/DB reads or writes. Manual Inspect performs one bounded local scan.
- Running: one local word scan per 50ms at most (64 word slots per scan), plus direct focus/current-word validation at send boundaries. No unbounded enumeration; words at most 64 × 96 ASCII characters. UI only renders cached results.
- Territory metadata is read once per territory transition using the already installed sheet, not a network request.
- Drawing/changing tabs: no configuration writes, no word scan triggered by rendering.
- Config changes: immediate in-memory application; at most one normal autosave per 60 seconds if values changed. Logout/normal shutdown/unload flush pending state, with duplicate flushes coalesced. No input history files and no per-key disk writes.
- Simultaneous config writers are serialized. Stop/shutdown and Framework processing share a lock so completed shutdown cannot start another send. No worker/timer is created.

## Live gaps

The installed game executable version is 2026.10.16.0000.0000. Its KTG UI strings and local ULDs were inspected, including KTGInput component 7 and KTGEWord component 2/text 20/22. ULD existence does not prove the live instance name, active text node, romanization, input-focus semantics or key consumption path. No running ffxiv_dx11 process was found. It is not possible to claim game operation or first-publication readiness from the offline evidence.

Next acceptance step: load the DLL in a live game, open the minigame, inspect detected words, validate manual physical-key recognition then bounded automatic typing and foreground/Esc/logout/unload stops. If the word count is zero or input focus remains false, correct the reader using actual UI state before proceeding. The user explicitly requested publication of the experimental build. Release notes and installer metadata must retain the above limitations.

## Public preview packaging

User requested publication of this preview on 2026-10-28. Live-game gaps remain disclosed; publication does not change that validation status.

Original icon: 512×512 RGB PNG, generated from geometric primitives using tools/Create-Icon.py; visually inspected. No third-party images or game artwork used.
Manifest includes RepoUrl and IconUrl in both source JSON and project metadata. Publication hashes and anonymous HTTP checks are recorded outside the immutable ZIP in PUBLICATION_STATE.md and release-metadata.json.
