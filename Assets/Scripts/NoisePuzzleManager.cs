using UnityEngine;
using UnityEngine.UI;

public class NoisePuzzleManager : MonoBehaviour
{
    [Header("UI & オーディオ参照")]
    public GameObject pausePanel;    // 設定画面パネル
    public Slider volumeSlider;       // 音量スライダー
    public AudioSource noiseAudio;    // ホワイトノイズ音源
    public MouseLook mouseLook;       // カメラ操作スクリプト
    public GameObject letterKi;       // 獲得できる文字「き」

    private bool isPaused = false;
    private bool isCleared = false;

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (letterKi != null) letterKi.SetActive(false);

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
    }

    void Update()
    {
        // クリアした後でも Esc キーで設定画面を開閉できるように変更
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        if (pausePanel != null) pausePanel.SetActive(isPaused);

        // カーソル固定の解除／復帰
        if (mouseLook != null)
        {
            mouseLook.isChatActive = isPaused;
            mouseLook.LockCursor(!isPaused);
        }
    }

    void OnVolumeChanged(float value)
    {
        if (noiseAudio != null)
        {
            noiseAudio.volume = value;
        }

        // 音量が 0 にされたら謎解きクリア！
        if (value <= 0.001f && !isCleared)
        {
            SolvePuzzle();
        }
    }

    void SolvePuzzle()
    {
        isCleared = true;
        Debug.Log("【2つ目の謎クリア】ホワイトノイズが消えた！");

        if (noiseAudio != null) noiseAudio.Stop();
        if (letterKi != null) letterKi.SetActive(true);
    }
}
