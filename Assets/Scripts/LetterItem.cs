using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LetterItem : MonoBehaviour
{
    [Header("文字の設定")]
    public string letterName = "お"; // Inspectorで各文字を設定 ("お", "き", "ろ")

    [Header("扉の開閉アニメーション設定")]
    [Tooltip("文字取得後に開かせる扉の Animator")]
    public Animator doorAnimator;
    [Tooltip("Animator の Trigger 名")]
    public string openTriggerName = "Open";
    public AudioSource doorAudioSource; //ドアが開く音用のAudioSource

    [Header("メタ演出用参照")]
    public GameClearManager gameClearManager;
    public GameObject chatPanel;

    [Header("取得時のイベント（任意）")]
    public UnityEvent onCollected;

    // ゲーム全体で何文字拾ったかを記録する静的変数
    public static int totalCollected = 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneResetHook()
    {
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            if (mode == LoadSceneMode.Single) totalCollected = 0;
        };
    }

    private struct Line
    {
        public string text;
        public float duration;
        public float wait;

        public Line(string text, float duration, float wait)
        {
            this.text = text;
            this.duration = duration;
            this.wait = wait;
        }
    }

    private static readonly Line[] TruthLines =
    {
        new Line("「……『お』『き』『ろ』」",          3.0f, 4.0f),
        new Line("「……おきろ。」",                    3.0f, 4.0f),
        new Line("「誰に言ってるんだ？」",            4.0f, 5.0f),
        new Line("「……俺、か？」",                    3.0f, 4.0f),
        new Line("「……」",                            2.0f, 3.0f),
        new Line("「……そうか。」",                    2.0f, 3.0f),
        new Line("「ここ、夢なのか？」",              4.0f, 5.0f),
        new Line("「……だったら、」",                  4.0f, 5.0f),
        new Line("「起きればいい。」",                5.0f, 6.0f),
        new Line("「……おきろ、オキロ、okiro…………」", 5.0f, 6.0f),
    };

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        totalCollected++;

        if (totalCollected == 1)
        {
            SubtitleManager.Show($"「……『{letterName}』？ なんでこんなところに文字が……」", 5.0f);
            OpenDoor(); // 1文字目の時も即開ける（セリフ完了後なら Invoke で遅延も可）
        }
        else if (totalCollected == 2)
        {
            SubtitleManager.Show($"「また文字だ……『{letterName}』……？」", 5.0f);
            OpenDoor(); // 2文字目の時も開ける
        }
        else
        {
            StartCoroutine(PlayTruthSequence());
        }

        Debug.Log($"【文字取得】{letterName} を獲得しました！（通算: {totalCollected}文字目）");

        HideItem();

        if (onCollected != null) onCollected.Invoke();
    }

    /// <summary>扉を開けるアニメーションを呼ぶ</summary>
    private void OpenDoor()
    {
        if (doorAnimator != null && !string.IsNullOrEmpty(openTriggerName))
        {
            doorAnimator.SetTrigger(openTriggerName);
        }
        //ドアが開く音を再生！
            if (doorAudioSource != null)
            {
                doorAudioSource.Play();
            }
    }

    private void HideItem()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        MeshRenderer mesh = GetComponent<MeshRenderer>();
        if (mesh != null) mesh.enabled = false;
    }

    private IEnumerator PlayTruthSequence()
    {
        foreach (Line line in TruthLines)
        {
            SubtitleManager.Show(line.text, line.duration);
            yield return new WaitForSeconds(line.wait);
        }

        OpenDoor(); // 3文字集まってセリフをすべて言い終わったタイミングで扉を開ける！
        OpenChatOrFallback();

        gameObject.SetActive(false);
    }

    private void OpenChatOrFallback()
    {
        if (gameClearManager == null)
        {
            gameClearManager = FindObjectOfType<GameClearManager>();
        }

        if (gameClearManager != null)
        {
            gameClearManager.OpenChat();
            return;
        }

        if (chatPanel != null) chatPanel.SetActive(true);

        MouseLook mouse = FindObjectOfType<MouseLook>();
        if (mouse != null) mouse.isChatActive = true;

        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null) player.canMove = false;

        CursorUtil.SetLocked(false);
    }
}