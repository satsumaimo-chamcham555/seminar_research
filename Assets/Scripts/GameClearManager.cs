using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameClearManager : MonoBehaviour
{
    [Header("UI要素")]
    public TMP_InputField chatInputField;
    public Image whiteoutImage;

    [Header("参照")]
    public MouseLook mouseLook;
    public PlayerMovement playerMovement;
    public CharacterController playerController;

    [Header("フェーズ別消去オブジェクト群")]
    public GameObject[] propsObjects;    // 1回目：小物たち
    public GameObject[] pillarObjects;   // 2回目：柱
    public GameObject[] wallRoofObjects; // 3回目：壁・屋根
    public GameObject[] floorObjects;    // 4回目：床

    [Header("演出設定")]
    public float fadeSpeed = 0.5f;

    private int inputCount = 0;
    private bool isClearing = false;
    private bool isChatOpen = false;

    private float chatOpenTimer = 0f;
    private const float INPUT_BLOCK_DURATION = 0.3f; // 開いた直後の誤作動防止ガード時間

    void Start()
    {
        if (chatInputField != null)
        {
            chatInputField.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isClearing) return;

        // ★1. チャットが開いている時の処理
        if (isChatOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (chatOpenTimer > 0)
            {
                chatOpenTimer -= Time.unscaledDeltaTime;
                return; // ガード時間中は一切の判定を行わない
            }

            // 【Escキー】または【テンキーのEnter/MainのEnter】が押された時の判定
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseChat();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                CheckAndSubmit();
                return;
            }

            // UIのフォーカスを維持
            if (chatInputField != null && !chatInputField.isFocused)
            {
                chatInputField.ActivateInputField();
            }
            return;
        }

        // ★2. チャットが開いていない時：Enterキーで手動起動
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OpenChat();
        }
    }

    // ★Enterキーが押された時の入力判定処理
    void CheckAndSubmit()
    {
        if (chatInputField == null) return;

        string trimmedText = chatInputField.text.Trim();

        // 正解チェック
        if (trimmedText == "おきろ" || trimmedText == "オキロ" || trimmedText == "okiro")
        {
            inputCount++;
            chatInputField.text = "";
            ProcessPhase(inputCount);
        }
        else
        {
            // 不正解・空文字の場合は確定でチャットを閉じてゲームへ復帰
            CloseChat();
        }
    }

    // ★チャットを開く
    public void OpenChat()
    {
        isChatOpen = true;
        chatOpenTimer = INPUT_BLOCK_DURATION;

        if (mouseLook != null) mouseLook.isChatActive = true;
        if (playerMovement != null) playerMovement.canMove = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (chatInputField != null)
        {
            chatInputField.gameObject.SetActive(true);
            chatInputField.text = "";
            chatInputField.Select();
            chatInputField.ActivateInputField();
        }
    }

    // ★チャットを完全に閉じ、ゲーム操作を100%復帰させる
    public void CloseChat()
    {
        isChatOpen = false;

        if (chatInputField != null)
        {
            chatInputField.text = "";
            chatInputField.DeactivateInputField(); // フォーカス解除
            chatInputField.gameObject.SetActive(false); // 非表示
        }

        // 操作の復帰
        if (mouseLook != null) mouseLook.isChatActive = false;
        if (playerMovement != null) playerMovement.canMove = true;

        // カーソルを完全にゲーム画面中央へ固定＆非表示
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void ProcessPhase(int count)
    {
        switch (count)
        {
            case 1:
                ShowSubtitle("「...おきろ」", 3.0f);
                HideObjects(propsObjects);
                RefocusChat();
                break;

            case 2:
                ShowSubtitle("「...おきろ！」", 3.0f);
                HideObjects(pillarObjects);
                RefocusChat();
                break;

            case 3:
                ShowSubtitle("「おきろ！！！！」", 3.0f);
                HideObjects(wallRoofObjects);
                RefocusChat();
                break;

            case 4:
                StartCoroutine(FinalPhaseSequence());
                break;
        }
    }

    void RefocusChat()
    {
        if (chatInputField != null)
        {
            chatInputField.Select();
            chatInputField.ActivateInputField();
        }
    }

    void HideObjects(GameObject[] targetArray)
    {
        if (targetArray == null) return;
        foreach (GameObject obj in targetArray)
        {
            if (obj != null) obj.SetActive(false);
        }
    }

    IEnumerator FinalPhaseSequence()
    {
        isClearing = true;
        isChatOpen = false;

        if (chatInputField != null)
        {
            chatInputField.gameObject.SetActive(false);
        }

        HideObjects(floorObjects);

        Transform targetTransform = transform;
        if (playerMovement != null) targetTransform = playerMovement.transform;
        else if (playerController != null) targetTransform = playerController.transform;

        Transform cameraTransform = mouseLook != null ? mouseLook.transform : null;

        ShowSubtitle("「おきろ！！！！！！！！！」", 4.0f);

        float fallSpeed = 5.0f;
        float timer = 0f;

        while (timer < 1.5f)
        {
            timer += Time.deltaTime;

            fallSpeed += 30.0f * Time.deltaTime;
            Vector3 fallVector = Vector3.down * fallSpeed * Time.deltaTime;

            if (targetTransform != null) targetTransform.position += fallVector;
            if (cameraTransform != null && cameraTransform != targetTransform && !cameraTransform.IsChildOf(targetTransform))
            {
                cameraTransform.position += fallVector;
            }

            yield return null;
        }

        ShowSubtitle("「ああ、やっと....」", 4.0f);

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

            fallSpeed += 30.0f * Time.deltaTime;
            Vector3 fallVector = Vector3.down * fallSpeed * Time.deltaTime;

            if (targetTransform != null) targetTransform.position += fallVector;
            if (cameraTransform != null && cameraTransform != targetTransform && !cameraTransform.IsChildOf(targetTransform))
            {
                cameraTransform.position += fallVector;
            }

            yield return null;
        }

        Debug.Log("【ゲームクリア】現実へ目覚めました。");
    }

    void ShowSubtitle(string text, float duration)
    {
        if (SubtitleManager.Instance != null)
        {
            SubtitleManager.Instance.ShowSubtitle(text, duration);
        }
    }
}
