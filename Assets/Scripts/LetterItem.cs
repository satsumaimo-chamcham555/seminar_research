using System.Collections;
using UnityEngine;

public class LetterItem : MonoBehaviour
{
    [Header("文字の設定")]
    public string letterName = "お"; // Inspectorで各文字を設定 ("お", "き", "ろ")

    [Header("メタ演出用参照")]
    public GameClearManager gameClearManager; // ★GameClearManagerを直接指定
    public GameObject chatPanel; // （予備用）

    // ゲーム全体で何文字拾ったかを記録する静的変数
    public static int totalCollected = 0;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            totalCollected++;

            string subtitleText = "";

            if (totalCollected == 1)
            {
                subtitleText = $"「……『{letterName}』？ なんでこんなところに文字が……」";
                ShowSubtitle(subtitleText, 5.0f);
            }
            else if (totalCollected == 2)
            {
                subtitleText = $"「また文字だ……『{letterName}』……？」";
                ShowSubtitle(subtitleText, 5.0f);
            }
            else if (totalCollected >= 3)
            {
                StartCoroutine(PlayTruthSequence());
            }

            Debug.Log($"【文字取得】{letterName} を獲得しました！（通算: {totalCollected}文字目）");

            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            MeshRenderer mesh = GetComponent<MeshRenderer>();
            if (mesh != null) mesh.enabled = false;
        }
    }

    private IEnumerator PlayTruthSequence()
    {
        ShowSubtitle("「……『お』『き』『ろ』」", 3.0f);
        yield return new WaitForSeconds(4.0f);

        ShowSubtitle("「……おきろ。」", 3.0f);
        yield return new WaitForSeconds(4.0f);

        ShowSubtitle("「誰に言ってるんだ？」", 4.0f);
        yield return new WaitForSeconds(5.0f);

        ShowSubtitle("「……俺、か？」", 3.0f);
        yield return new WaitForSeconds(4.0f);

        ShowSubtitle("「……」", 2.0f);
        yield return new WaitForSeconds(3.0f);

        ShowSubtitle("「……そうか。」", 2.0f);
        yield return new WaitForSeconds(3.0f);

        ShowSubtitle("「ここ、夢なのか？」", 4.0f);
        yield return new WaitForSeconds(5.0f);

        ShowSubtitle("「……だったら、」", 4.0f);
        yield return new WaitForSeconds(5.0f);

        ShowSubtitle("「起きればいい。」", 5.0f);
        yield return new WaitForSeconds(6.0f);

        ShowSubtitle("「……おきろ、オキロ、okiro…………」", 5.0f);
        yield return new WaitForSeconds(6.0f);

        // 未割り当ての場合はシーン上から自動検索
        if (gameClearManager == null)
        {
            gameClearManager = FindObjectOfType<GameClearManager>();
        }

        // ★GameClearManager の OpenChat() を呼び出し
        if (gameClearManager != null)
        {
            gameClearManager.OpenChat();
        }
        else
        {
            // それでも見つからない場合のフォールバック（直接止める）
            if (chatPanel != null) chatPanel.SetActive(true);

            MouseLook mouse = FindObjectOfType<MouseLook>();
            if (mouse != null) mouse.isChatActive = true;

            PlayerMovement player = FindObjectOfType<PlayerMovement>();
            if (player != null) player.canMove = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        gameObject.SetActive(false);
    }

    private void ShowSubtitle(string text, float duration)
    {
        if (SubtitleManager.Instance != null)
        {
            SubtitleManager.Instance.ShowSubtitle(text, duration);
        }
    }
}
