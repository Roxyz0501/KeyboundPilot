## 2026-10-29 — Keybound Pilot 1.0.16.7 finish-current-run control

- Release: https://github.com/Roxyz0501/KeyboundPilot/releases/tag/v1.0.16.7
- Source/tag b09ac4d558589aa5cf2fae090a88ee6c86babd83; distribution index 29c95ef80f4261461ef0796f85eda183ab3eb313.
- Add “この周回で停止” / “Stop after this run” to the right of Loop, with pending indication and disabled repeat request. Keep current typing/results/exit flow, then complete on lobby return without another queue. Pre-dispatch requests suppress new queue; already requested entry finishes one run. Preserve configured count; pause/resume retains request, new loop/manual stop clears it. Transient state, no additional persistence/network/polling.
- Clean Release zero warnings/errors, 365 offline checks including 16 new loop-state cases. Native callback/timer fixtures, seven-language/four-font 975-character coverage, numeric interactions and Japanese normal/pending button layout pass. README lint/diff/verify completed. Live gameplay transitions remain unverified.
- Public repository/icon/ZIP/source README/main and pinned index verified; both indices 1.0.16.7. ZIP 173094 bytes; SHA-256 156809016e8983e196d9f96f0ddb81ccaf32b8a8f728cd0350027a3add13c633.

## 2026-10-29 — Keybound Pilot 1.0.16.6 dedicated inactivity fix

- Release: https://github.com/Roxyz0501/KeyboundPilot/releases/tag/v1.0.16.6
- Source/tag 5d851d1c3608d01f710dce6cffd0784b2ff9b558; distribution index d909bf197f2681bfc1bffdb858f748741d2fb66c.
- User confirmed forced inactivity exit after about 1–2 minutes with 1.0.16.5 loaded. Saucy comparison (PunishXIV/Saucy ec18e30459c1ad3b1a42fec53fc2e1e99471043d, BSD-3-Clause) exposed dedicated input-agent counters separate from InputTimerModule. Independently inspected installed client: agent 398, HUD owner at +0x28, two floats at +0x50/+0x54, 60-second withdrawal branch and repeated-key restoration between counters.
- Reset both dedicated counters on existing 3-second schedule and successful answer submission in both modes. Validate agent/HUD identity, shared UI owner and finite nonnegative values. Retain general/device timers; remove redundant OS/window mouse pulses and their success dependency. Debug shows dedicated idle/key-history counters and reset status. Preserve territory/loading/pause/stop/result guards. README/NOTICE credit Saucy and contain its full BSD notice.
- Clean Release zero warnings/errors; 349 offline checks; native callback, generic timers and dedicated-counter fixtures pass. Dedicated fixture reproduces the old 60-second failure and survives a three-minute periodic/submission/repeated-key replay after the change; invalid owners/values/nulls and stopped lifecycle do not write. Seven languages, four font faces, 964 characters; numeric interactions pass. Removed eight obsolete pulse scheduling checks. README lint/diff/final hash verify completed.
- Anonymous public repo/icon/ZIP/source README/main and pinned index verified. Both indices 1.0.16.6. ZIP 171521 bytes; SHA-256 770c4b3b47ffeccd3dc26fed410e798ab457b1a3cce304c0584d2de54e589a28.
- Live play after this fix remains unverified. Static/native inspection establishes the missing dedicated timer and threshold; offline replay is not a live gameplay result.

## 2026-10-29 — Keybound Pilot 1.0.16.5 visual obstruction and background activity

