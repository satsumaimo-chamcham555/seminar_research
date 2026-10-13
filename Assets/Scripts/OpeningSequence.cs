using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class OpeningSequence : MonoBehaviour
{
    [Header("UI設定")]
    public Image fadeOverlay;

    [Header("操作説明UI設定")]
    public GameObject tutorialPanel;   // 操作説明パネル本体
    public Button tutorialCloseButton; // 「ゲームを開始する」ボタン

    [Header("オーディオ設定")]
    public AudioSource openingAudio;

    [Header("Post Processing設定")]
    public PostProcessVolume postProcessVolume;

    [Header("プレイヤー参照")]
    public PlayerMovement playerMovement;
    public MouseLook mouseLook;

    [Header("演出設定")]
    public float fadeDuration = 3.0f;
    public string openingText = "「……ここ、どこだ？」";
    public float textDisplayDuration = 3.5f;

    private const float BlurredFocusDistance = 0.1f;
    private const float NormalFocusDistance = 20.0f;

    private DepthOfField dofComponent;
    private static bool isFirstPlay = true; // シーンをリロードしても保持される

    void Start()
    {
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGetSettings(out dofComponent);
        }

        // 2回目以降（リトライ時）はスキップ
        if (!isFirstPlay)
        {
            SkipOpening();
            return;
        }

        isFirstPlay = false;
        PrepareFirstPlay();
    }

    // ------------------------------------------------------------
    // 初回プレイ
    // ------------------------------------------------------------

    /// <summary>初回のみ：操作を止めて暗転し、操作説明を表示する</summary>
    void PrepareFirstPlay()
    {
        if (openingAudio != null) openingAudio.Stop();

        if (playerMovement != null) playerMovement.canMove = false;
        if (mouseLook != null) mouseLook.canLook = false;

        SetFadeAlpha(1f);
        CursorUtil.SetLocked(false);

        if (tutorialPanel != null) tutorialPanel.SetActive(true);

        if (tutorialCloseButton != null)
        {
            tutorialCloseButton.onClick.RemoveAllListeners();
            tutorialCloseButton.onClick.AddListener(OnStartButtonClicked);
        }
    }

    /// <summary>「ゲームを開始する」ボタン</summary>
    private void OnStartButtonClicked()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(false);

        CursorUtil.SetLocked(true);

        if (openingAudio != null) openingAudio.Play();

        StartCoroutine(StartOpeningSequence());
    }

    IEnumerator StartOpeningSequence()
    {
        SetFadeAlpha(1f);

        if (dofComponent != null)
        {
            dofComponent.focusDistance.overrideState = true;
            dofComponent.focusDistance.value = BlurredFocusDistance;

            dofComponent.kernelSize.overrideState = true;
            dofComponent.kernelSize.value = KernelSize.VeryLarge;
        }

        // フェードイン＋ピント合わせ
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;

            SetFadeAlpha(Mathf.Lerp(1f, 0f, progress));

            if (dofComponent != null)
            {
                dofComponent.focusDistance.value = Mathf.Lerp(BlurredFocusDistance, NormalFocusDistance, progress);
            }

            yield return null;
        }

        SetFadeAlpha(0f);
        ApplyNormalDepthOfField();

        SubtitleManager.Show(openingText, textDisplayDuration);

        yield return new WaitForSeconds(textDisplayDuration);

        EnablePlayerControl();
    }

    // ------------------------------------------------------------
    // 2回目以降
    // ------------------------------------------------------------

    void SkipOpening()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(false);

        SetFadeAlpha(0f);
        ApplyNormalDepthOfField();

        // オープニング音は流さず停止（BGM等が必要ならここでPlay）
        if (openingAudio != null) openingAudio.Stop();

        EnablePlayerControl();

        Debug.Log("【スキップ完了】2回目以降のためオープニングをスキップしました。");
    }

    // ------------------------------------------------------------
    // 共通処理
    // ------------------------------------------------------------

    void SetFadeAlpha(float alpha)
    {
        if (fadeOverlay == null) return;

        Color c = fadeOverlay.color;
        c.a = alpha;
        fadeOverlay.color = c;
    }

    /// <summary>被写界深度（画面のボカシ）を通常状態にする</summary>
    void ApplyNormalDepthOfField()
    {
        if (dofComponent == null) return;

        dofComponent.focusDistance.overrideState = true;
        dofComponent.focusDistance.value = NormalFocusDistance;
        dofComponent.kernelSize.value = KernelSize.Medium;
    }

    /// <summary>プレイヤーの操作許可・カーソルロックを一括で行う</summary>
    private void EnablePlayerControl()
    {
        CursorUtil.SetLocked(true);

        if (mouseLook != null) mouseLook.canLook = true;
        if (playerMovement != null) playerMovement.canMove = true;
    }
}
