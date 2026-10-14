using System.Collections.Generic;
using UnityEngine;

public class RandomPuzzleTrigger : MonoBehaviour
{
    [Header("固定出題（ステージ1用・任意）")]
    [Tooltip("空ならランダムに出題（従来どおり）。\n子オブジェクトの名前を入れると、その謎だけを必ず出す。\n例：Puzzle_SlowZone / Puzzle_TV / Puzzle_Enemy")]
    public string fixedPuzzleName = "";

    void Start()
    {
        ActivateUniquePuzzle();
    }

    void ActivateUniquePuzzle()
    {
        if (transform.childCount == 0) return;

        DeactivateAllChildren();

        bool isFixed = !string.IsNullOrEmpty(fixedPuzzleName);
        GameObject selected = isFixed ? PickFixedPuzzle() : PickRandomUnclearedPuzzle();
        if (selected == null) return;

        selected.SetActive(true);

        AssignNoisePuzzleReferences(selected);

        string how = isFixed ? "固定指定" : "未クリアの謎";
        Debug.Log($"【部屋の謎決定】{how}から『{selected.name}』を出しました。");
    }

    void DeactivateAllChildren()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    /// <summary>固定モード：クリア状況に関係なく、指定名の子を返す</summary>
    GameObject PickFixedPuzzle()
    {
        Transform puzzle = transform.Find(fixedPuzzleName);
        if (puzzle == null)
        {
            Debug.LogWarning($"【部屋の謎】固定指定『{fixedPuzzleName}』という名前の子オブジェクトが見つかりません。");
            return null;
        }
        return puzzle.gameObject;
    }

    /// <summary>ランダムモード：未クリアの謎の中から1つ選ぶ</summary>
    GameObject PickRandomUnclearedPuzzle()
    {
        var available = new List<GameObject>();

        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            bool isCleared = PuzzleManager.Instance != null && PuzzleManager.Instance.IsCleared(child.name);
            if (!isCleared)
            {
                available.Add(child);
            }
        }

        if (available.Count == 0)
        {
            Debug.Log("【部屋の謎】全ての謎をクリアしたため、普通の部屋を配置します。");
            return null;
        }

        return available[Random.Range(0, available.Count)];
    }

    /// <summary>
    /// TVの謎が選ばれた場合、NoisePuzzleManager に
    /// 「TVの親」と「報酬の『き』」を教える。
    /// </summary>
    void AssignNoisePuzzleReferences(GameObject selectedPuzzle)
    {
        NoisePuzzleManager noiseManager = FindObjectOfType<NoisePuzzleManager>();
        if (noiseManager == null) return;

        Transform tvs = selectedPuzzle.transform.Find("TVs");
        if (tvs != null)
        {
            noiseManager.tvParent = tvs;
        }
        else if (selectedPuzzle.GetComponentInChildren<TVController>() != null)
        {
            noiseManager.tvParent = selectedPuzzle.transform;
        }
        else
        {
            return; // TVの謎ではない
        }

        AssignLetterKi(noiseManager, selectedPuzzle);
    }

    /// <summary>
    /// 選ばれたTVの謎の中にある『き』を、マネージャーの報酬として登録する。
    /// （複数のファクトリーに同じ謎の複製があっても、出した方の『き』が出るようにするため）
    /// </summary>
    void AssignLetterKi(NoisePuzzleManager noiseManager, GameObject selectedPuzzle)
    {
        foreach (LetterItem letter in selectedPuzzle.GetComponentsInChildren<LetterItem>(true))
        {
            if (letter.letterName != "き") continue;

            letter.gameObject.SetActive(false); // 謎を解くまでは非表示
            noiseManager.letterKi = letter.gameObject;
            return;
        }
    }
}
