## 2026-10-28 — Keybound Pilot 1.0.6.0 nearest moving target and post-submit delay

- Release: https://github.com/Roxyz0501/KeyboundPilot/releases/tag/v1.0.6.0
- ZIP: https://github.com/Roxyz0501/KeyboundPilot/releases/download/v1.0.6.0/KeyboundPilot-1.0.6.0.zip
- Source/tag `615dae42166dfe91c06bdde3dfaeee3e6016eee1`; ZIP 123,122 bytes; SHA-256 `a5c1a4145cc77a57d906b9da763a5c07b960fc4e66ec106cedec98fe0815e3c9`.
- Select observed support/barrier prompts first, casting enemies second, then the closest moving eligible target, then other slots. Motion uses accumulated two-pixel coordinate changes with a two-second window. Closest is the screen-front (larger projected Y) approximation, not world-space distance. In-flight typing is retained.
- Add BetweenWordsMs, default 0, range 0–60000 ms, to seven-language settings and changed-only persistence. Start its deadline at game answer submission; IME Enter, per-key interval and unchanged-answer timeout remain separate. Diagnostics add configured delay and current entity ID only.
- Clean Release zero warnings/errors; 139 offline checks pass, including priority, movement, timing boundaries, old-config migration and new-setting persistence. Native background rendering verifies 776 glyphs across four fonts and seven-language labels/controls. 64 localization keys and canonical shared tabs verified. Updated live game behavior remains unverified.
- Anonymous public repository/icon/ZIP HTTP 200; image/png icon; downloaded ZIP SHA/CRC/eight files/manifest/version/URLs verified. Release prerelease=true, draft=false.
- Shared index `2f1beaa8333506c5a248b66ec1c94f80464d4859`: pinned public index and connected main both 1.0.6.0, full contents verified; all 14 unrelated entries preserved (15 total). The unqualified raw main URL still served 1.0.5.0 from cache at the first verification.
- Source/index/records via GitHub connector; unsupported Release/assets via authenticated standard Roxyz0501 CLI. Installed DLL/configuration and modified local shared repo.json preserved. Captured logs, input strings, native inspection artifacts, host libraries and personal paths excluded from publication.

## 2026-10-28 — Keybound Pilot 1.0.5.0 observed-prompt filtering

- Release: https://github.com/Roxyz0501/KeyboundPilot/releases/tag/v1.0.5.0
- ZIP: https://github.com/Roxyz0501/KeyboundPilot/releases/download/v1.0.5.0/KeyboundPilot-1.0.5.0.zip
- Source/tag `968b743b42a5fb604ff9f1eb1938c67ac87fb6c2`; ZIP 119,672 bytes; SHA-256 `96095ff094520380a6dbede52d291ef2e814423daea3d0ef757e7ade6070d359`.
- Require actual text-node display before admitting a candidate. Remember entity/reading pairs while temporarily concealed; invalidate on replacement/despawn/content reset. Distinguish temporary text hiding from target disappearance. Visibility history is bounded to current slots; diagnostic additions are entity/visibility booleans only.
- Historical recordings lacked child text visibility, so no retrospective first-visible time is asserted. Read-only native display mapping is locally inspected and fails closed on unmatched root/entity/reading; live mapping remains unverified.
- Clean Release zero warnings/errors; 126 offline checks pass. Existing 1.0.4.0 phase/control/priority/IME/font corrections retained. 61 UI keys x 7 languages and canonical tabs verified.
- Anonymous repository/icon/ZIP HTTP 200; image/png icon; ZIP hash/CRC/eight-file manifest verified. Release draft=false, prerelease=true. Public raw main and pinned index `cd628d15f5c2c3d820e7e7d77dff55bdb977950b` both 1.0.5.0; 15 entries and 14 unrelated entries preserved.
- Source/index/records via GitHub connector; Release/assets via standard Roxyz0501 CLI. Captured recordings, input strings, native inspection output, host libraries and personal paths excluded. Installed configuration/DLL and modified local repo.json preserved.

## 2026-10-28 — Keybound Pilot 1.0.4.0 phase continuity and input corrections

- Release: https://github.com/Roxyz0501/KeyboundPilot/releases/tag/v1.0.4.0
- ZIP: https://github.com/Roxyz0501/KeyboundPilot/releases/download/v1.0.4.0/KeyboundPilot-1.0.4.0.zip
- Source/tag `8943197248e0cbaf6d7cac152ec88e9de7093b72`; ZIP 116,336 bytes; SHA-256 `89dc57713caaa0ff75250754d31829780c0a627c6c63069c19ea253672e64b12`.
- Preserve automation across phase/focus transitions; rate-limit focus reacquisition and stop at results/exit. Immediate Start/Resume, geometric Play/Pause/Stop controls and window retention. Support-kind prompts precede casting enemies. Disappearing targets trigger physical Backspace cleanup before the next answer.
- For hiragana vu, request physical F6 once before IME commit and compare vu script/Unicode variants without accepting unrelated readings. Record answer-check categories only. Dynamic glyph coverage, batched four-face font replacement and loading/error handling added.
- Clean Release zero warnings/errors; 114 offline checks plus 3 local replay checks pass. Native background ImGui rendering: 764 characters on four fonts, seven languages, production symbol controls. 61 localization keys x 7 languages; canonical trailing tabs retained. Updated live gameplay and exact reported glyph appearance remain unverified; the specific IME text difference was not captured.
- Anonymous public repository/icon/ZIP HTTP 200; image/png icon; downloaded ZIP SHA/CRC/eight files/manifest/version/URLs verified. Release prerelease=true, draft=false.
- Shared index `89cf20b8bf88f6d20c2c6a24917249d3e879cffa`: public raw main and pinned index version/URLs verified, 15 entries; all 14 unrelated entries preserved.
- Source/index/records via GitHub connector; Release/assets via authenticated standard Roxyz0501 CLI. Installed DLL/configuration and modified local shared repo.json preserved. Captured logs, input strings and detailed per-game records are excluded from publication.

