using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Textを使うために追加

public class SubtitleManager : MonoBehaviour
{
    public static SubtitleManager Instance;

    [Header("UI参照")]
    public Text subtitleText; // TextMeshProUGUI から Text に変更

    private Coroutine currentCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        ShowSubtitle("「これは夢なんだ……。何か普通じゃないことで解決できないか？」", 5.0f);
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
