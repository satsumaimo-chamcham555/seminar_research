using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    // ★使用済みの謎の名前を記憶するリスト
    private HashSet<string> usedPuzzleNames = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // シーン跨ぎやリセット時用
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 謎がすでに使われているか確認
    public bool IsPuzzleUsed(string puzzleName)
    {
        return usedPuzzleNames.Contains(puzzleName);
    }

    // 謎を使用済みとして登録
    public void RegisterUsedPuzzle(string puzzleName)
    {
        if (!usedPuzzleNames.Contains(puzzleName))
        {
            usedPuzzleNames.Add(puzzleName);
            Debug.Log($"【PuzzleManager】『{puzzleName}』を使用済みに登録しました。");
        }
    }

    // リトライ時などに履歴をリセットしたい場合用
    public void ResetPuzzleHistory()
    {
        usedPuzzleNames.Clear();
    }
}