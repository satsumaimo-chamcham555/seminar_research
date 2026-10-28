using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// ステージ2エンディングの司令塔。シーンに1つ（GameManager など）に付ける。
///
/// 流れ：
///   「でろ」入力①  → 会話1（世界は変わらない）→ 自動でチャットを開き直す
///   「でろ」入力②  → 会話2（唱えるだけではダメだと気づく）→ 脱出フェーズ開始
///   脱出フェーズ   → 前進し続けるほど徐々に加速 → 十分に加速したら、次の部屋が「脱出エリア」に差し替わる
///   脱出エリア奥   → ホワイトアウト → 短いテキスト → END
/// </summary>
public class Stage2EndingManager : MonoBehaviour
{
    private enum State { Idle, Dialogue1, WaitSecondInput, Dialogue2, Escaping, Exiting, Finished }

    [Header("参照（空なら自動取得）")]
    public GameClearManager chatSystem;
    public PlayerMovement playerMovement;
    public MouseLook mouseLook;

    [Header("入力ゲート")]
    [Tooltip("ONの間は NotifyLettersReady() が呼ばれるまで「でろ」を受け付けない。\n「で」「ろ」取得後に既存の仕組みでチャットが開くなら OFF のままでOK")]
    public bool requireLettersReady = false;

    [Header("会話の間隔")]
    [Tooltip("会話1が終わってから、2回目の入力用にチャットを開き直すまでの秒数")]
    public float reopenDelay = 1.0f;

    [Header("脱出：加速（前進し続けるほど速くなる）")]
    [Tooltip("加速しきった時の最高速度（通常は PlayerMovement.speed。6 なら 14〜18 くらい）")]
    public float maxSpeed = 16.0f;
    [Tooltip("前進し続けて最高速度に達するまでの秒数")]
    public float rampSeconds = 20.0f;
    [Tooltip("前進をやめている間、加速度がしぼむ速さ（1で同じ速さ、大きいほど早く落ちる）")]
    public float decayRate = 1.5f;
    [Tooltip("最高速時の足音間隔の倍率（0.45 なら足音のテンポが約2倍になる）")]
    [Range(0.2f, 1.0f)] public float minStepIntervalRatio = 0.45f;

    [Header("脱出：ループ突破")]
    [Tooltip("加速度（0〜1）がこの値を超えたら、次に出す部屋を脱出エリアに差し替える")]
    [Range(0.3f, 1.0f)] public float breakthroughThreshold = 0.9f;
    [Tooltip("脱出エリアのプレハブ（奥に EscapeExitTrigger を置く）")]
    public GameObject exitAreaPrefab;
    [Tooltip("前進せず立ち止まったままこの秒数たつと、ヒントのセリフを1度だけ出す（0で無効）")]
    public float idleHintSeconds = 15.0f;

    [Header("演出")]
    [Tooltip("空なら GameClearManager.whiteoutImage を使う")]
    public Image whiteoutImage;
    public float whiteoutDuration = 3.0f;
    [Tooltip("加速に合わせて音量を上げる風音など（任意・ループ設定のもの）")]
    public AudioSource windAudio;
    public float windMaxVolume = 0.8f;
    [Tooltip("最後に出すパネル。空なら GameClearManager.END3Panel を使う")]
    public GameObject endPanel;
    public UnityEvent onEscaped;

    // 会話（表示秒数, 次の行までの待ち秒数）
    private static readonly (string text, float duration, float wait)[] Lines1 =
    {
        ("「……でろ。」",              2.5f, 3.5f),
        ("「……ここから出る？」",      3.0f, 4.0f),
        ("「……どうやって？」",        3.5f, 4.5f),
    };

    private static readonly (string text, float duration, float wait)[] Lines2 =
    {
        ("「でろ……。」",                  2.5f, 3.5f),
        ("「…………。」",                  2.0f, 3.0f),
        ("「ただ、唱えるだけじゃだめだ。」", 3.5f, 4.5f),
        ("「何か……行動しないと。」",        3.5f, 4.5f),
        ("「……出るんだ。」",               2.5f, 3.5f),
        ("「ここから。」",                  2.5f, 3.5f),
        ("「…………。」",                  2.0f, 3.0f),
    };

    private static readonly HashSet<string> EndingWords = new HashSet<string>
    {
        "でろ", "デロ", "ﾃﾞﾛ", "出ろ", "dero",
    };

