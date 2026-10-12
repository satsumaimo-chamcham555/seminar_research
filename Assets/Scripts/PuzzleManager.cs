using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    // クリアした（＝一度突破した）謎のHashSet
    private HashSet<string> clearedPuzzles = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    // すでにクリア済み（＝出現済み）か判定
    public bool IsCleared(string puzzleName)
    {
        return clearedPuzzles.Contains(puzzleName);
    }

    // クリアした瞬間に呼んで記録する
    public void MarkAsCleared(string puzzleName)
    {
        if (!clearedPuzzles.Contains(puzzleName))
        {
            clearedPuzzles.Add(puzzleName);
            Debug.Log($"【クリア完了】『{puzzleName}』はクリア済みになりました。");
        }
    }

    // ゲームを最初からリセットしたい時用
    public void ResetAllProgress()
    {
        clearedPuzzles.Clear();
    }
}
