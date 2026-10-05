using UnityEngine;

public class FactoryRoom : MonoBehaviour
{
    [Header("参照設定")]
    public GameObject factoryPrefab;    // 生成するファクトリーのプレハブ
    public Transform nextSpawnPoint;     // 次の部屋を出す位置 (SpawnPoint)
    public Animator doorAnimator;        // 閉じるドアのAnimator

    [Header("トリガーの設定タグ")]
    public string playerTag = "Player";

    private bool hasSpawnedNext = false;
    private bool isDoorClosed = false;

    // トリガー侵入時の判定
    public void OnChildTriggerEnter(string triggerType, Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        // 1. 次のファクトリーを増殖させるトリガー
        if (triggerType == "SpawnNext" && !hasSpawnedNext)
        {
            SpawnNextFactory();
        }
        // 2. ドアを閉めて退路を断つトリガー
        else if (triggerType == "CloseDoor" && !isDoorClosed)
        {
            CloseDoor();
        }
    }

    void SpawnNextFactory()
    {
        hasSpawnedNext = true;

        if (factoryPrefab != null && nextSpawnPoint != null)
        {
            // 次のファクトリーを生成して繋げる
            Instantiate(factoryPrefab, nextSpawnPoint.position, nextSpawnPoint.rotation);
            Debug.Log("新しいファクトリーを生成しました。");
        }
    }

    void CloseDoor()
    {
        isDoorClosed = true;

        if (doorAnimator != null)
        {
            // すでに実装済みのドアを閉めるトリガー（例: "Close"）を再生
            doorAnimator.SetTrigger("Close");
            Debug.Log("ドアが閉まり、退路が断たれました。");
        }
        // ★追加：ドアが閉まり始めてから1秒後にこのファクトリー（部屋）全体を削除する
        Destroy(gameObject, 1.0f);
        Debug.Log("1秒後に古いファクトリーを削除します。");
    }
}
