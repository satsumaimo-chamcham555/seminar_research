using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("喋らせたいセリフ")]
    public string message = "「うまく動けない……なんで？」";
    public float displayDuration = 4.0f;
    public bool triggerOnce = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement movement = other.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.isInSlowZone = true; // スローゾーンに入ったフラグを立てる
            }

            if (!hasTriggered || !triggerOnce)
            {
                if (SubtitleManager.Instance != null)
                {
                    SubtitleManager.Instance.ShowSubtitle(message, displayDuration);
                    hasTriggered = true;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement movement = other.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.isInSlowZone = false; // スローゾーンから出たら通常状態に戻す
            }
        }
    }
}
