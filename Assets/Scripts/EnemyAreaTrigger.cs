using UnityEngine;

public class EnemyAreaTrigger : MonoBehaviour
{
    public enum TriggerType { StartChasing, Goal }

    [Header("トリガーの種類設定")]
    public TriggerType triggerType = TriggerType.StartChasing;

    [Header("敵の参照")]
    public InvisibleEnemy enemy;

    // ★一度起動・クリアしたかを管理するフラグ
    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // 既に発動済みのトリガーなら何もしない
        if (isTriggered) return;

        if (other.CompareTag("Player") && enemy != null)
        {
            if (triggerType == TriggerType.StartChasing)
            {
                // ★まだ敵が追撃していない場合のみ起動
                if (!enemy.isChasing)
                {
                    isTriggered = true; // スタートゾーンは1回踏んだら無効化

                    // 【追加】敵オブジェクトを有効化（アクティブ化）する
                    enemy.gameObject.SetActive(true);

                    // 追撃開始
                    enemy.StartChasing();
                }
            }
            else if (triggerType == TriggerType.Goal)
            {
                // ★重要：敵が【追撃中】の時だけゴール判定を受け付ける
                if (enemy.isChasing)
                {
                    isTriggered = true; // ゴール成立で無効化
                    enemy.DefeatEnemy();
                    
                    // 【お好みで】撃破時に敵を非表示にしたい場合は以下を有効化
                    // enemy.gameObject.SetActive(false);
                }
                else
                {
                    Debug.Log("【ギミック】敵が追撃してきていないため、ゴール判定は無視されました。");
                }
            }
        }
    }
}
