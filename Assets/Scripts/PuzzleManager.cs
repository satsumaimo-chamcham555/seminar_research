using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    // クリア済み（＝一度突破した）謎の名前
    private readonly HashSet<string> clearedPuzzles = new HashSet<string>();

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

    public bool IsCleared(string puzzleName)
    {
        return clearedPuzzles.Contains(puzzleName);
    }

    public void MarkAsCleared(string puzzleName)
    {
        if (clearedPuzzles.Add(puzzleName))
        {
            Debug.Log($"【クリア完了】『{puzzleName}』はクリア済みになりました。");
        }
    }

    public void ResetAllProgress()
    {
        clearedPuzzles.Clear();
    }
}
