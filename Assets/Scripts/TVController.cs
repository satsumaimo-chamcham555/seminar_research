using UnityEngine;

public class TVController : MonoBehaviour
{
    [Header("UI設定")]
    public GameObject interactUI;

    [Header("消去対象のコンポーネント")]
    public Light tvLight;
    public AudioSource tvAudioSource;
    public GameObject tvScreenEmission;

    [Header("パズルマネージャー参照")]
    public NoisePuzzleManager noisePuzzleManager;

    [Header("キー設定")]
    public KeyCode interactKey = KeyCode.E;

    private bool isPlayerInside = false;
    private bool isTVOn = false;
    private bool isTVInteractive = false; // ノイズが始まっているかのフラグ

    void Start()
    {
        SetInteractUI(false);

        if (noisePuzzleManager == null)
        {
            noisePuzzleManager = FindObjectOfType<NoisePuzzleManager>();
        }

        // 最初はTVのライト・画面発光を切っておく
        if (tvLight != null) tvLight.enabled = false;
        if (tvScreenEmission != null) tvScreenEmission.SetActive(false);
    }

    void Update()
    {
        // ノイズ開始済み ＆ 範囲内 ＆ TV点灯中に Eキー で消す
        if (isTVInteractive && isPlayerInside && isTVOn && Input.GetKeyDown(interactKey))
        {
            TurnOffTV();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = true;

        // ノイズ開始後かつTVがついている時のみ「E:消す」を表示
        if (isTVInteractive && isTVOn)
        {
            SetInteractUI(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = false;
        SetInteractUI(false);
    }

    /// <summary>ノイズイベント開始時にTVをつける（NoisePuzzleManagerから呼ばれる）</summary>
    public void TurnOnTV()
    {
        isTVOn = true;
        isTVInteractive = true;

        if (tvLight != null) tvLight.enabled = true;
        if (tvScreenEmission != null) tvScreenEmission.SetActive(true);
        if (tvAudioSource != null && !tvAudioSource.isPlaying) tvAudioSource.Play();

        if (isPlayerInside)
        {
            SetInteractUI(true);
        }
    }

    /// <summary>マネージャーから一括消去される際の処理</summary>
    public void TurnOffTVByManager()
    {
        SwitchOff();
    }

    /// <summary>Eキー押下による電源オフ＋クリア通知</summary>
    void TurnOffTV()
    {
        SwitchOff();

        if (noisePuzzleManager != null)
        {
            noisePuzzleManager.SolvePuzzle();
        }

        Debug.Log("TVの電源をオフにして謎解きを完了しました！");
    }

    /// <summary>TVの見た目・音・状態をオフにする共通処理</summary>
    void SwitchOff()
    {
        isTVOn = false;
        isTVInteractive = false;

        if (tvLight != null) tvLight.enabled = false;
        if (tvAudioSource != null) tvAudioSource.Stop();
        if (tvScreenEmission != null) tvScreenEmission.SetActive(false);
        SetInteractUI(false);
    }

    void SetInteractUI(bool active)
    {
        if (interactUI != null) interactUI.SetActive(active);
    }
}
