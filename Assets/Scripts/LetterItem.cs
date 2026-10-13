using System.Collections;
using UnityEngine;

public class LetterItem : MonoBehaviour
{
    [Header("文字の設定")]
    public string letterName = "お"; // Inspectorで各文字を設定 ("お", "き", "ろ")

    [Header("メタ演出用参照")]
    public GameClearManager gameClearManager;
    public GameObject chatPanel; // （予備用）

    // ゲーム全体で何文字拾ったかを記録する静的変数
    public static int totalCollected = 0;

    // 3文字そろった時のセリフ（表示する文, 表示秒数, 次のセリフまでの待機秒数）
    private struct Line
    {
        public string text;
        public float duration;
        public float wait;

        public Line(string text, float duration, float wait)
        {
            this.text = text;
            this.duration = duration;
            this.wait = wait;
        }
    }

    private static readonly Line[] TruthLines =
    {
        new Line("「……『お』『き』『ろ』」",          3.0f, 4.0f),
        new Line("「……おきろ。」",                    3.0f, 4.0f),
        new Line("「誰に言ってるんだ？」",            4.0f, 5.0f),
        new Line("「……俺、か？」",                    3.0f, 4.0f),
        new Line("「……」",                            2.0f, 3.0f),
        new Line("「……そうか。」",                    2.0f, 3.0f),
        new Line("「ここ、夢なのか？」",              4.0f, 5.0f),
        new Line("「……だったら、」",                  4.0f, 5.0f),
        new Line("「起きればいい。」",                5.0f, 6.0f),
        new Line("「……おきろ、オキロ、okiro…………」", 5.0f, 6.0f),
    };

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        totalCollected++;

        if (totalCollected == 1)
        {
            SubtitleManager.Show($"「……『{letterName}』？ なんでこんなところに文字が……」", 5.0f);
        }
        else if (totalCollected == 2)
        {
            SubtitleManager.Show($"「また文字だ……『{letterName}』……？」", 5.0f);
        }
        else
        {
            StartCoroutine(PlayTruthSequence());
        }

        Debug.Log($"【文字取得】{letterName} を獲得しました！（通算: {totalCollected}文字目）");

        HideItem();
    }

    /// <summary>拾った文字を見えなくし、再取得できないようにする</summary>
    private void HideItem()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        MeshRenderer mesh = GetComponent<MeshRenderer>();
        if (mesh != null) mesh.enabled = false;
    }

    private IEnumerator PlayTruthSequence()
    {
        foreach (Line line in TruthLines)
        {
            SubtitleManager.Show(line.text, line.duration);
            yield return new WaitForSeconds(line.wait);
        }

        OpenChatOrFallback();

        gameObject.SetActive(false);
    }

    /// <summary>GameClearManager のチャットを開く。見つからなければ直接プレイヤーを止める</summary>
    private void OpenChatOrFallback()
    {
        if (gameClearManager == null)
        {
            gameClearManager = FindObjectOfType<GameClearManager>();
        }

        if (gameClearManager != null)
        {
            gameClearManager.OpenChat();
            return;
        }

        // フォールバック
        if (chatPanel != null) chatPanel.SetActive(true);

        MouseLook mouse = FindObjectOfType<MouseLook>();
        if (mouse != null) mouse.isChatActive = true;

        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null) player.canMove = false;

        CursorUtil.SetLocked(false);
    }
}
