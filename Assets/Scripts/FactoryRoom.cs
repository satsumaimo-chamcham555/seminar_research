using UnityEngine;

public class FactoryRoom : MonoBehaviour
{
    [Header("参照設定")]
    public GameObject factoryPrefab;    // 生成するファクトリーのプレハブ
    public Transform nextSpawnPoint;    // 次の部屋を出す位置
    public Animator doorAnimator;       // 閉じるドアのAnimator
    public AudioSource doorAudioSource;//閉まるドアの音源

    [Header("トリガーの設定タグ")]
    public string playerTag = "Player";

    [HideInInspector]
    public GameObject previousRoom;

    private bool hasSpawnedNext = false;
    private bool isDoorClosed = false;

    // ------------------------------------------------------------
    // ステージ2エンディング用：ループの切り替え（通常時は null / false のまま＝従来どおり）
    // ------------------------------------------------------------

    /// <summary>セットされている間、次に出す部屋を factoryPrefab の代わりにこのプレハブにする</summary>
    public static GameObject nextRoomOverride;

    /// <summary>true なら、以降は新しい部屋を一切生成しない（ループ終了）</summary>
    public static bool loopStopped;

    public static void ResetLoopOverride()
    {
        nextRoomOverride = null;
        loopStopped = false;
    }

    // シーン再読み込み・エディタの再生ごとに必ず初期化
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        ResetLoopOverride();
    }

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

        if (loopStopped) return; // 脱出エリアを出した後は、二度と生成しない

        bool useOverride = nextRoomOverride != null;
        GameObject prefabToSpawn = useOverride ? nextRoomOverride : factoryPrefab;

        if (prefabToSpawn == null || nextSpawnPoint == null) return;

        GameObject nextRoom = Instantiate(prefabToSpawn, nextSpawnPoint.position, nextSpawnPoint.rotation);

        if (useOverride)
        {
            // 脱出エリアを出した：ループはここで終了（重複生成の防止）
            loopStopped = true;
            nextRoomOverride = null;
        }

        FactoryRoom nextRoomScript = nextRoom.GetComponent<FactoryRoom>();
        if (nextRoomScript != null)
        {
            nextRoomScript.previousRoom = gameObject;
        }
    }

    void CloseDoor()
    {
        isDoorClosed = true;

        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Close");
        }
        //ドア閉まる音の再生
        if (doorAudioSource != null)
        {
            doorAudioSource.Play();
        }

        // 1つ前の部屋が存在すれば1秒後に消去
        if (previousRoom != null)
        {
            Destroy(previousRoom, 1.0f);
        }
    }
}