- Release: https://github.com/Roxyz0501/KeyboundPilot/releases/tag/v1.0.16.5
- Source/tag 0637d487c5e95d8ce4eb7b9864f79bc71e47aea0; distribution index 15d862083d492bcc33f0839ca4392f316bf1aa8a.
- Add Visual obstruction section under Input adjustment: default-on unseen-prompt exclusion preserves prior observations; separate interval defaults off, 150–250 ms, adjustable 20–5000 ms, applied for Status 5348/5472 including visible portions. Shared by normal and background input. Persist through changed-only 60-second saves and lifecycle flush.
- Compare AutoKeyboundBrawler 429937e6d8ced7d26a9df57a98aa5590618fc4fe. Add independently implemented process-validated game mouse notifications, zero-distance OS input and controller/mouse timer resets. Periodic attempts every 3 s; window/OS routes rate-limited 3/5 s. Answer submission retains immediate timer reset. Debug separates timer values from notification acceptance. Attribution updated; no code copied.
- Clean Release zero warnings/errors, 357 offline checks, native callback/device-timer fixtures, seven-language/four-font 962-character coverage and numeric interaction checks pass. Reviewed Japanese/English settings render. README lint/diff/verify completed; detected new values match implemented bounds and submission behavior.
- Anonymous public repository/icon/ZIP/source README/main and pinned index verified: both indices 1.0.16.5. ZIP 168385 bytes; SHA-256 d23d8973af03c4e9dab56ea472f062cb540f2f5c1b71d65bcec6d3f4691ad798.
- Live visual-effect duration, background game notification consumption, zero-motion OS behavior and server inactivity removal remain unverified. The user-reported forced exit is not claimed resolved solely from timer reset or API acceptance.

## 2026-10-29 — Keybound Pilot 1.0.16.4 follow-up audit

- Release: https://github.com/Roxyz0501/KeyboundPilot/releases/tag/v1.0.16.4
- Source/tag ea2e4fae95b30db9ad9a80dfee1e09e39824b67a; index 09eb43946d5ffe6ff3dc539f552590d9c87a827f.
- Typo submissions require input focus before cleanup; normal submissions retain acknowledgment observation without forced focus. Block auto-start/readiness/final dispatch for both zone-loading conditions. Reviewed settings changes, resume and target identity; no speculative behavior changes elsewhere.
- Clean Release zero warnings/errors and 328 offline checks pass. Three added regression checks distinguish normal vs typo post-submit focus needs. Live wrong-answer focus/zone transitions remain unverified. Previous background inactivity-removal report is still not confirmed resolved.
- Public repo/icon/ZIP/source README/pinned index verified. ZIP 161339 bytes; SHA-256 c3243c5450cb0ab8c44e033dd7d28cef9cf5f343b50d0ca49f5e9d896d60b96f. Main raw index initially cached 1.0.16.3.

## 2026-10-29 — Keybound Pilot 1.0.16.3 activity reset and diagnostics

- Release: https://github.com/Roxyz0501/KeyboundPilot/releases/tag/v1.0.16.3
- Source/tag a8ed2deea42de5373fabbb3dc297cf9a1355a8b6; index 2e94711bd720596f897e07db0e1ef7a1db9733f7.
- Reset activity on each successful answer dispatch in both modes, excluding IME confirmation. Read back fields to verify reset; successful submission restores failed periodic protection and coalesces next periodic write. Cached debug timers/limit/reset result/count/age/territory/status at most 4 Hz only while open with debug enabled. No new disk/network activity.
- Clean Release, 325 offline checks, native callback ABI and seven-language/four-font 919-character coverage pass. Continued background inactivity removal reported; root cause and live effectiveness remain unconfirmed. Client timer resets are not proof of server activity.
- Public repo/icon/ZIP/source README and pinned index verified. ZIP 160888 bytes, SHA-256 4ec08ed0b7d88e131489b8f58c2e44f211eb58f89c9aae4c252b82befd2ec1e3. Main raw index initially cached 1.0.16.1.

## 2026-10-29 — Keybound Pilot 1.0.16.2 audit

- Release: https://github.com/Roxyz0501/KeyboundPilot/releases/tag/v1.0.16.2
- Source/tag 985dd325b1da39604ae684c531c3dbf76ac31eed; index 52152034f7d0113effa13795b862e2f5ef04b20c.
- Fixed premature NoProgress stop after a consumed answer when target survives: observe empty input, preserve post-submit delay and continue. Unconfirmed answers are not automatically retried. Reviewed typing/loop/activity/persistence flows. 315 offline checks and native checks passed, clean Release; live game not verified.
- Public ZIP 157787 bytes, SHA-256 a42999a1ed28b72fc8e8d2fbb1ed2cd78e4afa3dd7cf1882a2d1107ec8ff0082 verified with public source/icon and pinned index. Superseded by 1.0.16.3 in the same task.

