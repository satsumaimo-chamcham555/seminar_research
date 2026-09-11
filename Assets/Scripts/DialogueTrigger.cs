using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("喋らせたいセリフ")]
    public string message = "「体が……重い。進みにくいな……」";
    public float displayDuration = 4.0f;
    public bool triggerOnce = true; // 1回だけ発動するか

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce) return;

        if (other.CompareTag("Player"))
        {
            if (SubtitleManager.Instance != null)
            {
                SubtitleManager.Instance.ShowSubtitle(message, displayDuration);
                hasTriggered = true;
            }
        }
    }
}
