using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NoisePuzzleManager : MonoBehaviour
{
    [Header("UI & オーディオ参照")]
    public GameObject pausePanel;
    public GameObject END1Panel;
    public Button retryButton;
    public Slider volumeSlider;
    public Slider brightnessSlider; // 明るさスライダー（ポーズ画面内）
    public AudioSource noiseAudio;
    public GameObject letterKi;
    public GameObject letterRo;     // 『ろ』の文字（アイテム）

    [Header("見えない敵・足音演出参照")]
    public InvisibleEnemy invisibleEnemy;
    public AudioSource enemyChaseAudio; // 追われてる時の足音
    public AudioSource enemyAwayAudio;  // 遠ざかっていく足音（SE）
    public bool isBeingChased = false;  // 敵に追われている最中かのフラグ

    [Header("TV参照")]
    public Transform tvParent; // 親オブジェクト「TVs」
    private TVController[] tvControllers;

    [Header("プレイヤー参照（操作停止用）")]
    public MouseLook mouseLook;
    public PlayerMovement playerMovement;

    [Header("ノイズの上昇設定")]
    public float maxVolume = 1.0f;
    public float fadeDuration = 45.0f;

    private const float SliderZeroThreshold = 0.001f;

    private bool isSolved = false;
    private bool isIncreasing = false;
    private bool hasNoticedVolumeDown = false; // 音量を下げたセリフの既読フラグ

    // 敵イベント管理
    private bool isEnemyEventSolved = false;
    private Coroutine enemySequenceCoroutine;
    private Coroutine noiseCoroutine;

    private bool IsPaused => pausePanel != null && pausePanel.activeSelf;

    // ------------------------------------------------------------
    // Unity ライフサイクル
    // ------------------------------------------------------------

    void Start()
    {
        if (letterKi != null) letterKi.SetActive(false);
        if (letterRo != null) letterRo.SetActive(false);
        if (END1Panel != null) END1Panel.SetActive(false);

        if (tvParent != null)
        {
            tvControllers = CollectTvControllers();
        }

        if (noiseAudio != null)
        {
            noiseAudio.volume = 0f;
            noiseAudio.pitch = 1.0f;
        }

        SetupUiListeners();

        if (mouseLook == null) mouseLook = FindObjectOfType<MouseLook>();
        if (playerMovement == null) playerMovement = FindObjectOfType<PlayerMovement>();
        if (invisibleEnemy == null) invisibleEnemy = FindObjectOfType<InvisibleEnemy>();
    }

    void Update()
    {
        if (END1Panel != null && END1Panel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            TogglePause();
        }

        if (isSolved) return;

        // ポーズ画面で音量を0にしたらクリア
        if (isIncreasing && IsPaused && volumeSlider != null && volumeSlider.value <= SliderZeroThreshold)
        {
            SolvePuzzle();
        }
    }

    void SetupUiListeners()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(RestartScene);
        }

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            AudioListener.volume = volumeSlider.value;
        }

        if (brightnessSlider != null)
        {
            brightnessSlider.onValueChanged.RemoveAllListeners();
            brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
        }
    }

    // ------------------------------------------------------------
    // ポーズ・プレイヤー操作
    // ------------------------------------------------------------

    void TogglePause()
    {
        if (pausePanel == null) return;
        SetPaused(!pausePanel.activeSelf);
    }

    void SetPaused(bool paused)
    {
        pausePanel.SetActive(paused);
        SetPlayerInputBlocked(paused);
        CursorUtil.SetLocked(!paused);
    }

    void SetPlayerInputBlocked(bool blocked)
    {
        if (mouseLook != null) mouseLook.isChatActive = blocked;
        if (playerMovement != null) playerMovement.canMove = !blocked;
    }

    // ------------------------------------------------------------
    // TVノイズの謎
    // ------------------------------------------------------------

    TVController[] CollectTvControllers()
    {
        return tvParent != null
            ? tvParent.GetComponentsInChildren<TVController>()
            : FindObjectsOfType<TVController>();
    }

    void ForEachTv(System.Action<TVController> action)
    {
        if (tvControllers == null) return;

        foreach (TVController tv in tvControllers)
        {
            if (tv != null) action(tv);
        }
    }

    public void StartNoiseSequence()
    {
        if (isSolved || isIncreasing) return;

        tvControllers = CollectTvControllers();
        ForEachTv(tv => tv.TurnOnTV());

        if (noiseAudio != null)
        {
            if (!noiseAudio.isPlaying) noiseAudio.Play();
            noiseCoroutine = StartCoroutine(FadeInNoiseSequence());
        }
    }

    IEnumerator FadeInNoiseSequence()
    {
        isIncreasing = true;
        float timer = 0f;

        bool flag5 = false;
        bool flag15 = false;
        bool flag30 = false;

        while (timer < fadeDuration && !isSolved)
        {
            timer += Time.deltaTime;

            if (noiseAudio != null)
            {
                float progress = Mathf.Clamp01(timer / fadeDuration);
                noiseAudio.volume = progress * maxVolume;

                if (timer > 30.0f)
                {
                    noiseAudio.pitch = 1.0f + ((timer - 30.0f) / 15.0f) * 0.3f;
                }
            }

            if (timer >= 5.0f && !flag5)
            {
                flag5 = true;
                ShowSubtitle("「……なんだこの音。」", 4.0f);
            }
            else if (timer >= 15.0f && !flag15)
            {
                flag15 = true;
                ShowSubtitle("「うるさい……」", 4.0f);
            }
            else if (timer >= 30.0f && !flag30)
            {
                flag30 = true;
                ShowSubtitle("「うるさいうるさい……もうやめてくれ……！！！」", 5.0f);
            }

            yield return null;
        }

        if (!isSolved)
        {
            if (noiseAudio != null) noiseAudio.volume = maxVolume;

            ShowSubtitle("「もう、ダメだ……」", 3.0f);
            yield return new WaitForSeconds(2.0f);

            GameOver();
        }
    }

    public void SolvePuzzle()
    {
        isSolved = true;

        if (noiseCoroutine != null)
        {
            StopCoroutine(noiseCoroutine);
        }

        ForEachTv(tv => tv.TurnOffTVByManager());

        if (noiseAudio != null)
        {
            noiseAudio.Stop();
            noiseAudio.pitch = 1.0f;
        }
        if (letterKi != null) letterKi.SetActive(true);

        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.MarkAsCleared("Puzzle_TV");
        }

        ShowSubtitle("「静かになった…………。」", 5.0f);

        if (IsPaused)
        {
            SetPaused(false);
        }

        Debug.Log("【謎解き完了】ノイズ停止＆『き』が出現！");
    }

    public void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;

        if (isIncreasing && !isSolved && !hasNoticedVolumeDown)
        {
            if (value < 0.95f && value > SliderZeroThreshold)
            {
                hasNoticedVolumeDown = true;
                ShowSubtitle("「……あれ？ 今、音が……」", 4.0f);
            }
        }
    }

    // ------------------------------------------------------------
    // 見えない敵：明るさ0で逃げ切る謎
    // ------------------------------------------------------------

    /// <summary>ポーズ画面で明るさスライダーを動かした時の処理</summary>
    public void OnBrightnessChanged(float value)
    {
        if (value > SliderZeroThreshold && enemySequenceCoroutine != null)
        {
            CancelEnemySequence();
        }
        else if (isBeingChased && !isEnemyEventSolved && value <= SliderZeroThreshold && enemySequenceCoroutine == null)
        {
            enemySequenceCoroutine = StartCoroutine(SolveEnemySequence());
        }
    }

    IEnumerator SolveEnemySequence()
    {
        // 1. 追跡中の足音を止め、敵を完全停止＆非表示（物理接触の事故防止）
        if (enemyChaseAudio != null) enemyChaseAudio.Stop();
        if (invisibleEnemy != null)
        {
            invisibleEnemy.isChasing = false;
            if (invisibleEnemy.footstepAudio != null) invisibleEnemy.footstepAudio.Stop();
            invisibleEnemy.gameObject.SetActive(false);
        }

        // 2. 「足音が止まった？」→ 明るさ0のまま2.5秒待機
        ShowSubtitle("「……足音が止まった？」", 3.0f);

        bool completed = false;
        yield return WaitWhileBrightnessZero(2.5f, result => completed = result);
        if (!completed)
        {
            RestoreEnemyState();
            yield break;
        }

        // 3. 遠ざかる足音 → 明るさ0のまま3.0秒待機
        if (enemyAwayAudio != null) enemyAwayAudio.Play();

        yield return WaitWhileBrightnessZero(3.0f, result => completed = result);
        if (!completed)
        {
            RestoreEnemyState();
            yield break;
        }

        // 4. 完全撃退
        ShowSubtitle("「……いなくなった……？」", 3.5f);

        isEnemyEventSolved = true;
        isBeingChased = false;

        // DefeatEnemy を通すため一瞬アクティブにしてから完全に消去
        if (invisibleEnemy != null)
        {
            invisibleEnemy.gameObject.SetActive(true);
            invisibleEnemy.DefeatEnemy();
            invisibleEnemy.gameObject.SetActive(false);
        }

        if (letterRo != null) letterRo.SetActive(true);

        enemySequenceCoroutine = null;
        Debug.Log("【見えない敵イベントクリア】完全撃退し、『ろ』が出現！");
    }

    /// <summary>
    /// 明るさが0のまま duration 秒経過したら true、
    /// 途中で明るさが戻されたら false を onFinished に渡す。
    /// </summary>
    IEnumerator WaitWhileBrightnessZero(float duration, System.Action<bool> onFinished)
    {
        float timer = 0f;
        while (timer < duration)
        {
            if (brightnessSlider != null && brightnessSlider.value > SliderZeroThreshold)
            {
                onFinished(false);
                yield break;
            }
            timer += Time.deltaTime;
            yield return null;
        }
        onFinished(true);
    }

    /// <summary>途中で明るさを戻した時に敵を再出現させる</summary>
    void RestoreEnemyState()
    {
        if (invisibleEnemy != null)
        {
            invisibleEnemy.gameObject.SetActive(true);
            invisibleEnemy.isChasing = true;
        }
    }

    /// <summary>撃退演出の途中で明るさを戻された時の追跡再開処理</summary>
    void CancelEnemySequence()
    {
        if (enemySequenceCoroutine != null)
        {
            StopCoroutine(enemySequenceCoroutine);
            enemySequenceCoroutine = null;
        }

        if (enemyAwayAudio != null && enemyAwayAudio.isPlaying)
        {
            enemyAwayAudio.Stop();
        }

        isBeingChased = true;
        if (invisibleEnemy != null)
        {
            invisibleEnemy.isChasing = true;
        }

        ShowSubtitle("「……ダメだ、また来る！！」", 3.0f);
        Debug.Log("【演出キャンセル】明るさが戻されたため、敵が追跡を再開します！");
    }

    // ------------------------------------------------------------
    // ゲームオーバー・リスタート
    // ------------------------------------------------------------

    void GameOver()
    {
        Debug.Log("【GAME OVER】ノイズの限界に達しました。");

        if (noiseAudio != null)
        {
            noiseAudio.Stop();
            noiseAudio.pitch = 1.0f;
        }

        if (END1Panel != null)
        {
            END1Panel.SetActive(true);
            CursorUtil.SetLocked(false);
        }

        SetPlayerInputBlocked(true);
    }

    public void RestartScene()
    {
        LetterItem.totalCollected = 0;

        CursorUtil.SetLocked(true);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void ShowSubtitle(string text, float duration)
    {
        SubtitleManager.Show(text, duration);
    }
}
