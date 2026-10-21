using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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
    public float reopenDelay = 1.0f;

    private static readonly HashSet<string> CorrectAnswers = new HashSet<string>
    {
        "おきろ", "オキロ", "okiro", "OKIRO",
        "おきる", "オキル", "okiru", "OKIRU",
    };

    private const float InputBlockDuration = 0.2f; // チャットを開いた直後の入力無視時間
    private const float InitialFallSpeed = 5.0f;
    private const float FallAcceleration = 30.0f;
    private const float FallDuration = 1.5f;

    private int inputCount = 0;
    private bool isClearing = false;
    private bool isChatOpen = false;
    private float chatOpenTimer = 0f;

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

        if (isChatOpen)
        {
            UpdateChatOpen();
            return;
        }

        // チャットが閉じている時：Enterキーで開く
        if (IsSubmitKeyDown())
        {
            OpenChat();
        }
    }

    void UpdateChatOpen()
    {
        CursorUtil.SetLocked(false);

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

        // Enterキーで送信
        if (IsSubmitKeyDown())
        {
            SubmitAndClose();
        }
    }

    static bool IsSubmitKeyDown()
    {
        return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
    }

    // ------------------------------------------------------------
    // チャットの開閉
    // ------------------------------------------------------------

    public void OpenChat()
    {
        isChatOpen = true;
        chatOpenTimer = InputBlockDuration;

        SetPlayerInputBlocked(true);
        CursorUtil.SetLocked(false);

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

        SetPlayerInputBlocked(false);
        CursorUtil.SetLocked(true);
    }

    void SetPlayerInputBlocked(bool blocked)
    {
        if (mouseLook != null) mouseLook.isChatActive = blocked;
        if (playerMovement != null) playerMovement.canMove = !blocked;
    }

    void SubmitAndClose()
    {
        string inputText = chatInputField != null ? chatInputField.text.Trim() : "";

        // 間違い・正解に関わらず一旦閉じる
        CloseChat();

        if (CorrectAnswers.Contains(inputText))
        {
            inputCount++;
            ProcessPhase(inputCount);
        }
    }

    // ------------------------------------------------------------
    // フェーズ進行
    // ------------------------------------------------------------

    void ProcessPhase(int count)
    {
        switch (count)
        {
            case 1: AdvanceStage("「……おきろ」",       propsObjects);    break;
            case 2: AdvanceStage("「……おきろ！」",     pillarObjects);   break;
            case 3: AdvanceStage("「おきろ！！！！」", wallRoofObjects); break;
            case 4: StartCoroutine(FinalPhaseSequence()); break;
        }
    }

    /// <summary>セリフを出し、対象を消して、少し後にチャットを開き直す</summary>
    void AdvanceStage(string subtitle, GameObject[] objectsToHide)
    {
        SubtitleManager.Show(subtitle, 3.0f);
        HideObjects(objectsToHide);
        StartCoroutine(ReopenChatAfterDelay(reopenDelay));
    }

    IEnumerator ReopenChatAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!isClearing)
        {
            OpenChat();
        }
    }

    void HideObjects(GameObject[] targets)
    {
        if (targets == null) return;

        foreach (GameObject obj in targets)
        {
            if (obj != null) obj.SetActive(false);
        }
    }

    // ------------------------------------------------------------
    // 最終フェーズ（床が消えて落下 → ホワイトアウト → END3）
    // ------------------------------------------------------------

    IEnumerator FinalPhaseSequence()
    {
        isClearing = true;
        isChatOpen = false;

        if (chatInputField != null)
        {
            chatInputField.gameObject.SetActive(false);
        }

        HideObjects(floorObjects);

        Transform targetTransform = GetFallTarget();
        Transform cameraTransform = mouseLook != null ? mouseLook.transform : null;

        SubtitleManager.Show("「おきろ！！！！！！！！！」", 4.0f);

        float fallSpeed = InitialFallSpeed;

        // 落下のみ
        float timer = 0f;
        while (timer < FallDuration)
        {
            timer += Time.deltaTime;
            fallSpeed = StepFall(fallSpeed, targetTransform, cameraTransform);
            yield return null;
        }

        SubtitleManager.Show("「……夢から、おきるんだ。」", 4.0f);

        // 落下しながらホワイトアウト
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            SetWhiteoutAlpha(alpha);

            fallSpeed = StepFall(fallSpeed, targetTransform, cameraTransform);
            yield return null;
        }

        yield return new WaitForSeconds(1.0f);

        if (END3Panel != null) END3Panel.SetActive(true);

        CursorUtil.SetLocked(false);
        SetPlayerInputBlocked(true);

        Debug.Log("【ゲームクリア】現実へ目覚めました。");
    }

    Transform GetFallTarget()
    {
        if (playerMovement != null) return playerMovement.transform;
        if (playerController != null) return playerController.transform;
        return transform;
    }

    /// <summary>1フレーム分だけ加速しながら落下させ、更新後の落下速度を返す</summary>
    float StepFall(float fallSpeed, Transform target, Transform cameraTf)
    {
        fallSpeed += FallAcceleration * Time.deltaTime;
        Vector3 fallVector = Vector3.down * fallSpeed * Time.deltaTime;

        if (target != null) target.position += fallVector;

        // カメラがプレイヤーの子でない場合のみ個別に落とす
        if (cameraTf != null && cameraTf != target && !cameraTf.IsChildOf(target))
        {
            cameraTf.position += fallVector;
        }

        return fallSpeed;
    }

    void SetWhiteoutAlpha(float alpha)
    {
        if (whiteoutImage == null) return;

        Color color = whiteoutImage.color;
        color.a = alpha;
        whiteoutImage.color = color;
    }

    public void LoadStage2()
    {
        SceneManager.LoadScene("Stage2"); // Stage2のシーン名
    }
}
