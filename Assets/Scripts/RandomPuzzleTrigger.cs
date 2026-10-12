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

            if (PuzzleManager.Instance != null)
            {
                if (!PuzzleManager.Instance.IsPuzzleUsed(child.name))
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

            if (PuzzleManager.Instance != null)
            {
                PuzzleManager.Instance.RegisterUsedPuzzle(selectedPuzzle.name);
            }

            // ★TVの謎が選ばれた場合、NoisePuzzleManagerにTVの親オブジェクトを設定する
            NoisePuzzleManager noiseManager = FindObjectOfType<NoisePuzzleManager>();
            if (noiseManager != null)
            {
                // アクティブ化された謎の中に "TVs" や TVController が含まれているか確認
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

            Debug.Log($"【部屋の謎決定】未消化の謎から『{selectedPuzzle.name}』を有効化しました。");
        }
        else
        {
            Debug.Log("【部屋の謎】すべての謎が出尽くしたため、この部屋には謎を配置しません。");
        }
    }
}
