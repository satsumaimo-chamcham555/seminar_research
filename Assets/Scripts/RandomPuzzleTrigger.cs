using UnityEngine;

public class RandomPuzzleTrigger : MonoBehaviour
{
    [Header("謎が出現する確率（0.0 〜 1.0） 例: 0.7 = 70%の確率で出現")]
    [Range(0f, 1f)]
    public float puzzleSpawnChance = 0.7f; 

    void Start()
    {
        ActivateRandomChildPuzzle();
    }

    void ActivateRandomChildPuzzle()
    {
        int childCount = transform.childCount;

        if (childCount == 0)
        {
            Debug.LogWarning("RandomPuzzleTriggerCube の中に謎の子オブジェクトがありません！");
            return;
        }

        // まず「謎を出すか・出さないか」を確率で判定
        if (Random.value > puzzleSpawnChance)
        {
            Debug.Log("【抽選結果】この部屋には何も選ばれませんでした（ハズレ）");
            return;
        }

        // 謎を出すことが決定したら、子オブジェクトからランダムで1つ選ぶ（確実に1つだけ）
        int randomIndex = Random.Range(0, childCount);

        // 選ばれた謎だけをアクティブ化
        GameObject selectedPuzzle = transform.GetChild(randomIndex).gameObject;
        selectedPuzzle.SetActive(true);

        Debug.Log("【抽選結果】この部屋の謎が決定しました: " + selectedPuzzle.name);
    }
}
