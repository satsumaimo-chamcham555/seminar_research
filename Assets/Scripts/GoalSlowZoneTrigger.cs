using UnityEngine;

public class GoalSlowZoneTrigger : MonoBehaviour
{
    [Header("ゴール到達時のセリフ")]
    public string message = "「あれ？ なんだか普通に歩けるようになった……」";
    public float displayDuration = 4.0f;
    public bool triggerOnce = true; // 1回だけ発動

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce) return;
        if (!other.CompareTag("Player")) return;

        // プレイヤーのスロー状態を完全に解除
        PlayerMovement movement = other.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.isInSlowZone = false;
        }

        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.MarkAsCleared("Puzzle_SlowZone");
        }

        // セリフ表示（SubtitleManager が存在する時だけ「発動済み」にする）
        if (SubtitleManager.Show(message, displayDuration))
        {
            hasTriggered = true;
        }
    }
}
