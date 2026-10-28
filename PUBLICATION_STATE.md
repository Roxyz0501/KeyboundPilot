## 2026-10-29 — Keybound Pilot 1.0.11.0 reception NPC loop

- Release: https://github.com/Roxyz0501/KeyboundPilot/releases/tag/v1.0.11.0
- Source/tag: bcc289b77cc7d57ff3365be1b035f86d16058753. Shared index: e029515b071d7b038ae2dd4462c100c8bc6d4729.
- Identify reception NPC 1058751 in the Square, approach and interact once, then use dedicated KTGEnterDialogue entry button. Remove regular Duty Finder selection. No direct queue fallback.
- vnavmesh is OPTIONAL: within three-unit range, interact without IPC; farther away, loaded vnavmesh supplies walking, otherwise stop with loop error. No required dependency in packaged manifest or index. Cancellable pathfinding, owned movement, pause/stop/logout/unload cleanup and bounded preparation retained.
- Clean Release, 198 offline checks and four-font/821-glyph coverage pass. 79 keys in seven languages. Live walking, interaction and registration remain unverified; public prerelease.
- Public repository/icon/ZIP and pinned index retrieved; ZIP hash/CRC/manifest verified. SHA-256: 2ffe74890eda01fc840585f3854fabade1f71643bc3b0f80b9f7f86d5cb4bc11.

## 2026-10-28 — Keybound Pilot 1.0.10.0 UI queue preparation

- Release: https://github.com/Roxyz0501/KeyboundPilot/releases/tag/v1.0.10.0
- Source/tag: f4c0b592742c763b8c9ebf6da21e59163bc25739. Shared index: a4ee37c1d1009a41d91e2d4ee10ed3df8a6ae60a.
- Remove direct QueueDuties request after reported immediate 90002. Use an independently implemented UI preparation/selection/join sequence conceptually referenced from ffxivcode/AutoDuty; attribution and no-code-copy scope in README. No root license found in inspected reference repository.
- Verify target ID, single selected duty and displayed name before joining. Latch selection/join attempts; pause/loading/foreground/stop gates and timeout remain. No direct-request fallback. Special-duty UI support and successful elimination of 90002 remain unverified.
- Clean Release, 194 offline checks pass. Public repository/icon/ZIP and pinned index retrieved; ZIP hash/CRC/manifest verified. SHA-256: 0d7722c3c842635ab127a59b870eb42d4ec6c113e26dc49a93481fdcda6ecdfb.

## 2026-10-28 — Keybound Pilot 1.0.9.0 cursed gaze

- Release: https://github.com/Roxyz0501/KeyboundPilot/releases/tag/v1.0.9.0
- Source/tag: aa545eb2f5584935eeac58c928ca3f1a7532a7e3. Shared index: 2a260898fce0501eb7ed538b1f9004bd5cd25835.
- Add four-second pause after inferred late completion of cursed gaze. Track cast identity as well as progress; early interruptions/despawn do not trigger, overlapping casts extend the deadline. Disabled input continues to block typing after four seconds.
- Clean Release; 191 offline checks pass. Four seconds is provisional, not a measured effect duration. Live avoidance remains unverified; public prerelease.
- Public repository/icon/ZIP and pinned index retrieved; ZIP hash/CRC/manifest verified. SHA-256: c7b55d9c5765da3d7332c67da52a35bcc2fdd2e0e369c618d8c2868b3850aefc.

## 2026-10-28 — Keybound Pilot 1.0.8.0 IME recovery

- Release: https://github.com/Roxyz0501/KeyboundPilot/releases/tag/v1.0.8.0
- Source/tag: dca7ab0be40e2be34b4da0b6da6b01a5e9da6ebd. Shared index: 8ec2ba1513af233b7f30e00f33643223d88807ad.
- IME failures clear input with physical Backspace, confirm empty composition/input, then prepare a fresh answer. Existing focus, prohibition, manual pause and bounded deletion remain.
- Clean Release, 184 offline checks pass. Live recovery unverified; public prerelease.
- Public repository/icon/ZIP and pinned index retrieved; ZIP hash/CRC/manifest verified. SHA-256: cc5c72b160593c08e0fe95e9c35ab2b21ad4f08f47580a24091f4c488f7ddc19.

## 2026-10-28 — Keybound Pilot 1.0.7.0 counted loops, optional logs and usage help

- Release: https://github.com/Roxyz0501/KeyboundPilot/releases/tag/v1.0.7.0
- ZIP: https://github.com/Roxyz0501/KeyboundPilot/releases/download/v1.0.7.0/KeyboundPilot-1.0.7.0.zip
- Source/tag `abd6b6d67e045dc2362d37949ea8ea98b9b6e2e7`; ZIP 132308 bytes; SHA-256 `d52e7d88084d36af744568c2e0bec8b3146dbad54550be035c15b40d0b79af1c`.
- Add a separately started, counted queue/play/leave loop with progress, pause/resume and stop controls. Requests and confirmation clicks are bounded and are not replayed on uncertain outcomes. Unrelated queues are not taken over; stopping does not cancel an existing queue.
- Diagnostic logging is configurable and defaults off, including old-config migration. Disabled diagnostics skip capture and create no files. Active automatic typing refreshes target-content client activity timers at most once per ten seconds, without sending movement or extra keys. Manual pause/stop/departure disables this refresh.
- Installer description and README now explain purpose and simple normal/loop usage. Remove the gameplay-log development banner from UI; retain operational guidance and separate validation documentation.
- Clean Release: zero warnings/errors; 181 offline checks pass. Native background rendering verifies 810 glyphs on four CJK faces, all seven languages and production controls. 78 localization keys and byte-identical canonical tabs verified. Actual repeated games, native result/entry actions and idle-ejection prevention remain unverified; public prerelease, draft=false.
- Public repository and icon HTTP 200 (image/png); downloaded ZIP SHA/CRC/eight files/manifest/version/URLs verified. Shared index commit `292825760b326fc647a0fe1548a09c7e92831fdd`; anonymous main and pinned index both serve 1.0.7.0. All 14 unrelated entries preserved (15 total).
- Source/index/state via GitHub connector; unsupported Release/assets via authenticated standard Roxyz0501 CLI. Installed DLL/configuration and modified local shared repo.json preserved. No captured logs, input strings, game binaries, native inspection artifacts or personal paths published.

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

