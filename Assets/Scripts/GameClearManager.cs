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
    private const float INPUT_BLOCK_DURATION = 0.5f; // 0.5秒ガード

    void Start()
    {
        if (chatInputField != null)
        {
            chatInputField.gameObject.SetActive(false);
            chatInputField.onValueChanged.AddListener(OnTextChanged);
        }
    }

    void Update()
    {
        if (isClearing) return;

        // ★チャットが開いている間の強固なキーブロック処理
        if (isChatOpen)
        {
            if (chatOpenTimer > 0)
            {
                chatOpenTimer -= Time.deltaTime;
            }

            // スペースキーや矢印キーが押されてもUIフォーカスを奪わせない
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || 
                Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                // チャット入力欄へのフォーカスを強制維持
                if (chatInputField != null && !chatInputField.isFocused)
                {
                    chatInputField.ActivateInputField();
                }
            }
            return;
        }

        // Enterキーで手動起動する場合
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OpenChat();
        }
    }

    public void OpenChat()
    {
        isChatOpen = true;
        chatOpenTimer = INPUT_BLOCK_DURATION;

        chatInputField.gameObject.SetActive(true);
        chatInputField.text = "";
        chatInputField.Select();
        chatInputField.ActivateInputField();

        // 視点移動を停止＆カーソル解放
        if (mouseLook != null)
        {
            mouseLook.isChatActive = true;
            mouseLook.LockCursor(false);
            mouseLook.enabled = false;
        }

        // ★修正：PlayerMovementのフラグを直接折る
        if (playerMovement != null)
        {
            playerMovement.canMove = false; // ★移動・ジャンプの操作を直接ロック
        }
    }


    void OnTextChanged(string text)
    {
        if (isClearing) return;

        // ディレイ時間中の入力は自動クリアして無視
        if (chatOpenTimer > 0)
        {
            chatInputField.text = "";
            return;
        }

        string trimmedText = text.Trim();

        if (trimmedText == "おきろ" || trimmedText == "オキロ" || trimmedText == "okiro")
        {
            inputCount++;
            chatInputField.text = "";
            
            ProcessPhase(inputCount);
        }
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
        chatInputField.gameObject.SetActive(false);

        HideObjects(floorObjects);

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        ShowSubtitle("「おきろ！！！！！！！！！」", 4.0f);
        yield return new WaitForSeconds(2.0f);

        ShowSubtitle("「ああ、やっと....」", 4.0f);
        yield return new WaitForSeconds(1.5f);

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

            if (playerController != null)
            {
                playerController.transform.position += Vector3.down * 15.0f * Time.deltaTime;
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
