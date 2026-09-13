using UnityEngine;

public class EnemyAreaTrigger : MonoBehaviour
{
    public enum TriggerType { StartChasing, Goal }

    [Header("トリガーの種類設定")]
    public TriggerType triggerType = TriggerType.StartChasing;

    [Header("敵の参照")]
    public InvisibleEnemy enemy;

    // ★一度でも謎をクリアしたか管理するフラグ
    private static bool isCleared = false;

    private void OnTriggerEnter(Collider other)
    {
        // 既にクリア済みの場合は二度と発動しない
        if (isCleared) return;

        if (other.CompareTag("Player") && enemy != null)
        {
            if (triggerType == TriggerType.StartChasing)
            {
                enemy.StartChasing();
            }
            else if (triggerType == TriggerType.Goal)
            {
                // ゴール到達時にクリアフラグを立てる
                isCleared = true;
                enemy.DefeatEnemy();
            }
        }
    }
}
