# Published plugin state

## 2026-10-28 — Keybound Pilot 1.0.0.0 initial public preview

- Repository: https://github.com/Roxyz0501/KeyboundPilot
- Release: https://github.com/Roxyz0501/KeyboundPilot/releases/tag/v1.0.0.0
- ZIP: https://github.com/Roxyz0501/KeyboundPilot/releases/download/v1.0.0.0/KeyboundPilot-1.0.0.0.zip
- Source/tag: `e954d15197b0560979bf16bd9188ae639ad7d380`. ZIP 81,820 bytes, 8 files; SHA-256 `6e0091da5250e36520c6b2f14771a12404245cf7d659e751041f39dae46e3e28`; public download and GitHub asset digest match. Release draft=false, prerelease=true.
- Sends Windows scan-code keyboard down/up events (no paste, WM_CHAR, text-field writes or direct game callbacks). Config bounds 20–5000 ms, default 80–150 ms, inclusive random delays; equal bounds fixed. Explicit start, three-second arming, Esc/focus/foreground stop. Current-word presence rechecked before send. No entry/requeue/reward automation.
- Clean nonincremental Release: 0 warnings/errors. 29 offline scheduler/persistence checks pass; tests send no real keys. 37 keys × 7 languages checked; canonical trailing-tab code byte-identical. Original 512×512 keyboard/clock icon visually checked; source JSON/project/packaged manifest retain RepoUrl and IconUrl; Apache-2.0 and notices packaged.
- Live KTG word capture, input focus, physical key recognition and seven-language in-game rendering are unverified. The README, manifest, Release notes and index explicitly identify this experimental limitation. Publication was explicitly requested by the user; no installed DLL or production config changed.
- Repo/icon/ZIP anonymous HTTP 200, icon image/png. Shared index commit `296146236770bf77dc09f9477b701f7421ba01eb`: main and pinned URLs show KeyboundPilot 1.0.0.0, 15 total entries; all 14 prior entries preserved.
- Connector used for supported source/index/state operations. CLI used only for unsupported repository creation and Release/assets; standard authenticated account Roxyz0501 verified. Existing modified local shared-index files were preserved; the published index snapshot is stored in this task's artifacts.

