using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class OpeningSequence : MonoBehaviour
{
    [Header("UI設定")]
    public Image fadeOverlay;

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
        if (postProcessVolume != null && postProcessVolume.profile != null)
        {
            postProcessVolume.profile.TryGetSettings(out dofComponent);
        }

        StartCoroutine(StartOpeningSequence());
    }

    IEnumerator StartOpeningSequence()
    {
        // 1. 開始直後は操作を禁止（canLook / canMove を false に）
        if (playerMovement != null) playerMovement.canMove = false;
        if (mouseLook != null)
        {
            mouseLook.canLook = false; // ★無効化ではなくフラグ制御
            mouseLook.LockCursor(true);
        }

        // 暗転パネルを真っ黒で表示
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

        // 4. セリフ終了後にスムーズに操作解禁（画面のズレが発生しなくなります）
        if (mouseLook != null)
        {
            mouseLook.canLook = true;
        }
        if (playerMovement != null)
        {
            playerMovement.canMove = true;
        }
    }
}
