using UnityEngine;

public class LetterItem : MonoBehaviour
{
    [Header("文字の設定")]
    public string letterName = "お";
    public string obtainMessage = "「『お』の文字を手に入れた……？ なぜこんなところに……」";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 主人公のセリフを表示
            if (SubtitleManager.Instance != null)
            {
                SubtitleManager.Instance.ShowSubtitle(obtainMessage, 4.0f);
            }

            Debug.Log($"【文字取得】{letterName} を獲得しました！");

            // 取得したら文字を消す
            gameObject.SetActive(false);
        }
    }
}