## 2026-10-28 — Keybound Pilot 1.0.3.0 startup focus and resume

- Release: https://github.com/Roxyz0501/KeyboundPilot/releases/tag/v1.0.3.0
- ZIP: https://github.com/Roxyz0501/KeyboundPilot/releases/download/v1.0.3.0/KeyboundPilot-1.0.3.0.zip
- Source/tag `8481e4cbe36f245e45a5eb1fa97c424aca17dc95`; ZIP 111,804 bytes; SHA-256 `a8b48bb665613863bc16d17ef5a75f70d1e8082c42ec38ae4553ed8734e5e16c`.
- Correct startup focus gating: honor arming before UI-capture checks, wait for disabled input, protect another active text editor, activate the dedicated addon and focus its edit node, and observe the last focus attempt before failure.
- User-requested AutoStart defaults true and is configurable/persisted. One automatic start per content entry when input and words are ready. Manual/error stops do not auto-restart; the Resume button re-arms and reacquires focus. Existing hiragana IME mode is preserved.
- Existing wave support, cast priority, user-selected four-second jammer wait and expanded diagnostics remain. Added control-gate/focus-request diagnostics without saving input text. Revised native input/IME/focus success remains unverified in live gameplay.
- Clean Release zero warnings/errors; 89 offline checks and 3 local replay checks passed. 55 UI keys x 7 languages; canonical trailing tabs retained.
- Public repo/icon/ZIP HTTP 200; icon image/png; downloaded ZIP SHA/CRC/manifest/version/URLs verified. Release prerelease=true, draft=false.
- Shared index `6c787c7680b4a2381b2707357305d6449df102ae`: public main/pinned both 1.0.3.0, 15 entries; all 14 unrelated entries preserved.
- Source/index/records via GitHub connector; Release/assets via authenticated standard Roxyz0501 CLI. Installed DLL/configuration and modified local shared repo.json not replaced. Captured logs and detailed per-game statistics are excluded from new publication records.

## 2026-10-28 — Keybound Pilot 1.0.2.0

- Release: https://github.com/Roxyz0501/KeyboundPilot/releases/tag/v1.0.2.0
- ZIP: https://github.com/Roxyz0501/KeyboundPilot/releases/download/v1.0.2.0/KeyboundPilot-1.0.2.0.zip
- Source/tag `25deb02b2bbb69743b62d0b38af7a841b15a96b7`; ZIP 108,267 bytes; SHA-256 `0f53bc9bc10ea98d72c11248e755b3a97bea06f367974357e03ab05e8d6b75f3`.
- Japanese IME input, automatic dedicated-field focus, tilde support, next-word cast priority and user-requested four-second jammer wait implemented. Revised native keyboard/IME behavior remains unverified in live gameplay.
- Diagnostic limits expanded to 64 MiB/session, 256 MiB total and 120 minutes; UI delta encoding added. Captured logs are excluded from distribution.
- Release build: zero warnings/errors; 69 offline checks passed. 53 UI keys across seven languages; canonical trailing tabs retained.
- Public repository/icon/ZIP returned HTTP 200; icon image/png. Downloaded ZIP hash/CRC/manifest verified. Release is public, prerelease=true, draft=false.
- Shared index `ac3f2044be9580e59c3a0dd3b995f5ffae70f35b`: public main and pinned versions both 1.0.2.0; 15 entries with all 14 unrelated entries preserved.
- Source/index/state via GitHub connector; Release/assets via standard Roxyz0501 CLI. Installed plugin/configuration and modified local shared repo.json were not replaced.

## 2026-10-28 — Keybound Pilot 1.0.1.0 gameplay diagnostics

- Release: https://github.com/Roxyz0501/KeyboundPilot/releases/tag/v1.0.1.0
- ZIP: https://github.com/Roxyz0501/KeyboundPilot/releases/download/v1.0.1.0/KeyboundPilot-1.0.1.0.zip
- Source/tag `e564cc6a77a71512e372d814bdd6f6fa011dfbfb`; ZIP 96,968 bytes, SHA-256 `b2775a937d2e3aa0ed146c03ce79c86f92a9db693f3ac008d3d1e9a294cf1504`.
- User reports no actual typing. This remains open. Added automatic content-only diagnostics independent of auto typing for manual gameplay: dedicated KTG arrays/UI, 100ms sampling, changed-only JSONL deltas, JST daily files, up to 64KiB buffered / 2s flush, 30m / 8MiB per session / 32MiB total / 1000-file cap. Logout/unload flush. No uncertain append replay, no deletion or external upload. No chat, player names, input text or raw key records.
- Static native inspection found the duplicated UI structure and required Enter submission; changed the reader to dedicated KTG arrays and added one physical Enter after each completed word. No live success claimed. Cast priority and typing-prohibition rules await actual gameplay logs; unknown fields stay unnamed.
- Clean Release 0 warnings/errors; 41 offline checks, 49 keys x 7 languages, canonical trailing tabs byte-identical, original icon retained. Public repository/icon/ZIP HTTP200 (icon image/png); anonymous ZIP digest/CRC/manifest verified.
- Shared index `1542050bc9945469cba124f8f2658b694962cb16`: main/pinned 1.0.1.0, 15 entries, all 14 unrelated entries preserved. Release prerelease=true, draft=false.
- Published under explicit user request and standing authorization. Connector for supported source/index/state operations; standard Roxyz0501 CLI for unsupported Release/assets. No installed DLL/configuration overwritten. Real gameplay logs have not yet been collected by this agent; the user must update and play once.

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

