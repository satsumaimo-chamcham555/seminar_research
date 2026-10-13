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
        if (!other.CompareTag("Player")) return;

        SetSlowZone(other, true); // スローゾーンに入ったフラグを立てる

        if (!hasTriggered || !triggerOnce)
        {
            // SubtitleManager が存在する時だけ「発動済み」にする
            if (SubtitleManager.Show(message, displayDuration))
            {
                hasTriggered = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        SetSlowZone(other, false); // スローゾーンから出たら通常状態に戻す
    }

    private void SetSlowZone(Collider player, bool value)
    {
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.isInSlowZone = value;
        }
    }
}
