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
    private bool isTVInteractive = false;     // ノイズが始まっているかのフラグ

    void Start()
    {
        if (interactUI != null)
        {
            interactUI.SetActive(false);
        }

        // ★マネージャーの参照が空なら自動で探す
        if (noisePuzzleManager == null)
        {
            noisePuzzleManager = FindObjectOfType<NoisePuzzleManager>();
        }

        // ★最初はTVのライト・画面発光を切っておく
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
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;

            // ノイズ開始後かつTVがついている時のみ「E:消す」を表示
            if (isTVInteractive && isTVOn && interactUI != null)
            {
                interactUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            if (interactUI != null) interactUI.SetActive(false);
        }
    }

    // ★ノイズイベント開始時にTVをつける関数（NoisePuzzleManagerから呼ばれる）
    public void TurnOnTV()
    {
        isTVOn = true;
        isTVInteractive = true; // これでインタラクト可能になる

        if (tvLight != null) tvLight.enabled = true;
        if (tvScreenEmission != null) tvScreenEmission.SetActive(true);
        if (tvAudioSource != null && !tvAudioSource.isPlaying) tvAudioSource.Play();

        if (isPlayerInside && interactUI != null)
        {
            interactUI.SetActive(true);
        }
    }

    // TVの電源を消す処理（Eキー押下時）
    void TurnOffTV()
    {
        isTVOn = false;
        isTVInteractive = false;

        if (tvLight != null) tvLight.enabled = false;
        if (tvAudioSource != null) tvAudioSource.Stop();
        if (tvScreenEmission != null) tvScreenEmission.SetActive(false);
        if (interactUI != null) interactUI.SetActive(false);

        // ★パズルマネージャーにクリアを伝える！
        if (noisePuzzleManager != null)
        {
            noisePuzzleManager.SolvePuzzle();
        }

        Debug.Log("TVの電源をオフにして謎解きを完了しました！");
    }

    // マネージャーから一括消去される際の処理
    public void TurnOffTVByManager()
    {
        isTVOn = false;
        isTVInteractive = false;

        if (tvLight != null) tvLight.enabled = false;
        if (tvAudioSource != null) tvAudioSource.Stop();
        if (tvScreenEmission != null) tvScreenEmission.SetActive(false);
        if (interactUI != null) interactUI.SetActive(false);
    }
}
