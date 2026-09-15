using UnityEngine;
using UnityEngine.UI;

public class PauseSettings : MonoBehaviour
{
    [Header("UIスライダー参照")]
    public Slider volumeSlider;
    public Slider brightnessSlider;
    public Slider sensitivitySlider;

    [Header("参照コンポーネント")]
    public MouseLook mouseLook;
    public Light playerCameraLight; // カメラのライトを指定

    [Header("明るさ（ライト強さ）の範囲設定")]
    public float minIntensity = 0f;    // スライダー最小時の強さ
    public float maxIntensity = 3f;    // スライダー最大時の強さ

    void Start()
    {
        // 音量スライダーの初期化
        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(SetGlobalVolume);
        }

        // 明るさスライダーの初期化
        if (brightnessSlider != null && playerCameraLight != null)
        {
            brightnessSlider.onValueChanged.AddListener(SetBrightness);
            // ★変更箇所：スライダーで設定してある Value(1.0など) をそのままライトに適用する
            SetBrightness(brightnessSlider.value);
        }

        // マウス感度スライダーの初期化
        if (sensitivitySlider != null && mouseLook != null)
        {
            sensitivitySlider.value = mouseLook.mouseSensitivity;
            sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
        }
    }

    // ① 音量調整
    public void SetGlobalVolume(float value)
    {
        AudioListener.volume = value;
    }

    // ② 画面の明るさ調整（カメラ付属ライトの強度変更）
    public void SetBrightness(float value)
    {
        if (playerCameraLight != null)
        {
            // スライダー値(0.0〜1.0)を minIntensity 〜 maxIntensity に変換して適用
            playerCameraLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, value);
        }
    }

    // ③ マウス感度調整
    public void SetSensitivity(float value)
    {
        if (mouseLook != null)
        {
            mouseLook.mouseSensitivity = value;
        }
    }
}
