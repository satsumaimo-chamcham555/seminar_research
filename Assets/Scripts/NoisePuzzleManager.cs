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
    public GameObject letterRo;     // 『ろ』の文字（敵側に未設定の場合のフォールバック）

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
    // 見えない敵との連携（明るさ0での挙動は InvisibleEnemy 側が管理）
    // ------------------------------------------------------------

    /// <summary>ポーズ画面の明るさスライダーが0か（InvisibleEnemy が毎フレーム参照する）</summary>
    public bool IsBrightnessZero => brightnessSlider != null && brightnessSlider.value <= SliderZeroThreshold;

    /// <summary>敵側に『ろ』が設定されていない場合のフォールバック表示</summary>
    public void ShowLetterRo()
    {
        if (letterRo != null) letterRo.SetActive(true);
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