## 2026-10-29 — Keybound Pilot 1.0.16.1 background submit and activity

- Release: https://github.com/Roxyz0501/KeyboundPilot/releases/tag/v1.0.16.1
- Source/tag: f5411ba93864f97f931c71cc50d041787e51277a. Shared index: 82904d4740eacda178e680dcfbf8f6dce6035434.
- Replace background keyboard Enter with validated dedicated input Enter callback, preserving raw/evaluated text, event kind and ClearText result. No OS fallback. Retain activity session on input errors; manual pause/stop/Escape, results, exit and lifecycle end it. Stop stays enabled and retained activity is displayed. Keep ten-second updates and three-failure bound.
- Clean Release zero warnings/errors; 310 offline checks plus fabricated native callback ABI/ownership/argument/cleanup tests pass. Seven-language/four-font coverage (906 characters) and numeric UI checks pass. Live inactive/minimized submission and prevention of inactivity removal remain unverified; suspected previous NoProgress-to-activity-stop chain is not a confirmed live trace.
- Public repository/icon/ZIP/source README and pinned index verified. ZIP 157352 bytes; SHA-256 a997caaf8d87cc48dd0927ef50380a890842eaffd8a2adaa2a2c8fcad4abad69. Main raw index initially cached 1.0.16.0.

## 2026-10-29 — Keybound Pilot 1.0.16.0 settings and ranges

- Release: https://github.com/Roxyz0501/KeyboundPilot/releases/tag/v1.0.16.0
- Source/tag: aea933e7f9f3bc1163f4c9ababe2bc8ef9139ead. Shared index: 2dceff2126523f30994771d57105f297fe719d9c.
- Background/auto-start/debug controls above Input adjustment, scoped information-icon tooltips, separate min/max rows. Post-submit waits and typo counts sampled inclusively; endpoint edits do not change the other endpoint. Legacy fixed delay and N±5 typo ranges retained via fallback, including persistence/retry.
- Clean Release with zero warnings/errors, 306 offline checks, four-font 897-character coverage, seven-language native rendering and numeric interactions pass. Settings preview uses the production SettingsPanel. Live game behavior not tested in this update.
- Public repository/icon/ZIP/source README and both main/pinned index verified at 1.0.16.0. ZIP 155687 bytes; SHA-256: 7b27b91897352d43f9d5f806ffd24f4a564a2214fca6c74076c1de66bbe84853.

## 2026-10-29 — Keybound Pilot 1.0.15.0 background mode

- Release: https://github.com/Roxyz0501/KeyboundPilot/releases/tag/v1.0.15.0
- Source/tag: 72d3b5d3d6f47cdce967f2e32f0b476842429dd8. Shared index: 557369811924eebebe31b9b3154cf3217657a5bc.
- Optional Background mode (default off): internal converted Unicode text delivery, exact Red XIII, simulated romaji intervals/confirmation waits, final game-local Enter without OS fallback. Preserve target/visibility/prohibition/pause/clear/typo/delay guards. Mode changes stop playback; foreground-only Escape. Loop can progress in background.
- Clean Release, 284 offline checks and seven-language/four-font (887 glyphs) native rendering/numeric interactions pass. Live non-active/minimized gameplay, game acceptance and loop behavior remain unverified. No actual game input performed.
- Public repository/icon/ZIP, source README and commit-pinned index retrieved and verified. Main raw index initially cached 1.0.14.0. ZIP 151667 bytes; SHA-256: 37ea2f4f6e73aaea5d1a1ddd0aa7ffc615f4939843c44a6fd49950d358ac5c9a.

## 2026-10-29 — Keybound Pilot 1.0.14.0 input adjustment