    private State state = State.Idle;
    private bool lettersReady = false;

    // 脱出フェーズ用
    private float baseSpeed;
    private float baseStepInterval;
    private float forwardTime = 0f;
    private float accel01 = 0f;
    private float idleTimer = 0f;
    private bool exitArmed = false;
    private bool hasShownHalfLine = false;
    private bool hasShownIdleHint = false;

    /// <summary>true の間、GameClearManager は Enter でチャットを開かない</summary>
    public bool IsChatLocked => state != State.Idle && state != State.WaitSecondInput;

    // ------------------------------------------------------------
    // ライフサイクル
    // ------------------------------------------------------------

    void Awake()
    {
        FactoryRoom.ResetLoopOverride(); // ループ状態の初期化
    }

    void Start()
    {
        if (chatSystem == null) chatSystem = FindObjectOfType<GameClearManager>();
        if (playerMovement == null) playerMovement = FindObjectOfType<PlayerMovement>();
        if (mouseLook == null) mouseLook = FindObjectOfType<MouseLook>();

        if (whiteoutImage == null && chatSystem != null) whiteoutImage = chatSystem.whiteoutImage;
        if (endPanel == null && chatSystem != null) endPanel = chatSystem.END3Panel;

        if (playerMovement != null)
        {
            baseSpeed = playerMovement.speed;
            baseStepInterval = playerMovement.stepInterval;
        }

        if (exitAreaPrefab == null)
        {
            Debug.LogError("【Stage2Ending】exitAreaPrefab が未設定です。脱出エリアのプレハブを指定してください。");
        }
    }

    void OnDisable()
    {
        RestoreMovement();
    }

    void OnDestroy()
    {
        FactoryRoom.ResetLoopOverride();
    }

    void Update()
    {
        if (state != State.Escaping && state != State.Exiting) return;

        UpdateAcceleration();

        if (state == State.Escaping)
        {
            UpdateLoopBreakthrough();
            UpdateIdleHint();
        }
    }

    // ------------------------------------------------------------
    // 入力（GameClearManager から呼ばれる）
    // ------------------------------------------------------------

    /// <summary>「で」「ろ」を取り終えたことを通知する（requireLettersReady を使う場合のみ）</summary>
    public void NotifyLettersReady()
    {
        lettersReady = true;
    }

    /// <summary>
    /// 入力文字を受け取る。エンディングの言葉なら true（＝こちらで処理した）、それ以外は false。
    /// </summary>
    public bool TryHandleWord(string input)
    {
        Debug.Log($"【Ending】入力を受け取りました: [{input}]");

        if (string.IsNullOrEmpty(input)) return false;
        if (!EndingWords.Contains(input.Trim().ToLowerInvariant())) return false;
        if (requireLettersReady && !lettersReady) return false;

        switch (state)
        {
            case State.Idle:
                state = State.Dialogue1;
                StartCoroutine(FirstInputSequence());
                break;

            case State.WaitSecondInput:
                state = State.Dialogue2;
                StartCoroutine(SecondInputSequence());
                break;

            // 会話中・脱出中の入力は無視（会話が重複・無限ループしないように）
            default:
                break;
        }
        return true;
    }

    // ------------------------------------------------------------
    // 会話
    // ------------------------------------------------------------

    IEnumerator PlayLines((string text, float duration, float wait)[] lines)
    {
        foreach (var line in lines)
        {
            SubtitleManager.Show(line.text, line.duration);
            yield return new WaitForSeconds(line.wait);
        }
    }

    IEnumerator FirstInputSequence()
    {
        yield return PlayLines(Lines1);

        state = State.WaitSecondInput;
        yield return new WaitForSeconds(reopenDelay);

        // その間に自分で再入力していなければ、チャットを開き直す
        if (state == State.WaitSecondInput && chatSystem != null)
        {
            chatSystem.OpenChat();
        }
    }

    IEnumerator SecondInputSequence()
    {
        yield return PlayLines(Lines2);

        BeginEscape();
    }

    // ------------------------------------------------------------
    // 脱出フェーズ
    // ------------------------------------------------------------

    void BeginEscape()
    {
        state = State.Escaping;

        forwardTime = 0f;
        accel01 = 0f;
        idleTimer = 0f;

        if (windAudio != null)
        {
            windAudio.volume = 0f;
            windAudio.loop = true;
            windAudio.Play();
        }

        Debug.Log("【Stage2Ending】脱出フェーズ開始：前進で加速します。");
    }

