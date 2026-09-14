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
                subtitleText = $"「……『{letterName}』？ なんでこんなところに文字があるんだ……」";
                ShowSubtitle(subtitleText, 5.0f);
            }
            else if (totalCollected == 2)
            {
                subtitleText = $"「また文字だ……『{letterName}』……何かのメッセージか……？」";
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
        ShowSubtitle("「これで最後か……？ 『お』『き』『ろ』……おきろ……？」", 5.0f);
        yield return new WaitForSeconds(7.0f);

        ShowSubtitle("「おきろ、って……誰に？」", 5.0f);
        yield return new WaitForSeconds(7.0f);

        ShowSubtitle("「……俺に？」", 5.0f);
        yield return new WaitForSeconds(7.0f);

        ShowSubtitle("「……」", 5.0f);
        yield return new WaitForSeconds(7.0f);

        ShowSubtitle("「……そうか。」", 5.0f);
        yield return new WaitForSeconds(7.0f);

        ShowSubtitle("「これは夢なんだ。」", 5.0f);
        yield return new WaitForSeconds(7.0f);

        ShowSubtitle("「だったら、起きればいい。」", 5.0f);
        yield return new WaitForSeconds(5.0f);

        // ★GameClearManager経由でチャットを開く
        if (gameClearManager != null)
        {
            // メソッドを呼び出して入力状態まで完了させる
            gameClearManager.SendMessage("OpenChat", SendMessageOptions.DontRequireReceiver);
        }
        else if (chatPanel != null)
        {
            chatPanel.SetActive(true);
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
