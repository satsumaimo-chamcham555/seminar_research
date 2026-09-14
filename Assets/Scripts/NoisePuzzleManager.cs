using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NoisePuzzleManager : MonoBehaviour
{
    [Header("UI & オーディオ参照")]
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public Button retryButton;
    public Slider volumeSlider;
    public AudioSource noiseAudio;
    public GameObject letterKi;

    [Header("ノイズの上昇設定")]
    public float maxVolume = 1.0f;       
    public float fadeDuration = 90.0f;   

    private bool isSolved = false;
    private bool isIncreasing = false;

    void Start()
    {
        if (letterKi != null) letterKi.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        
        if (noiseAudio != null)
        {
            noiseAudio.volume = 0f;
            noiseAudio.pitch = 1.0f;
        }

        // リトライボタンにシーン再読み込み処理を自動登録
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
            retryButton.onClick.AddListener(RestartScene);
        }
    }

    void Update()
    {
        // ゲームオーバーパネルが表示されている時はポーズなどの入力を受け付けない
        if (gameOverPanel != null && gameOverPanel.activeSelf) return;

        // Escキーによるポーズ画面の開閉処理
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausePanel != null)
            {
                bool isActive = !pausePanel.activeSelf;
                pausePanel.SetActive(isActive);

                Cursor.lockState = isActive ? CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = isActive;
            }
        }

        if (isSolved) return;

        // 音量スライダーが0になったらクリア判定
        if (pausePanel != null && pausePanel.activeSelf && volumeSlider != null)
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

        if (noiseAudio != null)
        {
            if (!noiseAudio.isPlaying) noiseAudio.Play();
            StartCoroutine(FadeInNoiseSequence());
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
                ShowSubtitle("「なんだこの音、、、」", 4.0f);
            }
            else if (timer >= 30.0f && !flag30)
            {
                flag30 = true;
                ShowSubtitle("「うるさい、、、どうにかしてこの音を消せないか？」", 5.0f);
            }
            else if (timer >= 60.0f && !flag60)
            {
                flag60 = true;
                ShowSubtitle("「うるさいうるさい！早く、、、！！！」", 5.0f);
            }

            yield return null;
        }

        if (!isSolved)
        {
            if (noiseAudio != null) noiseAudio.volume = maxVolume;

            ShowSubtitle("「もう、、ダメだ、、、」", 3.0f);
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

    // ★ゲームオーバー時にノイズ音を停止する
    if (noiseAudio != null)
    {
        noiseAudio.Stop();
        noiseAudio.pitch = 1.0f; // ピッチも元に戻す
    }

    if (gameOverPanel != null)
    {
        gameOverPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}


   // リトライ処理
    public void RestartScene()
    {
        // ★追加：リトライ時に文字の獲得カウントをリセット
        LetterItem.totalCollected = 0;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }


    void SolvePuzzle()
    {
        isSolved = true;
        StopAllCoroutines();

        if (noiseAudio != null)
        {
            noiseAudio.Stop();
            noiseAudio.pitch = 1.0f;
        }
        if (letterKi != null) letterKi.SetActive(true);

        //ShowSubtitle("「……音が消えた。何か落ちてきたみたいだ」", 4.0f);

        Debug.Log("【謎解き完了】ノイズ停止＆『き』が出現！");
    }
}
