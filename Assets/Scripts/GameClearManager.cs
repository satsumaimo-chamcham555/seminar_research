using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameClearManager : MonoBehaviour
{
    [Header("UI要素")]
    public TMP_InputField chatInputField;
    public Image whiteoutImage;
    public GameObject[] uiElements;

    [Header("参照")]
    public MouseLook mouseLook;

    [Header("演出設定")]
    public float fadeSpeed = 0.5f;

    private bool isClearing = false;
    private bool isChatOpen = false;

    void Start()
    {
        if (chatInputField != null)
        {
            chatInputField.gameObject.SetActive(false);
            // 文字が変更されるたびにリアルタイムでチェックする
            chatInputField.onValueChanged.AddListener(OnTextChanged);
        }
    }

    void Update()
    {
        if (isClearing) return;

        // Enterキーでチャットを開く
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (!isChatOpen)
            {
                OpenChat();
            }
        }
    }

    void OpenChat()
    {
        isChatOpen = true;
        chatInputField.gameObject.SetActive(true);
        chatInputField.text = ""; // 開いた時は空にする
        chatInputField.Select();
        chatInputField.ActivateInputField();

        if (mouseLook != null)
        {
            mouseLook.isChatActive = true;
            mouseLook.LockCursor(false);
        }
    }

    // 1文字変わるごとに呼ばれる処理（変換確定待ち問題を回避）
    void OnTextChanged(string text)
    {
        // 余計な空白などを除外して判定
        string trimmedText = text.Trim();

        if ((trimmedText == "おきろ" || trimmedText == "オキロ" || trimmedText == "okiro") && !isClearing)
        {
            StartCoroutine(ClearSequence());
        }
    }

    IEnumerator ClearSequence()
    {
        isClearing = true;

        // 1. チャット入力欄を消す
        chatInputField.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.8f);

        // 2. その他のUIを順番に消す
        foreach (GameObject ui in uiElements)
        {
            if (ui != null)
            {
                ui.SetActive(false);
                yield return new WaitForSeconds(0.5f);
            }
        }

        yield return new WaitForSeconds(1.0f);

        // 3. 徐々に画面をホワイトアウトさせる
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            if (whiteoutImage != null)
            {
                Color color = whiteoutImage.color;
                color.a = alpha;
                whiteoutImage.color = color;
            }
            yield return null;
        }

        Debug.Log("【ゲームクリア】夢から目覚めました。");
    }
}
