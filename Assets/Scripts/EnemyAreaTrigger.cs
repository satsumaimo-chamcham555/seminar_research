using UnityEngine;

public class EnemyAreaTrigger : MonoBehaviour
{
    public enum TriggerType { StartChasing, Goal }

    [Header("トリガーの種類設定")]
    public TriggerType triggerType = TriggerType.StartChasing;

    [Header("敵の参照")]
    public InvisibleEnemy enemy;

    // 一度起動・クリアしたかを管理するフラグ
    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;
        if (!other.CompareTag("Player") || enemy == null) return;

        if (triggerType == TriggerType.StartChasing)
        {
            HandleStartZone();
        }
        else
        {
            HandleGoalZone();
        }
    }

    void HandleStartZone()
    {
        // まだ敵が追撃していない場合のみ起動（1回踏んだら無効化）
        if (enemy.isChasing) return;

        isTriggered = true;
        enemy.gameObject.SetActive(true);
        enemy.StartChasing();
    }

    void HandleGoalZone()
    {
        // 敵が【追撃中】の時だけゴール判定を受け付ける
        if (!enemy.isChasing)
        {
            Debug.Log("【ギミック】敵が追撃してきていないため、ゴール判定は無視されました。");
            return;
        }

        isTriggered = true;
        enemy.DefeatEnemy();
    }
}
