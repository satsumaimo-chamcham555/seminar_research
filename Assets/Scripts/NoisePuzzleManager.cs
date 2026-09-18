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
    public AudioSource noiseAudio;
    public GameObject letterKi;

    [Header("TV参照")]
    public TVController tvController; // ★追加：TVコントローラーをアタッチ

    [Header("プレイヤー参照（操作停止用）")]
    public MouseLook mouseLook;
    public PlayerMovement playerMovement; 

    [Header("ノイズの上昇設定")]
    public float maxVolume = 1.0f;       
    public float fadeDuration = 90.0f;   

    private bool isSolved = false;
    private bool isIncreasing = false;
    private bool hasNoticedVolumeDown = false; // ★追加：音量を少し下げたセリフの既読フラグ
    private Coroutine noiseCoroutine; // ★追加：コルーチン管理用

    void Start()
    {
        if (letterKi != null) letterKi.SetActive(false);
        if (END1Panel != null) END1Panel.SetActive(false);
        
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

        if (mouseLook == null) mouseLook = FindObjectOfType<MouseLook>();
        if (playerMovement == null) playerMovement = FindObjectOfType<PlayerMovement>();
    }

    void Update()
    {
        if (END1Panel != null && END1Panel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Escape))
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

        // ★ノイズ開始時にTVをオンにする（ライトをつける）
        if (tvController != null)
        {
            tvController.TurnOnTV();
        }

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
        if (SubtitleManager.Instance != null && !isSolved)
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
        StopAllCoroutines();

        // ★進行中の音量＆字幕コルーチンを強制停止
        if (noiseCoroutine != null)
        {
            StopCoroutine(noiseCoroutine);
        }

        if (noiseAudio != null)
        {
            noiseAudio.Stop();
            noiseAudio.pitch = 1.0f;
        }
        if (letterKi != null) letterKi.SetActive(true);

        // ★修正：ポーズ画面を閉じる前に字幕を表示するように変更
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

    // ★追加：スライダーを動かした時の処理
    public void OnVolumeChanged(float value)
    {
        AudioListener.volume = value;

        // ノイズイベント中 且つ まだ一度も少し下げたセリフを出していない場合
        if (isIncreasing && !isSolved && !hasNoticedVolumeDown)
        {
            // スライダーの値が0.95以下になったら気づく演出（0に到達する前）
            if (value < 0.95f && value > 0.001f)
            {
                hasNoticedVolumeDown = true;
                ShowSubtitle("「……あれ？ 今、音が……」", 4.0f);
            }
        }
    }
}
