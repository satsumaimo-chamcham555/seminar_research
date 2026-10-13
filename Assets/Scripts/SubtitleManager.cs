using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SubtitleManager : MonoBehaviour
{
    public static SubtitleManager Instance;

    [Header("UI参照")]
    public Text subtitleText;

    private Coroutine currentCoroutine;

    /// <summary>
    /// null チェック込みの字幕表示ヘルパー（新規追加）。
    /// 各クラスにあった「if (SubtitleManager.Instance != null) ...」と
    /// private ShowSubtitle の重複をこれに統一。
    /// Instance が存在したかどうかを返す。
    /// </summary>
    public static bool Show(string text, float duration = 4.0f)
    {
        if (Instance == null) return false;
        Instance.ShowSubtitle(text, duration);
        return true;
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        ShowSubtitle(" ", 5.0f);
    }

    public void ShowSubtitle(string text, float duration = 4.0f)
    {
        if (subtitleText == null) return;

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(DisplayRoutine(text, duration));
    }

    IEnumerator DisplayRoutine(string text, float duration)
    {
        subtitleText.text = text;
        subtitleText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        subtitleText.gameObject.SetActive(false);
    }
}
