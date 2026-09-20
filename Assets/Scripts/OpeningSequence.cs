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

    void Start()
    {

        // ★1. 最初は音をしっかり止めておく
        if (openingAudio != null)
        {
            openingAudio.Stop();
        }

        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGetSettings(out dofComponent);
        }

        // 1. 最初は画面を真っ黒にしてプレイヤー操作を停止
        if (playerMovement != null) playerMovement.canMove = false;
        if (mouseLook != null)
        {
            mouseLook.canLook = false;
        }

        if (fadeOverlay != null)
        {
            Color initialColor = fadeOverlay.color;
            initialColor.a = 1f;
            fadeOverlay.color = initialColor;
        }

        // 2. カーソルを表示・ロック解除して操作説明パネルをアクティブにする
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
        }

        // 3. ★スクリプト側でボタンクリック時の処理を登録（InspectorでのOnClick設定は不要になります！）
        if (tutorialCloseButton != null)
        {
            tutorialCloseButton.onClick.RemoveAllListeners();
            tutorialCloseButton.onClick.AddListener(OnStartButtonClicked);
        }
    }

    // ★ボタンが押されたら呼ばれる関数
    private void OnStartButtonClicked()
    {
        // 操作説明パネルを閉じる
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        // カーソルを隠して画面中央にロック
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // ★ここで初めて音を鳴らす！
        if (openingAudio != null)
        {
            openingAudio.Play();
        }

        // オープニング演出を開始！
        StartCoroutine(StartOpeningSequence());
    }

    IEnumerator StartOpeningSequence()
    {
        // 暗転パネルを真っ黒で表示（念のため）
        if (fadeOverlay != null)
        {
            Color initialColor = fadeOverlay.color;
            initialColor.a = 1f;
            fadeOverlay.color = initialColor;
        }

        // 初期状態：近くにピントを合わせ、ボカシを最大化
        if (dofComponent != null)
        {
            dofComponent.focusDistance.overrideState = true;
            dofComponent.focusDistance.value = 0.1f;

            dofComponent.kernelSize.overrideState = true;
            dofComponent.kernelSize.value = KernelSize.VeryLarge;
        }

        // 2. フェードアウト＆ボカシ解除
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

        // 3. セリフ表示
        if (SubtitleManager.Instance != null)
        {
            SubtitleManager.Instance.ShowSubtitle(openingText, textDisplayDuration);
        }

        yield return new WaitForSeconds(textDisplayDuration);

        // 4. セリフ終了後にスムーズに操作解禁
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
