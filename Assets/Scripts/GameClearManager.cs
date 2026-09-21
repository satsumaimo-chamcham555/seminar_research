using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameClearManager : MonoBehaviour
{
    [Header("UI要素")]
    public WebGLNativeInputField chatInputField;
    public Image whiteoutImage;
    public GameObject END3Panel;

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
    private const float INPUT_BLOCK_DURATION = 0.2f; // 開いた直後の誤作動防止

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

        // ★1. チャットが開いている時
        if (isChatOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (chatOpenTimer > 0)
            {
                chatOpenTimer -= Time.unscaledDeltaTime;
                return;
            }

            // Escキーで手動キャンセル
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseChat();
                return;
            }

            // ★Enterキー（メイン/テンキー）が押されたら文字判定を実行
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                CheckAndSubmit();
            }
            return;
        }

        // ★2. チャットが開いていない時：Enterキーで開く
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OpenChat();
        }
    }

    // ★Enter入力時の直接判定メソッド
    private void CheckAndSubmit()
    {
        if (chatInputField == null) return;

        string trimmedText = chatInputField.text.Trim();

        // まずチャット欄を閉じて入力状態をリセット
        CloseChat();

        // 正解判定
        if (trimmedText == "おきろ" || trimmedText == "オキロ" || trimmedText == "okiro")
        {
            inputCount++;
            ProcessPhase(inputCount);
        }
        // 間違っている場合（または空文字）は CloseChat() により閉じたままゲーム復帰
    }

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
            chatInputField.ActivateInputField();
        }
    }

    public void CloseChat()
    {
        isChatOpen = false;

        if (chatInputField != null)
        {
            chatInputField.text = "";
            chatInputField.DeactivateInputField();
            chatInputField.gameObject.SetActive(false);
        }

        if (mouseLook != null) mouseLook.isChatActive = false;
        if (playerMovement != null) playerMovement.canMove = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void ProcessPhase(int count)
    {
        switch (count)
        {
            case 1:
                ShowSubtitle("「……おきろ」", 3.0f);
                HideObjects(propsObjects);
                OpenChat(); // 演出後に空のチャットを自動再開
                break;

            case 2:
                ShowSubtitle("「……おきろ！」", 3.0f);
                HideObjects(pillarObjects);
                OpenChat();
                break;

            case 3:
                ShowSubtitle("「おきろ！！！！」", 3.0f);
                HideObjects(wallRoofObjects);
                OpenChat();
                break;

            case 4:
                StartCoroutine(FinalPhaseSequence());
                break;
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

        ShowSubtitle("「……夢から、おきるんだ。」", 4.0f);

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
        // 少し余韻を残すため0.5秒待機
        yield return new WaitForSeconds(1.0f);

        // ★追加：真っ白になった後にEND3パネルを表示＆マウス操作解放
        if (END3Panel != null)
        {
            END3Panel.SetActive(true);
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (mouseLook != null) mouseLook.isChatActive = true;
        if (playerMovement != null) playerMovement.canMove = false;

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
