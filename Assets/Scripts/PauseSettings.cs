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
    public float minIntensity = 0f; // スライダー最小時の強さ
    public float maxIntensity = 3f; // スライダー最大時の強さ

    void Start()
    {
        InitVolumeSlider();
        InitBrightnessSlider();
        InitSensitivitySlider();
    }

    void InitVolumeSlider()
    {
        if (volumeSlider == null) return;

        volumeSlider.value = AudioListener.volume;
        volumeSlider.onValueChanged.AddListener(SetGlobalVolume);
    }

    void InitBrightnessSlider()
    {
        if (brightnessSlider == null || playerCameraLight == null) return;

        brightnessSlider.onValueChanged.AddListener(SetBrightness);
        // スライダーに設定済みの Value をそのままライトに適用
        SetBrightness(brightnessSlider.value);
    }

    void InitSensitivitySlider()
    {
        if (sensitivitySlider == null || mouseLook == null) return;

        sensitivitySlider.value = mouseLook.mouseSensitivity;
        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
    }

    /// <summary>音量調整</summary>
    public void SetGlobalVolume(float value)
    {
        AudioListener.volume = value;
    }

    /// <summary>明るさ調整（スライダー値 0〜1 を min〜maxIntensity に変換）</summary>
    public void SetBrightness(float value)
    {
        if (playerCameraLight != null)
        {
            playerCameraLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, value);
        }
    }

    /// <summary>マウス感度調整</summary>
    public void SetSensitivity(float value)
    {
        if (mouseLook != null)
        {
            mouseLook.mouseSensitivity = value;
        }
    }
}
