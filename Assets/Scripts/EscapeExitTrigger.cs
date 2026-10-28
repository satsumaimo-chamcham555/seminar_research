using UnityEngine;

/// <summary>
/// 脱出エリアのプレハブ内に置くトリガー（Collider は Is Trigger）。
/// プレイヤーが触れると、ホワイトアウト → 最後のテキスト → END が始まる。
/// </summary>
public class EscapeExitTrigger : MonoBehaviour
{
    private Stage2EndingManager manager;
    private bool fired = false;

    void Start()
    {
        // プレハブはシーンのオブジェクトを直接参照できないので、実行時に探す
        manager = FindObjectOfType<Stage2EndingManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (fired || !other.CompareTag("Player") || manager == null) return;

        fired = true;
        manager.OnReachedExit();
    }
}