- Release: https://github.com/Roxyz0501/KeyboundPilot/releases/tag/v1.0.14.0
- Source/tag: 5fdc756e354214d0951350471f8317bff35a1163. Shared index: cdc8df09d4783edc092f0669101aaabf8be4f438.
- Input adjustment groups key intervals, post-submit delay and optional typo submission (default off, interval 20). Intervals draw N±5, minimum 1; count answer submissions only. Preserve target identity, IME verification, pause/focus/prohibition gates, settings persistence and cleanup after wrong answers.
- Red XIII uses katakana prefix confirmation followed by IME-off exact uppercase XIII. Verify prefix and final spelling; never send prefix Enter without active composition. Preserve seen-prompt gating.
- Clean Release (zero warnings/errors), 252 offline checks, seven-language/four-font glyph coverage and native numeric interactions pass. Live game typo penalties, IME behavior and new settings UI remain unverified; no real keyboard events sent.
- Public repository/icon/ZIP, main and pinned index retrieved; hash/CRC/manifest and bilingual README verified. ZIP 147555 bytes; SHA-256: b6761fb27a7d9bd8112d57e3aa84bbffcf9609d277f13c625904e29e9ca092c3.

## 2026-10-29 — Keybound Pilot 1.0.13.2 interval endpoint fix

- Release: https://github.com/Roxyz0501/KeyboundPilot/releases/tag/v1.0.13.2
- Source/tag: d1f6d7c03921aceba5dea273631c20c66143e643. Shared index: 884797250c90c23b9dced97ef341a56cc4452b30.
- Editing maximum no longer lowers minimum; editing minimum no longer raises maximum. Clamp only the edited endpoint against the existing other endpoint. Preserve changed-only delayed persistence.
- Clean Release (zero warnings/errors), 214 offline checks pass including six endpoint regressions. No live game keyboard events sent. Public repository/icon/ZIP and pinned index retrieved; hash/CRC/manifest/README verified. Main raw index temporarily cached 1.0.13.1 during initial verification.
- ZIP 142886 bytes; SHA-256: 3d413f509833735fd1e6b678b8dcece55decf6ce8f4c0e48c9c976af91056cd3.

## 2026-10-29 — Keybound Pilot 1.0.13.1 UI and bilingual README

- Release: https://github.com/Roxyz0501/KeyboundPilot/releases/tag/v1.0.13.1
- Source/tag: 7b57095bd9169edcf16e7780b97679c6637ccd42. Shared index: bb44862a47cb4bfe2a196992b389307f33176987.
- Separate normal transport and loop controls; hide run-count label, place labeled Loop below numeric field, hover help next to Stop, optional persisted debug display, native drag/double-click numeric editing. Diagnostic UI/commands and runtime capture disabled; backend retained.
- English-first/Japanese-second user README; remove obsolete history and developer-only explanations, retain required attribution in README/NOTICE.
- Clean Release, 208 offline checks, seven-language/four-face offscreen font coverage and numeric input interaction pass. No live game input performed. Public repository/icon/ZIP and commit-pinned index retrieved; ZIP hash/CRC/manifest and source README text verified. Main raw index briefly cached 1.0.13.0 during CDN propagation.
- ZIP 142763 bytes; SHA-256: 1d92b71ff8421a4864786945779a8020b9e1dcde2dc82f0e814954a51eca7120.
- Supersedes 1.0.13.0 published earlier this session (source 9077d2589ddadecaeaf7bd119bc532793d302d15, index a59d872eefeb840c4806d07e5dcd4468b5afcc37), with reader-focused bilingual documentation. Runtime unchanged from 1.0.13.0.

## 2026-10-29 — Keybound Pilot 1.0.12.0 keyboard entry confirmation

- Release: https://github.com/Roxyz0501/KeyboundPilot/releases/tag/v1.0.12.0
- Source/tag: 95bfead55a4272047589581d43da84649e7e94f8. Shared index: fef607e07dbc91af4d46abe415fee43603a9192a.
- Match localized Addon 17826 keyboard warning in SelectYesno and click ready Yes once per round, only during active loop entry wait in the Square. Respect pause/loading/foreground/unrelated queue gates; no arbitrary dialog confirmation.
- Clean Release, 205 offline checks pass; native live confirmation unverified. Public repository/icon/ZIP and pinned index retrieved; ZIP hash/CRC/manifest verified. SHA-256: b2d79375c70b46dd9a09a027602886d4c2d3bd85311167893f470da7fe27b537.

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

