# Keybound Pilot

[日本語](#日本語)

Keybound Pilot is a Dalamud plugin that reads Keybound Brawler prompts and answers them using keyboard input or background mode. It can automate a single game or repeat registration, play and exit for a set number of runs. Created by Roxyz0501.

## Installation and updates

Add this URL to Dalamud's custom plugin repositories, then install Keybound Pilot. If you already have it installed, use the plugin updater.

`https://raw.githubusercontent.com/Roxyz0501/DalamudPluginRepo/main/repo.json`

Individual downloads are available on [GitHub Releases](https://github.com/Roxyz0501/KeyboundPilot/releases).

## Auto typing

1. For normal input, make sure a Japanese IME is available. Open the plugin with `/keyboundpilot`.
2. In Settings, set the minimum and maximum key intervals and the delay after submitting an answer. Drag a numeric field left or right to adjust it, or double-click to type a value.
3. Press Play inside the content to start. “Start automatically when the game begins” is enabled by default.
4. Press Pause to wait and Play to resume. Use Stop, Esc or `/keyboundpilot stop` to end the session.

The plugin window stays open after starting. Hover over the information icon to the right of Stop for instructions.

## Looping

In the Square, choose 1–9999 runs and press “▶ Loop” below the numeric field. The plugin approaches the reception NPC, talks to them, registers, plays and exits, then repeats until the selected number of runs is complete. Loop status shows the current run and completed count. If you start looping during a game, that game counts as the first run.

Automatic travel to reception requires [vnavmesh](https://github.com/awgil/ffxiv_navmesh). Without it, the plugin can still register if you are close enough to talk to reception; otherwise, it stops with an error. If reception has not loaded yet, it waits for up to 120 seconds. It does not travel or teleport from another area.

The normal Pause, Play and Stop buttons also control loops. Stopping does not cancel a registration already in progress. If the game displays the keyboard-disconnection warning while waiting for entry in the Square, the plugin automatically selects Yes once per run. It does not confirm other dialogs or act while paused or stopped.

## Input behavior

- Answers prioritize barrier reinforcement, then casting enemies, then the nearest moving enemy, then other targets. Distance is approximated from screen position. Prompts whose text has never been seen are excluded.
- Phase transitions from 1 through 3 and temporary loss of input focus put typing on hold. Typing stops at the result screen or on leaving. Normal input waits while you use another application. Both modes wait while another game text field is active.
- If a target disappears during typing, the plugin clears the remaining text and checks that the field is empty and moves to the next target. It supports “ゔ” and the wave characters “~”, “～” and “〜”.
- For the Japanese-named abilities “動体感知ジャマー” and “呪いの視線”, it estimates activation from the cast display and pauses input for four seconds. It keeps waiting if the input field is still disabled. Other ability names and other input-prohibition mechanics are not supported.
- While auto typing is active, the plugin periodically updates activity state to prevent inactivity removal from the content. It sends no extra keystrokes or movement for this purpose. Protection continues if typing stops on an error, and ends on manual pause, Stop, results or exit. A status message indicates when protection is continuing; Stop remains available.

Readings are entered using romaji keys, with hiragana confirmation and answer submission handled separately. If the IME state or confirmed reading cannot be verified, the plugin clears the text and checks for an empty field and restarts typing. Input prohibition, focus waiting and manual stop still take priority during clearing.

When typo submission is enabled, one character in an answer is changed before sending. The plugin checks and clears any remaining text, then continues typing. “レッドXIII” is entered as katakana “レッド” followed by uppercase ASCII “XIII”.

The four-second pause for “呪いの視線” is a precautionary waiting period, not a measured effect duration.

With “Background mode” enabled, the plugin sends converted characters directly to the dedicated game input field, including hiragana and the exact spelling “レッドXIII”. You can use another application while it runs. It simulates each romaji key interval and the confirmation waits used by normal input; actual IME processing time varies by system and is not reproduced exactly. The same pauses, target checks, typo settings and post-submit delay apply. Escape stops playback only when FF14 is active.

## Settings

Background mode, automatic start and debug display appear above “Input adjustment”, in that order. Hover over the information icon beside a title for details. Input adjustment contains separate minimum and maximum fields for key intervals, post-submit waits and typo counts. Equal limits give a fixed value.

| Setting | Behavior |
| --- | --- |
| Background mode | Disabled by default. Enable in Settings to type and loop while FF14 is inactive, without an IME. Switching modes stops playback; press Play to restart. Finish any existing IME composition first. |
| Key interval | Set minimum and maximum values from 20–5000 ms. An interval is chosen for each character. Equal values give a fixed interval. |
| Wait after submission | Set minimum and maximum values from 0–60000 ms. A new delay is drawn after each answer submission. |
| Start automatically when the game begins | Starts auto typing when a game begins. Enabled by default. |
| Occasionally submit a typo | Disabled by default. Set the minimum and maximum number of submitted answers between typos (defaults 15–25). Each interval is drawn from that range. IME confirmations do not count. Starting a session or changing the typo settings resets the count. |
| Show debug information | Displays the detected prompt count and text list, input-field visibility and focus, and sent-character count. Disabled by default. |

Input preparation and frame delays can make actual key intervals longer than configured.

Change the display language in Language settings. Japanese, English, German, French, Korean, Simplified Chinese and Traditional Chinese are supported. Changed settings are saved every 60 seconds and on logout, normal exit or plugin disable. Three valid backup generations are retained.

## Author and license

Author: Roxyz0501. License: [Apache-2.0](LICENSE).

Parts of configuration saving and font support use code from [BaitBeacon](https://github.com/Roxyz0501/BaitBeacon) by Roxyz0501, licensed under Apache-2.0. Automatic travel to reception integrates with [vnavmesh](https://github.com/awgil/ffxiv_navmesh) by awgil and contributors.

Optional support: [Roxyz0501 / Ko-fi](https://ko-fi.com/roxyz0501)

---

Background input concept reference: [AutoKeyboundBrawler](https://github.com/1Stalk/AutoKeyboundBrawler) by 1Stalk (AGPL-3.0-or-later), specifically the game-local input approach. No source code was copied. Text conversion and input scheduling are implemented in Keybound Pilot using FFXIVClientStructs APIs.

## 日本語

キーバウンド・ブロウラーの出題を読み取り、キーボード操作で回答する Dalamud プラグインです。1ゲームの自動入力に加え、回数を決めて参加申請から退出まで繰り返す周回機能があります。作者は Roxyz0501 です。

### インストール・更新

Dalamud のカスタムリポジトリに次のURLを登録し、Keybound Pilot をインストールしてください。導入済みの場合はプラグイン更新から更新できます。

`https://raw.githubusercontent.com/Roxyz0501/DalamudPluginRepo/main/repo.json`

個別の配布ファイルは [GitHub Releases](https://github.com/Roxyz0501/KeyboundPilot/releases) にあります。

### 自動入力の使い方

1. 日本語IMEを使用できる状態にしてください。`/keyboundpilot` でプラグインを開きます。
2. 「設定」でキー入力間隔の下限・上限と、送信後の待ち時間を指定します。数値は左右へのドラッグで調整でき、ダブルクリックすると直接入力できます。
3. コンテンツ内で再生ボタンを押すと、自動入力が始まります。「ゲーム開始時に自動実行」は初期値オンです。
4. 一時停止ボタンで待機、再生ボタンで再開します。終了するには停止ボタン、Esc、または `/keyboundpilot stop` を使います。

開始後もプラグイン画面は開いたままです。停止ボタンの右にある情報アイコンにマウスを合わせると、操作説明を読めます。

### 周回の使い方

キーバウンドスクウェアで回数を1〜9999回に設定し、数値欄の下の「▶ 周回」を押してください。「参加受付」への移動・会話・申請・プレイ・退出を繰り返し、指定回数で終了します。現在の周回と完了回数は「周回ステータス」に表示します。プレイ中に周回を開始した場合は、そのゲームを1周目に数えます。

受付への自動移動には [vnavmesh](https://github.com/awgil/ffxiv_navmesh) が必要です。入れていなくても受付に話しかけられる距離なら申請でき、離れている場合はエラーで停止します。受付がまだ読み込まれていない場合は最大120秒待機します。別エリアからの移動やテレポートはしません。

周回中も通常の一時停止・再生・停止ボタンを使います。停止しても申請済みの参加申請は取り消しません。参加待ちで「キーボードの接続が確認できません」という確認が出た場合は、スクウェアでの参加待ちに限り、1周につき1回だけ自動で「はい」を押します。一時停止・停止中や別の確認画面では押しません。

### 入力中の動作

- バリア強化、詠唱中、最も手前の移動中の敵、その他の順に回答します。近さは画面上の位置による近似です。一度も文字の表示を確認していない出題は選びません。
- フェーズ1〜3の切替や入力欄のフォーカス喪失では待機し、結果画面・退出時に自動入力を終了します。通常モードでは別アプリの操作中も待機します。ゲーム内の他の入力欄を操作している間は、どちらのモードも待機します。
- 入力途中で対象が消えたら、残った文字を消し、空欄を確認して次の対象へ進みます。「ゔ」と波線「~」「～」「〜」にも対応しています。
- 「動体感知ジャマー」「呪いの視線」の発動を詠唱表示から推定し、4秒間入力を止めます。入力欄が無効な間は4秒後も待機します。他言語の技名と、その他の入力禁止技は未対応です。
- 自動入力中は無操作退出を防ぐため、対象コンテンツの操作状態を定期更新します。追加のキー入力や移動はしません。入力エラーで自動入力が止まっても退出対策は継続し、継続中であることを表示します。手動一時停止・停止ボタン・結果画面・退出で終了します。

読み仮名はローマ字キーで入力し、ひらがなの確定と回答の送信を分けて行います。IMEの状態や確定した読みを確認できない場合は、入力済みの文字をBackspaceで消し、空欄を確認してから再開します。削除中も入力禁止・フォーカス待ち・手動停止を優先します。

タイプミス送信をオンにすると、回答の一文字を変えて送信します。送信後は残った文字を確認・削除して入力を続けます。「レッドXIII」はカタカナの「レッド」と半角大文字の「XIII」で入力します。

「呪いの視線」の4秒は予防のための待機時間で、効果時間の実測値ではありません。

「バックグラウンドモード」をオンにすると、ひらがななどの変換済み文字を専用入力欄へ直接渡します。「レッドXIII」もその表記のまま入力します。別アプリを操作している間も入力と周回を続けます。入力速度はローマ字の各キーに相当する間隔と、通常モードの確定待ち時間に合わせます。実際のIME処理時間は環境によって変わるため、完全には一致しません。入力禁止時の待機・対象確認・タイプミス設定・送信後の待ち時間も適用します。Escで停止できるのはFF14がアクティブな間です。

### 設定

「入力調整」の上に、バックグラウンドモード・ゲーム開始時に自動実行・デバッグ表示を並べています。各タイトル右側の「i」にマウスを合わせると詳細説明を表示します。入力間隔・送信後待機時間・タイプミス抽選候補は、それぞれ下限と上限を設定してください。同じ値なら固定になります。

| 項目 | 内容 |
| --- | --- |
| バックグラウンドモード | 初期値はオフ。設定でオンにすると、IMEを使わず非アクティブ時も入力・周回します。切り替えると停止するため、再生で再開してください。入力途中のIME変換がある場合は、先に確定してください。 |
| キー入力間隔 | 20〜5000msの範囲で下限・上限を指定します。各文字で間隔を抽選し、上下限を同じにすると固定間隔になります。 |
| 送信後待機時間 | 0〜60000msの範囲で下限・上限を指定します。回答送信ごとに範囲内から待ち時間を抽選します。 |
| ゲーム開始時に自動実行 | ゲーム開始に合わせて自動入力を始めます。初期値はオンです。 |
| タイプミスをして送信する | 初期値はオフです。「タイプミス抽選候補」の下限・上限で、ミスするまでの送信回数を指定します（初期値15〜25回）。毎回、その範囲から回数を抽選します。IMEの確定は数えません。開始時とタイプミス設定の変更時に数え直します。 |
| デバッグ表示 | 検出した出題数・文字の一覧、専用入力欄の表示・フォーカス、送信文字数を表示します。初期値はオフです。 |

入力準備やフレームの遅延によって、実際のキー間隔が設定より長くなる場合があります。

表示言語は「言語設定」で変更できます。日本語・English・Deutsch・Français・한국어・简体中文・繁體中文に対応しています。設定は変更時のみ60秒ごとに保存し、ログアウト・通常終了・無効化時にも保存します。正常なバックアップを3世代残します。

### 作者・ライセンス

作者: Roxyz0501。ライセンス: [Apache-2.0](LICENSE)。

設定保存とフォント表示の一部に [BaitBeacon](https://github.com/Roxyz0501/BaitBeacon)（Roxyz0501、Apache-2.0）のコードを使用しています。受付への自動移動は [vnavmesh](https://github.com/awgil/ffxiv_navmesh)（awgil と各コントリビューター）と連携します。

任意の開発支援: [Roxyz0501 / Ko-fi](https://ko-fi.com/roxyz0501)

バックグラウンド入力は [AutoKeyboundBrawler](https://github.com/1Stalk/AutoKeyboundBrawler)（1Stalk、AGPL-3.0-or-later）のゲーム内で入力を処理する方式を参考にしています。コードのコピーはせず、文字変換と入力間隔の制御はKeybound Pilot側で実装しています。