    /// <summary>前進入力が続くほど速くなり、止まるとゆっくり落ちる</summary>
    void UpdateAcceleration()
    {
        if (playerMovement == null) return;

        bool movingForward = playerMovement.canMove && Input.GetAxisRaw("Vertical") > 0.1f;

        if (movingForward)
        {
            forwardTime += Time.deltaTime;
            idleTimer = 0f;
        }
        else
        {
            forwardTime -= Time.deltaTime * decayRate;
            idleTimer += Time.deltaTime;
        }
        forwardTime = Mathf.Clamp(forwardTime, 0f, rampSeconds);

        accel01 = Mathf.SmoothStep(0f, 1f, forwardTime / rampSeconds);

        playerMovement.speed = Mathf.Lerp(baseSpeed, maxSpeed, accel01);
        playerMovement.stepInterval = Mathf.Lerp(baseStepInterval, baseStepInterval * minStepIntervalRatio, accel01);

        if (windAudio != null) windAudio.volume = accel01 * windMaxVolume;
    }

    /// <summary>十分に加速したら、次に生成される部屋を脱出エリアに差し替える</summary>
    void UpdateLoopBreakthrough()
    {
        if (!hasShownHalfLine && accel01 >= 0.5f)
        {
            hasShownHalfLine = true;
            SubtitleManager.Show("「……足が、軽い……？」", 3.5f);
        }

        if (!exitArmed && accel01 >= breakthroughThreshold && exitAreaPrefab != null)
        {
            exitArmed = true;
            FactoryRoom.nextRoomOverride = exitAreaPrefab; // 次の SpawnNext で脱出エリアが出る
            SubtitleManager.Show("「……行ける……！」", 3.0f);
            Debug.Log("【Stage2Ending】ループ突破の準備完了：次の部屋が脱出エリアになります。");
        }
    }

    void UpdateIdleHint()
    {
        if (hasShownIdleHint || idleHintSeconds <= 0f) return;

        if (idleTimer >= idleHintSeconds)
        {
            hasShownIdleHint = true;
            SubtitleManager.Show("「……進まなきゃ。」", 3.5f);
        }
    }

    // ------------------------------------------------------------
    // 脱出（EscapeExitTrigger から呼ばれる）
    // ------------------------------------------------------------

    public void OnReachedExit()
    {
        if (state != State.Escaping) return;

        state = State.Exiting;
        StartCoroutine(ExitSequence());
    }

    IEnumerator ExitSequence()
    {
        // 走り続けたまま、徐々に白くなる
        float t = 0f;
        while (t < whiteoutDuration)
        {
            t += Time.deltaTime;
            SetWhiteoutAlpha(Mathf.Clamp01(t / whiteoutDuration));
            yield return null;
        }
        SetWhiteoutAlpha(1f);

        state = State.Finished;
        LockPlayer();
        RestoreMovement();
        if (windAudio != null) windAudio.Stop();

        yield return new WaitForSeconds(1.0f);

        SubtitleManager.Show("「…………。」", 3.0f);
        yield return new WaitForSeconds(3.5f);

        SubtitleManager.Show("「……出られた。」", 4.0f);
        yield return new WaitForSeconds(5.0f);

        if (endPanel != null) endPanel.SetActive(true);
        CursorUtil.SetLocked(false);

        if (onEscaped != null) onEscaped.Invoke();

        Debug.Log("【Stage2Ending】ループを突破して脱出しました。");
    }

    // ------------------------------------------------------------
    // 共通処理
    // ------------------------------------------------------------

    void LockPlayer()
    {
        if (playerMovement != null) playerMovement.canMove = false;
        if (mouseLook != null) mouseLook.isChatActive = true;
    }

    /// <summary>加速で変えた速度・足音間隔を元に戻す</summary>
    void RestoreMovement()
    {
        if (playerMovement == null) return;

        if (baseSpeed > 0f) playerMovement.speed = baseSpeed;
        if (baseStepInterval > 0f) playerMovement.stepInterval = baseStepInterval;
    }

    void SetWhiteoutAlpha(float alpha)
    {
        if (whiteoutImage == null) return;

        Color c = whiteoutImage.color;
        c.a = alpha;
        whiteoutImage.color = c;
    }
}
