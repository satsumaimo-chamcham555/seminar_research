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

        if (other.CompareTag("Player"))
        {
            // プレイヤーのスロー状態を完全に解除する
            PlayerMovement movement = other.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.isInSlowZone = false;
            }

            if (PuzzleManager.Instance != null)
            {
                PuzzleManager.Instance.MarkAsCleared("Puzzle_SlowZone");
            }
            // ゴールセリフを表示
            if (SubtitleManager.Instance != null)
            {
                SubtitleManager.Instance.ShowSubtitle(message, displayDuration);
                hasTriggered = true;
            }
        }
    }
}
