using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameClearManager : MonoBehaviour
{
    [Header("UI要素")]
    public TMP_InputField chatInputField;
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

    [Tooltip("正解してから次のチャット欄が自動で開くまでの時間（秒）")]
    public float reopenDelay = 1.0f; // ★ Inspectorから変更できます（初期値：1秒）

    private int inputCount = 0;
    private bool isClearing = false;
    private bool isChatOpen = false;

    private float chatOpenTimer = 0f;
    private const float INPUT_BLOCK_DURATION = 0.2f;

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

        // 1. チャットが開いている時
        if (isChatOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (chatOpenTimer > 0)
            {
                chatOpenTimer -= Time.unscaledDeltaTime;
                return;
            }

            // Escキーで手動キャンセルして閉じる
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseChat();
                return;
            }

            // Enterキーで送信して閉じる
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                SubmitAndClose();
            }
            return;
        }

        // 2. チャットが閉じている時：Enterキーで開く
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OpenChat();
        }
    }

    private void SubmitAndClose()
    {
        string inputText = "";
        if (chatInputField != null)
        {
            inputText = chatInputField.text.Trim();
        }

        // 間違い・正解に関わらず一旦閉じる
        CloseChat();

        // 正解判定
        if (inputText == "おきろ" || inputText == "オキロ" || inputText == "okiro" || inputText == "OKIRO" || inputText == "おきる" || inputText == "オキル" || inputText == "okiru" || inputText == "OKIRU")
        {
            inputCount++;
            ProcessPhase(inputCount);
        }
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
                StartCoroutine(ReopenChatAfterDelay(reopenDelay));
                break;

            case 2:
                ShowSubtitle("「……おきろ！」", 3.0f);
                HideObjects(pillarObjects);
                StartCoroutine(ReopenChatAfterDelay(reopenDelay));
                break;

            case 3:
                ShowSubtitle("「おきろ！！！！」", 3.0f);
                HideObjects(wallRoofObjects);
                StartCoroutine(ReopenChatAfterDelay(reopenDelay));
                break;

            case 4:
                StartCoroutine(FinalPhaseSequence());
                break;
        }
    }

    // 少し時間を置いてからチャットを開くコルーチン
    IEnumerator ReopenChatAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!isClearing)
        {
            OpenChat();
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

        yield return new WaitForSeconds(1.0f);

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
