using System.Collections.Generic;
using UnityEngine;

public class RandomPuzzleTrigger : MonoBehaviour
{
    void Start()
    {
        ActivateUniquePuzzle();
    }

    void ActivateUniquePuzzle()
    {
        if (transform.childCount == 0) return;

        List<GameObject> availablePuzzles = CollectAvailablePuzzles();

        if (availablePuzzles.Count == 0)
        {
            Debug.Log("【部屋の謎】全ての謎をクリアしたため、普通の部屋を配置します。");
            return;
        }

        GameObject selected = availablePuzzles[Random.Range(0, availablePuzzles.Count)];
        selected.SetActive(true);

        AssignTvParent(selected);

        Debug.Log($"【部屋の謎決定】未クリアの謎から『{selected.name}』を出しました。");
    }

    /// <summary>全ての子を一旦非表示にしつつ、未クリアの謎だけを返す</summary>
    List<GameObject> CollectAvailablePuzzles()
    {
        var result = new List<GameObject>();

        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            child.SetActive(false);

            bool isCleared = PuzzleManager.Instance != null && PuzzleManager.Instance.IsCleared(child.name);
            if (!isCleared)
            {
                result.Add(child);
            }
        }
        return result;
    }

    /// <summary>TVの謎が選ばれた場合、NoisePuzzleManager に TV の親を教える</summary>
    void AssignTvParent(GameObject selectedPuzzle)
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
    }
}
