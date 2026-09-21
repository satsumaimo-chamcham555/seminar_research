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
    public Slider brightnessSlider; // ★明るさスライダー（ポーズ画面内）
    public AudioSource noiseAudio;
    public GameObject letterKi;
    public GameObject letterRo;     // ★『ろ』の文字（アイテム）

    [Header("見えない敵・足音演出参照")]
    public InvisibleEnemy invisibleEnemy; // ★見えない敵の参照
    public AudioSource enemyChaseAudio; // ★追われてる時の足音
    public AudioSource enemyAwayAudio;  // ★遠ざかっていく足音（SE）
    public bool isBeingChased = false;   // ★敵に追われている最中かのフラグ

    [Header("TV参照")]
    public Transform tvParent; // ★ここに親オブジェクト「TVs」を1個だけセット！
    private TVController[] tvControllers; // 自動取得用の配列

    [Header("プレイヤー参照（操作停止用）")]
    public MouseLook mouseLook;
    public PlayerMovement playerMovement; 

    [Header("ノイズの上昇設定")]
    public float maxVolume = 1.0f;       
    public float fadeDuration = 90.0f;   

    private bool isSolved = false;
    private bool isIncreasing = false;
    private bool hasNoticedVolumeDown = false; // 音量を少し下げたセリフの既読フラグ

    // ★敵イベント管理用変数（重複を削除してすっきり整理）
    private bool isEnemyEventSolved = false;   // 敵イベント完全撃退フラグ
    private Coroutine enemySequenceCoroutine;  // 撃退コルーチン管理用
    private Coroutine noiseCoroutine;          // ノイズコルーチン管理用

    void Start()
    {
        if (letterKi != null) letterKi.SetActive(false);
        if (letterRo != null) letterRo.SetActive(false);
        if (END1Panel != null) END1Panel.SetActive(false);
        
        if (tvParent != null)
        {
            tvControllers = tvParent.GetComponentsInChildren<TVController>();
        }
        if (noiseAudio != null)
        {
            noiseAudio.volume = 0f;
            noiseAudio.pitch = 1.0f;
        }

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

        if (mouseLook == null) mouseLook = FindObjectOfType<MouseLook>();
        if (playerMovement == null) playerMovement = FindObjectOfType<PlayerMovement>();
        if (invisibleEnemy == null) invisibleEnemy = FindObjectOfType<InvisibleEnemy>();
    }

    void Update()
    {
        if (END1Panel != null && END1Panel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (pausePanel != null)
            {
                bool isActive = !pausePanel.activeSelf;
                pausePanel.SetActive(isActive);

                if (isActive)
                {
                    if (mouseLook != null) mouseLook.isChatActive = true;
                    if (playerMovement != null) playerMovement.canMove = false;

                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    if (mouseLook != null) mouseLook.isChatActive = false;
                    if (playerMovement != null) playerMovement.canMove = true;

                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }

        if (isSolved) return;

        if (isIncreasing && pausePanel != null && pausePanel.activeSelf && volumeSlider != null)
        {
            if (volumeSlider.value <= 0.001f)
            {
                SolvePuzzle();
            }
        }
    }

    public void StartNoiseSequence()
    {
        if (isSolved || isIncreasing) return;

        if (tvControllers != null && tvControllers.Length > 0)
        {
            foreach (TVController tv in tvControllers)
            {
                if (tv != null)
                {
                    tv.TurnOnTV();
                }
            }
        }

        if (noiseAudio != null)
        {
            if (!noiseAudio.isPlaying) noiseAudio.Play();
            noiseCoroutine = StartCoroutine(FadeInNoiseSequence());
        }
    }

    // ★ポーズ画面で「明るさスライダー」を動かした時の処理
    public void OnBrightnessChanged(float value)
    {
        // 撃退演出の途中で明るさを 0 より大きく戻してしまった時（追跡再開！）
        if (value > 0.001f && enemySequenceCoroutine != null)
        {
            CancelEnemySequence();
        }
        // 敵に追われている最中 ＆ 未撃退 ＆ 明るさが 0 になった時
        else if (isBeingChased && !isEnemyEventSolved && value <= 0.001f && enemySequenceCoroutine == null)
        {
            enemySequenceCoroutine = StartCoroutine(SolveEnemySequence());
        }
    }

    // ★明るさを0にして敵から逃げ切る一連の演出コルーチン
    IEnumerator SolveEnemySequence()
    {
        // ※この時点ではまだ完全撃退（isEnemyEventSolved）にはしない

        // 1. 追跡中の足音をストップ＆敵本体の移動を停止
        if (enemyChaseAudio != null) enemyChaseAudio.Stop();
        if (invisibleEnemy != null)
        {
            invisibleEnemy.isChasing = false;
            if (invisibleEnemy.footstepAudio != null) invisibleEnemy.footstepAudio.Stop();
        }

        // 2. セリフ「……足音が止まった？」
        ShowSubtitle("「……足音が止まった？」", 3.0f);

        // ★明るさが0のままか監視しながら 2.5秒 待機
        float timer = 0f;
        while (timer < 2.5f)
        {
            if (brightnessSlider != null && brightnessSlider.value > 0.001f) yield break;
            timer += Time.deltaTime;
            yield return null;
        }

        // 3. 遠ざかっていく足音を再生
        if (enemyAwayAudio != null)
        {
            enemyAwayAudio.Play();
        }

        // ★明るさが0のままか監視しながら 3.0秒 待機
        timer = 0f;
        while (timer < 3.0f)
        {
            if (brightnessSlider != null && brightnessSlider.value > 0.001f) yield break;
            timer += Time.deltaTime;
            yield return null;
        }

        // 4. セリフ「……いなくなった……？」
        ShowSubtitle("「……いなくなった……？」", 3.5f);

        // 5. 完全撃退完了！
        isEnemyEventSolved = true;
        isBeingChased = false;
        if (invisibleEnemy != null) invisibleEnemy.DefeatEnemy();

        if (letterRo != null)
        {
            letterRo.SetActive(true);
        }

        enemySequenceCoroutine = null;
        Debug.Log("【見えない敵イベントクリア】完全撃退し、『ろ』が出現！");
    }

    // ★途中で明るさを戻された時の追跡再開処理
    void CancelEnemySequence()
    {
        if (enemySequenceCoroutine != null)
        {
            StopCoroutine(enemySequenceCoroutine);
            enemySequenceCoroutine = null;
        }

        // 遠ざかる足音を即ストップ
        if (enemyAwayAudio != null && enemyAwayAudio.isPlaying)
        {
            enemyAwayAudio.Stop();
        }

        // 敵の追跡状態を復活させる！
        isBeingChased = true;
        if (invisibleEnemy != null)
        {
            invisibleEnemy.isChasing = true;
        }

        ShowSubtitle("「……ダメだ、また来る！！」", 3.0f);
        Debug.Log("【演出キャンセル】明るさが戻されたため、敵が追跡を再開します！");
    }

    IEnumerator FadeInNoiseSequence()
    {
        isIncreasing = true;
        float timer = 0f;

        bool flag10 = false;
        bool flag30 = false;
        bool flag60 = false;

        while (timer < fadeDuration && !isSolved)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / fadeDuration);

            if (noiseAudio != null)
            {
                noiseAudio.volume = progress * maxVolume;

                if (timer > 60.0f)
                {
                    noiseAudio.pitch = 1.0f + ((timer - 60.0f) / 30.0f) * 0.3f;
                }
            }

            if (!isSolved)
            {
                if (timer >= 10.0f && !flag10)
                {
                    flag10 = true;
                    ShowSubtitle("「……なんだこの音。」", 4.0f);
                }
                else if (timer >= 30.0f && !flag30)
                {
                    flag30 = true;
                    ShowSubtitle("「うるさい……」", 5.0f);
                }
                else if (timer >= 60.0f && !flag60)
                {
                    flag60 = true;
                    ShowSubtitle("「うるさいうるさい……もうやめてくれ……！！！」", 5.0f);
                }
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
    void ShowSubtitle(string text, float duration)
    {
        if (SubtitleManager.Instance != null)
        {
            SubtitleManager.Instance.ShowSubtitle(text, duration);
        }
    }

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
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (mouseLook != null) mouseLook.isChatActive = true;
        if (playerMovement != null) playerMovement.canMove = false;
    }

    public void RestartScene()
    {
        LetterItem.totalCollected = 0;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void SolvePuzzle()
    {
        isSolved = true;
        //StopAllCoroutines();

        if (noiseCoroutine != null)
        {
            StopCoroutine(noiseCoroutine);
        }

        if (tvControllers != null)
        {
            foreach (TVController tv in tvControllers)
            {
                if (tv != null)
                {
                    tv.TurnOffTVByManager();
                }
            }
        }

        if (noiseAudio != null)
        {
            noiseAudio.Stop();
            noiseAudio.pitch = 1.0f;
        }
        if (letterKi != null) letterKi.SetActive(true);

        if (SubtitleManager.Instance != null)
        {
            SubtitleManager.Instance.ShowSubtitle("「静かになった…………。」", 5.0f);
        }

        if (pausePanel != null && pausePanel.activeSelf)
        {
            pausePanel.SetActive(false);
            if (mouseLook != null) mouseLook.isChatActive = false;
            if (playerMovement != null) playerMovement.canMove = true;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        Debug.Log("【謎解き完了】ノイズ停止＆『き』が出現！");
    }

    public void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;

        if (isIncreasing && !isSolved && !hasNoticedVolumeDown)
        {
            if (value < 0.95f && value > 0.001f)
            {
                hasNoticedVolumeDown = true;
                ShowSubtitle("「……あれ？ 今、音が……」", 4.0f);
            }
        }
    }
}