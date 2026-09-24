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

    private DepthOfField dofComponent;
    private static bool isFirstPlay = true;

    void Start()
    {
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGetSettings(out dofComponent);
        }

        // ★2回目以降は即座にスキップして処理を終了する（一番上に移動）
        if (!isFirstPlay)
        {
            SkipOpening();
            return;
        }

        // 初回プレイ時のフラグ更新
        isFirstPlay = false;

        // --- 以下、初回のみ実行される初期化処理 ---
        if (openingAudio != null)
        {
            openingAudio.Stop();
        }

        if (playerMovement != null) playerMovement.canMove = false;
        if (mouseLook != null) mouseLook.canLook = false;

        if (fadeOverlay != null)
        {
            Color initialColor = fadeOverlay.color;
            initialColor.a = 1f;
            fadeOverlay.color = initialColor;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }

        if (tutorialCloseButton != null)
        {
            tutorialCloseButton.onClick.RemoveAllListeners();
            tutorialCloseButton.onClick.AddListener(OnStartButtonClicked);
        }
    }

    // ★ボタンが押されたら呼ばれる関数
    private void OnStartButtonClicked()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (openingAudio != null)
        {
            openingAudio.Play();
        }

        StartCoroutine(StartOpeningSequence());
    }

    IEnumerator StartOpeningSequence()
    {
        if (fadeOverlay != null)
        {
            Color initialColor = fadeOverlay.color;
            initialColor.a = 1f;
            fadeOverlay.color = initialColor;
        }

        if (dofComponent != null)
        {
            dofComponent.focusDistance.overrideState = true;
            dofComponent.focusDistance.value = 0.1f;

            dofComponent.kernelSize.overrideState = true;
            dofComponent.kernelSize.value = KernelSize.VeryLarge;
        }

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;

            if (fadeOverlay != null)
            {
                Color c = fadeOverlay.color;
                c.a = Mathf.Lerp(1f, 0f, progress);
                fadeOverlay.color = c;
            }

            if (dofComponent != null)
            {
                dofComponent.focusDistance.value = Mathf.Lerp(0.1f, 20.0f, progress);
            }

            yield return null;
        }

        if (fadeOverlay != null)
        {
            Color finalColor = fadeOverlay.color;
            finalColor.a = 0f;
            fadeOverlay.color = finalColor;
        }
        if (dofComponent != null)
        {
            dofComponent.focusDistance.value = 20.0f;
            dofComponent.kernelSize.value = KernelSize.Medium;
        }

        if (SubtitleManager.Instance != null)
        {
            SubtitleManager.Instance.ShowSubtitle(openingText, textDisplayDuration);
        }

        yield return new WaitForSeconds(textDisplayDuration);

        // スムーズに操作解禁
        EnablePlayerControl();
    }

    // ★2回目以降（リトライ時）に呼ばれるスキップ処理
    void SkipOpening()
    {
        // チュートリアルUIを非表示
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        // フェード暗転を即座に透明（解除）にする
        if (fadeOverlay != null)
        {
            Color c = fadeOverlay.color;
            c.a = 0f;
            fadeOverlay.color = c;
        }

        // 被写界深度（画面のボカシ）を通常状態に戻す
        if (dofComponent != null)
        {
            dofComponent.focusDistance.overrideState = true;
            dofComponent.focusDistance.value = 20.0f;
            dofComponent.kernelSize.value = KernelSize.Medium;
        }

        // オープニング音も流さず停止しておく（BGM等が必要な場合はここでPlay）
        if (openingAudio != null)
        {
            openingAudio.Stop();
        }

        // 即座にプレイヤー操作を可能にする
        EnablePlayerControl();

        Debug.Log("【スキップ完了】2回目以降のためオープニングをスキップしました。");
    }

    // プレイヤーの操作許可・カーソルロックを一括で行う共通処理
    private void EnablePlayerControl()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (mouseLook != null)
        {
            mouseLook.canLook = true;
            mouseLook.LockCursor(true);
        }
        if (playerMovement != null)
        {
            playerMovement.canMove = true;
        }
    }
}
