using UnityEngine;

public class FactoryRoom : MonoBehaviour
{
    [Header("参照設定")]
    public GameObject factoryPrefab;    // 生成するファクトリーのプレハブ
    public Transform nextSpawnPoint;     // 次の部屋を出す位置
    public Animator doorAnimator;        // 閉じるドアのAnimator

    [Header("トリガーの設定タグ")]
    public string playerTag = "Player";

    [HideInInspector]
    public GameObject previousRoom;

    private bool hasSpawnedNext = false;
    private bool isDoorClosed = false;

    // トリガー侵入時の判定
    public void OnChildTriggerEnter(string triggerType, Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (triggerType == "SpawnNext" && !hasSpawnedNext)
        {
            SpawnNextFactory();
        }
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
            GameObject nextRoomObj = Instantiate(factoryPrefab, nextSpawnPoint.position, nextSpawnPoint.rotation);
            
            FactoryRoom nextRoomScript = nextRoomObj.GetComponent<FactoryRoom>();
            if (nextRoomScript != null)
            {
                nextRoomScript.previousRoom = this.gameObject;
            }
        }
    }

    void CloseDoor()
    {
        isDoorClosed = true;

        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Close");
        }

        // 2つ前の部屋（親）が存在すれば1秒後に消去
        if (previousRoom != null)
        {
            Destroy(previousRoom, 1.0f);
        }
    }
}
