using UnityEngine;

/// <summary>
/// 工場プレハブの子 Cube に付ける。プレイヤーが触れたらノイズ＋敵の追跡を開始する。
/// （何度触れても、2回目以降は Manager 側で無視される）
/// </summary>
public class NoiseEnemyStartTrigger : MonoBehaviour
{
    private NoiseEnemyPuzzleManager manager;

    void Start()
    {
        // プレハブはシーン内オブジェクトを直接参照できないので、実行時に探す
        manager = FindObjectOfType<NoiseEnemyPuzzleManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || manager == null) return;

        manager.BeginEvent();
    }
}
