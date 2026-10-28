using Dalamud.Game;
namespace KeyboundPilot;
internal static class Loc
{
    public static readonly string[] Codes = ["ja", "en", "de", "fr", "ko", "zh-Hans", "zh-Hant"];
    private static readonly Dictionary<string, string[]> Text = new()
    {
        ["Main"] = ["自動入力", "Auto typing", "Automatische Eingabe", "Saisie automatique", "자동 입력", "自动输入", "自動輸入"],
        ["Settings"] = ["設定", "Settings", "Einstellungen", "Paramètres", "설정", "设置", "設定"],
        ["Language"] = ["言語設定", "Language settings", "Spracheinstellungen", "Langue", "언어 설정", "语言设置", "語言設定"],
        ["Support"] = ["支援", "Support", "Unterstützung", "Soutien", "후원", "支持", "支持"],
        ["LanguageHelp"] = ["表示言語を変更します。", "Change the display language.", "Anzeigesprache ändern.", "Modifier la langue d’affichage.", "표시 언어를 변경합니다.", "更改显示语言。", "變更顯示語言。"],
        ["SupportText"] = ["開発への支援は任意です。", "Supporting development is optional.", "Die Unterstützung der Entwicklung ist freiwillig.", "Le soutien au développement est facultatif.", "개발 후원은 선택 사항입니다.", "支持开发完全自愿。", "支持開發完全自願。"],
        ["Recipient"] = ["支援先: Roxyz0501", "Recipient: Roxyz0501", "Empfänger: Roxyz0501", "Bénéficiaire : Roxyz0501", "후원 대상: Roxyz0501", "支持对象：Roxyz0501", "支持對象：Roxyz0501"],
        ["Donate"] = ["Ko-fiで支援する", "Support on Ko-fi", "Auf Ko-fi unterstützen", "Soutenir sur Ko-fi", "Ko-fi에서 후원", "通过 Ko-fi 支持", "透過 Ko-fi 支持"],
        ["Min"] = ["入力間隔の下限 (ms)", "Minimum interval (ms)", "Mindestintervall (ms)", "Intervalle minimal (ms)", "최소 입력 간격 (ms)", "最小输入间隔 (ms)", "最小輸入間隔 (ms)"],
        ["Max"] = ["入力間隔の上限 (ms)", "Maximum interval (ms)", "Höchstintervall (ms)", "Intervalle maximal (ms)", "최대 입력 간격 (ms)", "最大输入间隔 (ms)", "最大輸入間隔 (ms)"],
        ["RangeHelp"] = ["1文字ごとに範囲内の間隔を抽選します。同じ値なら固定です。20〜5000ms。実際の間隔はフレーム遅延で長くなる場合があります。", "A new interval is sampled for each character. Equal limits give a fixed interval. Range: 20–5000 ms. Frame delays can increase actual intervals.", "Pro Zeichen wird ein Intervall gewählt. Gleiche Grenzen ergeben ein festes Intervall. Bereich: 20–5000 ms. Bildverzögerungen können das Intervall verlängern.", "Un intervalle est tiré par caractère. Des limites égales donnent un intervalle fixe. Plage : 20–5000 ms. Les retards d’affichage peuvent l’allonger.", "문자마다 범위 내 간격을 정합니다. 두 값이 같으면 고정입니다. 범위: 20–5000ms. 프레임 지연으로 실제 간격이 길어질 수 있습니다.", "每个字符随机选择间隔。上下限相同则为固定间隔。范围：20–5000毫秒。帧延迟可能延长实际间隔。", "每個字元隨機選擇間隔。上下限相同則為固定間隔。範圍：20–5000毫秒。影格延遲可能延長實際間隔。"],
        ["Start"] = ["開始（3秒後）", "Start (after 3 s)", "Start (nach 3 s)", "Démarrer (dans 3 s)", "시작 (3초 후)", "开始（3秒后）", "開始（3秒後）"],
        ["Stop"] = ["停止", "Stop", "Stoppen", "Arrêter", "중지", "停止", "停止"],
        ["Guide"] = ["開始後は専用入力欄へ自動でフォーカスします。日本語IMEを使用します。Esc・別の入力欄・画面切替で停止します。", "The dedicated input is focused automatically. Japanese IME is used. Esc, another input field or switching windows stops typing.", "Das Spieleingabefeld wird automatisch fokussiert. Japanische IME wird verwendet. Esc, ein anderes Eingabefeld oder Fensterwechsel stoppt die Eingabe.", "Le champ du jeu reçoit automatiquement le focus. L’IME japonais est utilisé. Échap, un autre champ ou un changement de fenêtre arrête la saisie.", "전용 입력란에 자동으로 포커스를 맞춥니다. 일본어 IME를 사용합니다. Esc, 다른 입력란 또는 창 전환 시 중지합니다.", "自动聚焦专用输入框，使用日语IME。Esc、其他输入框或切换窗口会停止输入。", "自動聚焦專用輸入框，使用日語IME。Esc、其他輸入框或切換視窗會停止輸入。"],
        ["Preview"] = ["実プレイログに基づく修正版です。修正後のIME・キー認識は実ゲームでの確認が必要です。", "Updated from gameplay logs. The revised IME and keyboard path still needs live-game verification.", "Anhand von Spielprotokollen korrigiert. IME und Tastenerkennung müssen noch im Spiel geprüft werden.", "Corrigé à partir de journaux de partie. L’IME et les touches corrigés restent à vérifier en jeu.", "실제 플레이 로그를 바탕으로 수정했습니다. 수정된 IME 및 키 인식은 게임에서 확인이 필요합니다.", "已根据实际游玩日志修正。更新后的IME和按键识别仍需实机验证。", "已根據實際遊玩日誌修正。更新後的IME和按鍵識別仍需實機驗證。"],
        ["Status"] = ["状態", "Status", "Status", "État", "상태", "状态", "狀態"],
        ["Stopped"] = ["停止中", "Stopped", "Gestoppt", "Arrêté", "중지됨", "已停止", "已停止"],
        ["Arming"] = ["開始待ち（3秒）", "Arming (3 s)", "Start in 3 s", "Démarrage dans 3 s", "시작 대기 (3초)", "等待开始（3秒）", "等待開始（3秒）"],
        ["Waiting"] = ["出題の更新待ち", "Waiting for a new word", "Warte auf neues Wort", "En attente d’un mot", "새 단어 대기", "等待新题目", "等待新題目"],
        ["Typing"] = ["入力中", "Typing", "Eingabe läuft", "Saisie en cours", "입력 중", "正在输入", "正在輸入"],
        ["FocusLost"] = ["対象画面・フォーカスを確認できないため停止", "Stopped: game screen or input is not focused", "Gestoppt: Spielbildschirm oder Eingabefokus fehlt", "Arrêt : écran du jeu ou focus absent", "대상 화면 또는 포커스를 확인할 수 없어 중지", "无法确认目标界面或焦点，已停止", "無法確認目標介面或焦點，已停止"],
        ["NoProgress"] = ["出題が更新されないため停止", "Stopped: word did not change", "Gestoppt: Wort unverändert", "Arrêt : le mot n’a pas changé", "단어가 갱신되지 않아 중지", "题目未更新，已停止", "題目未更新，已停止"],
        ["Unsupported"] = ["未対応の文字のため停止", "Stopped: unsupported characters", "Gestoppt: nicht unterstützte Zeichen", "Arrêt : caractères non pris en charge", "지원하지 않는 문자로 중지", "存在不支持的字符，已停止", "存在不支援的字元，已停止"],
        ["SendFailed"] = ["キーを送信できないため停止（自動再送なし）", "Stopped: key delivery failed (no automatic retry)", "Gestoppt: Tastenübertragung fehlgeschlagen (keine Wiederholung)", "Arrêt : échec d’envoi de touche (sans nouvel essai)", "키 전송 실패로 중지 (자동 재시도 없음)", "按键发送失败，已停止（不自动重试）", "按鍵傳送失敗，已停止（不自動重試）"],
        ["Fault"] = ["読み取りエラーのため停止", "Stopped: read error", "Gestoppt: Lesefehler", "Arrêt : erreur de lecture", "읽기 오류로 중지", "读取错误，已停止", "讀取錯誤，已停止"],
        ["Words"] = ["検出した出題数", "Detected word count", "Erkannte Wörter", "Nombre de mots détectés", "감지된 단어 수", "检测到的题目数", "偵測到的題目數"],
        ["Current"] = ["入力対象", "Current word", "Aktuelles Wort", "Mot actuel", "입력 대상", "当前题目", "目前題目"],
        ["Sent"] = ["送信文字数", "Characters sent", "Gesendete Zeichen", "Caractères envoyés", "전송한 문자 수", "已发送字符数", "已傳送字元數"],
        ["Refresh"] = ["表示文字を確認", "Inspect displayed words", "Angezeigte Wörter prüfen", "Inspecter les mots affichés", "표시 단어 확인", "检查显示文字", "檢查顯示文字"],
        ["Persistence"] = ["設定保存の問題があります。設定を確認して再試行してください。", "Configuration storage needs attention. Check settings and retry.", "Konfigurationsspeicher prüfen und erneut versuchen.", "Vérifiez le stockage des paramètres et réessayez.", "설정 저장을 확인한 후 다시 시도하세요.", "设置存储存在问题，请检查后重试。", "設定儲存存在問題，請檢查後重試。"],
        ["Recovered"] = ["バックアップから設定を復元しました。", "Settings restored from backup.", "Einstellungen aus Sicherung wiederhergestellt.", "Paramètres restaurés depuis une sauvegarde.", "백업에서 설정을 복원했습니다.", "已从备份恢复设置。", "已從備份恢復設定。"],
        ["Retry"] = ["設定保存を再試行", "Retry saving settings", "Speichern erneut versuchen", "Réessayer l’enregistrement", "설정 저장 재시도", "重试保存设置", "重試儲存設定"],
        ["CommandHelp"] = ["キーバウンド自動入力の設定を開きます。", "Open Keybound Pilot settings.", "Keybound-Pilot-Einstellungen öffnen.", "Ouvrir les paramètres de Keybound Pilot.", "Keybound Pilot 설정 열기.", "打开 Keybound Pilot 设置。", "開啟 Keybound Pilot 設定。"],
        ["InputVisible"] = ["専用入力欄の表示", "Dedicated input visible", "Spieleingabe sichtbar", "Champ du jeu visible", "전용 입력란 표시", "专用输入框可见", "專用輸入框可見"],
        ["InputFocused"] = ["専用入力欄のフォーカス", "Dedicated input focused", "Spieleingabe fokussiert", "Champ du jeu actif", "전용 입력란 포커스", "专用输入框获得焦点", "專用輸入框取得焦點"],
        ["Yes"] = ["はい", "Yes", "Ja", "Oui", "예", "是", "是"],
        ["No"] = ["いいえ", "No", "Nein", "Non", "아니요", "否", "否"],
        ["Diagnostics"] = ["診断ログ", "Diagnostics", "Diagnose", "Diagnostic", "진단 로그", "诊断日志", "診斷日誌"],
        ["DiagHelp"] = ["コンテンツ内では自動入力が停止中でも状態を記録します。手動で1回遊び、退出または記録停止後にログを確認してください。チャット・プレイヤー名・入力文字列・生のキーは記録しません。最大120分・1回64MiB・合計256MiB。上限で停止し、既存ログは削除しません。", "State is recorded in the content even when auto typing is stopped. Play manually once, then leave or stop recording. No chat, player names, typed text or raw keys are recorded. Limits: 120 minutes, 64 MiB per session, 256 MiB total. Recording stops at the limit; existing logs are kept.", "Der Zustand wird im Inhalt auch ohne automatische Eingabe aufgezeichnet. Einmal manuell spielen, dann verlassen oder Aufnahme stoppen. Keine Chats, Spielernamen, Eingabetexte oder rohen Tasten. Grenzen: 120 Minuten, 64 MiB pro Sitzung, insgesamt 256 MiB. Vorhandene Protokolle bleiben erhalten.", "L’état est enregistré dans le contenu même sans saisie automatique. Jouez une fois manuellement, puis quittez ou arrêtez. Aucun chat, nom de joueur, texte saisi ou touche brute. Limites : 120 minutes, 64 Mio par session, 256 Mio au total. Les anciens journaux sont conservés.", "자동 입력 중지 상태에서도 콘텐츠 상태를 기록합니다. 한 번 수동으로 플레이한 후 퇴장하거나 기록을 중지하세요. 채팅, 플레이어 이름, 입력한 텍스트, 원시 키는 기록하지 않습니다. 최대 120분, 세션당 64MiB, 총 256MiB이며 기존 로그는 삭제하지 않습니다.", "即使自动输入已停止，仍会在副本内记录状态。请手动玩一次，然后退出或停止记录。不记录聊天、玩家姓名、输入文字或原始按键。上限为120分钟、每次64MiB、总计256MiB。达到上限后停止，不删除已有日志。", "即使自動輸入已停止，仍會在副本內記錄狀態。請手動玩一次，然後退出或停止記錄。不記錄聊天、玩家姓名、輸入文字或原始按鍵。上限為120分鐘、每次64MiB、總計256MiB。達到上限後停止，不刪除既有日誌。"],
        ["DiagIdle"] = ["記録待機中", "Recording idle", "Aufnahme inaktiv", "En attente", "기록 대기", "等待记录", "等待記錄"],
        ["DiagRecording"] = ["記録中", "Recording", "Aufzeichnung läuft", "Enregistrement", "기록 중", "正在记录", "正在記錄"],
        ["DiagLimit"] = ["記録上限に到達したため停止", "Stopped at recording limit", "Aufnahmegrenze erreicht", "Limite atteinte", "기록 한도에 도달하여 중지", "达到记录上限，已停止", "達到記錄上限，已停止"],
        ["DiagError"] = ["記録に失敗しました。自動再送は行いません。", "Recording failed; no automatic replay.", "Aufnahme fehlgeschlagen; keine automatische Wiederholung.", "Échec d’enregistrement ; aucune réécriture automatique.", "기록 실패. 자동 재전송하지 않습니다.", "记录失败，不自动重写。", "記錄失敗，不自動重寫。"],
        ["DiagStart"] = ["記録開始／再開", "Start / restart recording", "Aufnahme starten / neu starten", "Démarrer / redémarrer", "기록 시작 / 재시작", "开始／重新记录", "開始／重新記錄"],
        ["DiagStop"] = ["記録を停止", "Stop recording", "Aufnahme stoppen", "Arrêter", "기록 중지", "停止记录", "停止記錄"],
        ["DiagCopy"] = ["保存先をコピー", "Copy log directory", "Protokollpfad kopieren", "Copier le dossier", "저장 경로 복사", "复制保存路径", "複製儲存路徑"],
        ["DiagBlock"] = ["入力禁止を見た時の目印", "Mark typing prohibition", "Eingabeverbot markieren", "Marquer une interdiction", "입력 금지 시점 표시", "标记禁止输入时刻", "標記禁止輸入時刻"],
        ["DiagCast"] = ["詠唱を見た時の目印", "Mark enemy cast", "Gegnerzauber markieren", "Marquer une incantation", "적 시전 시점 표시", "标记敌人咏唱", "標記敵人詠唱"],
        ["DiagRecords"] = ["記録件数／書込バイト", "Records / written bytes", "Einträge / geschriebene Bytes", "Entrées / octets écrits", "기록 수 / 기록 바이트", "记录数／写入字节", "記錄數／寫入位元組"],
        ["Paused"] = ["一時待機中（入力禁止・入力欄の準備）", "Waiting (input prohibition or input preparation)", "Wartezeit (Eingabesperre oder Vorbereitung)", "En attente (interdiction ou préparation)", "일시 대기 (입력 금지 또는 준비)", "暂时等待（禁止输入或输入准备）", "暫時等待（禁止輸入或輸入準備）"],
        ["Confirming"] = ["ひらがなを確定中", "Confirming hiragana", "Hiragana bestätigen", "Confirmation des hiragana", "히라가나 확정 중", "正在确认平假名", "正在確認平假名"],
        ["ImeFailed"] = ["IME・確定した読みを確認できないため停止。日本語のひらがな入力を確認してください。", "Stopped: IME or committed reading could not be verified. Check Japanese hiragana input.", "Gestoppt: IME oder bestätigte Lesung nicht verifiziert. Japanische Hiragana-Eingabe prüfen.", "Arrêt : IME ou lecture validée non vérifiée. Vérifiez la saisie japonaise en hiragana.", "IME 또는 확정된 읽기를 확인할 수 없어 중지했습니다. 일본어 히라가나 입력을 확인하세요.", "无法确认IME或已确定的读音，已停止。请检查日语平假名输入。", "無法確認IME或已確定的讀音，已停止。請檢查日語平假名輸入。"],
        ["TargetChanged"] = ["入力途中で対象が変わったため停止。残った文字を消して再開してください。", "Stopped: target changed during typing. Clear remaining input before restarting.", "Gestoppt: Ziel während der Eingabe geändert. Resttext vor Neustart löschen.", "Arrêt : cible modifiée pendant la saisie. Effacez le texte restant avant de reprendre.", "입력 중 대상이 변경되어 중지했습니다. 남은 입력을 지우고 다시 시작하세요.", "输入途中目标变化，已停止。请清空剩余文字后重新开始。", "輸入途中目標變化，已停止。請清空剩餘文字後重新開始。"],
    };
    public static string FontText => string.Concat(Text.Values.SelectMany(x => x)) + string.Concat(Codes) + "日本語EnglishDeutschFrançais한국어简体中文繁體中文";
    public static bool IsSupported(string? value) => Array.IndexOf(Codes, value) >= 0;
    public static string Get(string? code, string key) => Text.TryGetValue(key, out var values) ? values[IsSupported(code) ? Array.IndexOf(Codes, code) : 1] : key;
    public static void CheckCompleteness() { if (Text.Values.Any(v => v.Length != 7 || v.Any(string.IsNullOrWhiteSpace))) throw new InvalidDataException("Incomplete localization"); }
    public static string? FromGame(ClientLanguage value) => value switch
    { ClientLanguage.Japanese => "ja", ClientLanguage.English => "en", ClientLanguage.German => "de", ClientLanguage.French => "fr", _ => null };
    public static string? Normalize(string? value)
    {
        var v = value?.Trim().Replace('_', '-').ToLowerInvariant();
        return v switch { "ja" or "ja-jp" => "ja", "en" or "en-us" or "en-gb" => "en", "de" or "de-de" => "de", "fr" or "fr-fr" => "fr", "ko" or "ko-kr" => "ko", "zh-hans" or "zh-cn" or "zh-sg" => "zh-Hans", "zh-hant" or "zh-tw" or "zh-hk" or "zh-mo" => "zh-Hant", _ => null };
    }
}