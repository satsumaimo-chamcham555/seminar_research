using UnityEngine;

public class FactoryRoom : MonoBehaviour
{
    [Header("参照設定")]
    public GameObject factoryPrefab;    // 生成するファクトリーのプレハブ
    public Transform nextSpawnPoint;     // 次の部屋を出す位置 (SpawnPoint)
    public Animator doorAnimator;        // 閉じるドアのAnimator

    [Header("トリガーの設定タグ")]
    public string playerTag = "Player";

    // ★追加：1つ前の部屋（親）を覚えておくための変数
    [HideInInspector]
    public GameObject previousRoom;

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
            // 次のファクトリーを生成
            GameObject nextRoomObj = Instantiate(factoryPrefab, nextSpawnPoint.position, nextSpawnPoint.rotation);
            
            // ★追加：生成された新しい部屋に「お前の1つ前の部屋は自分（この部屋）だよ」と教えてあげる
            FactoryRoom nextRoomScript = nextRoomObj.GetComponent<FactoryRoom>();
            if (nextRoomScript != null)
            {
                nextRoomScript.previousRoom = this.gameObject;
            }

            Debug.Log("新しいファクトリーを生成しました。");
        }
    }

    void CloseDoor()
    {
        isDoorClosed = true;

        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Close");
            Debug.Log("ドアが閉まりました。");
        }

        // ★変更：自分自身ではなく「1つ前の部屋」が存在すれば、それを消去する！
        if (previousRoom != null)
        {
            Destroy(previousRoom, 5.0f); // ドアが閉まりきった5秒後に消去
            Debug.Log("2つ前の部屋（元のファクトリー）を消去しました。");
        }
    }
}
