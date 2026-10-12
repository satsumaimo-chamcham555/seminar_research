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
        int childCount = transform.childCount;
        if (childCount == 0) return;

        List<GameObject> availablePuzzles = new List<GameObject>();

        for (int i = 0; i < childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            child.SetActive(false);

            // まだクリアしていない謎だけをリストに追加
            if (PuzzleManager.Instance != null)
            {
                if (!PuzzleManager.Instance.IsCleared(child.name))
                {
                    availablePuzzles.Add(child);
                }
            }
            else
            {
                availablePuzzles.Add(child);
            }
        }

        if (availablePuzzles.Count > 0)
        {
            int randomIndex = Random.Range(0, availablePuzzles.Count);
            GameObject selectedPuzzle = availablePuzzles[randomIndex];

            selectedPuzzle.SetActive(true);

            // TVの謎が選ばれた場合の参照セット
            NoisePuzzleManager noiseManager = FindObjectOfType<NoisePuzzleManager>();
            if (noiseManager != null)
            {
                Transform tvsTransform = selectedPuzzle.transform.Find("TVs");
                if (tvsTransform != null)
                {
                    noiseManager.tvParent = tvsTransform;
                }
                else if (selectedPuzzle.GetComponentInChildren<TVController>() != null)
                {
                    noiseManager.tvParent = selectedPuzzle.transform;
                }
            }

            Debug.Log($"【部屋の謎決定】未クリアの謎から『{selectedPuzzle.name}』を出しました。");
        }
        else
        {
            Debug.Log("【部屋の謎】全ての謎をクリアしたため、普通の部屋を配置します。");
        }
    }
}
