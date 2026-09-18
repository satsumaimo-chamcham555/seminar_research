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
    public NoisePuzzleManager noisePuzzleManager; // ★追加：マネージャーを参照

    [Header("キー設定")]
    public KeyCode interactKey = KeyCode.E;

    private bool isPlayerInside = false;   
    private bool isTVOn = false;
    private bool isTVInteractive = false;     // ★ノイズが始まっているかのフラグ

    void Start()
    {
        if (interactUI != null)
        {
            interactUI.SetActive(false);
        }

        // ★最初はTVのライトを切っておく（ノイズ開始時に点灯させるため）
        if (tvLight != null) tvLight.enabled = false;
        if (tvScreenEmission != null) tvScreenEmission.SetActive(false);
    }

    void Update()
    {
        // ★ノイズが開始されていて(isTVInteractive)、TVがついていて、プレイヤーが範囲内にいる時だけEキーを受け付ける
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

            // ★ノイズ開始後かつTVがついている時のみ「E:消す」を表示
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

    // ★ノイズイベント開始時にTVをつける関数
    public void TurnOnTV()
    {
        isTVOn = true;
        isTVInteractive = true; // ★これで初めてインタラクト可能になる

        if (tvLight != null) tvLight.enabled = true;
        if (tvScreenEmission != null) tvScreenEmission.SetActive(true);
        if (tvAudioSource != null && !tvAudioSource.isPlaying) tvAudioSource.Play();

        // 既にプレイヤーがトリガー内に立っていた場合、UIを表示する
        if (isPlayerInside && interactUI != null)
        {
            interactUI.SetActive(true);
        }
    }

    // TVの電源を消す処理
    void TurnOffTV()
    {
        isTVOn = false;
        isTVInteractive = false; // イベント終了

        if (tvLight != null) tvLight.enabled = false;
        if (tvAudioSource != null) tvAudioSource.Stop();
        if (tvScreenEmission != null) tvScreenEmission.SetActive(false);
        if (interactUI != null) interactUI.SetActive(false);

        // ★パズル側に「クリアした」と通知する！
        if (noisePuzzleManager != null)
        {
            noisePuzzleManager.SolvePuzzle();
        }

        Debug.Log("TVの電源をオフにして謎解きを完了しました！");
    }
}
