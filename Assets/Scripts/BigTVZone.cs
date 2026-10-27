using UnityEngine;

/// <summary>
/// 工場プレハブの子 BigTV に付ける（Collider は Is Trigger）。
/// 範囲内：ピー音を1回鳴らし、ノイズを止める。範囲外：ノイズ再開。
/// クリア時、この工場の真ん中に置いた『き』を表示する。
/// </summary>
public class BigTVZone : MonoBehaviour
{
    [Header("ピー音（AudioSource）")]
    public AudioSource beepAudio;

    [Header("クリア報酬：この工場の真ん中に置いた『き』（プレハブ内で非アクティブにしておく）")]
    public GameObject letterKi;

    private NoiseEnemyPuzzleManager manager;
    private bool isInside = false;

    void Start()
    {
        manager = FindObjectOfType<NoiseEnemyPuzzleManager>();

        if (letterKi != null) letterKi.SetActive(false);

        // ★ Loop はオフにしておく
        if (beepAudio != null)
        {
            beepAudio.loop = false;
            beepAudio.Stop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || manager == null) return;
        if (!manager.IsRunning) return; // イベント開始前・クリア後は反応しない

        if (!isInside)
        {
            isInside = true;

            // ★ 範囲に入った最初の1回だけ音を再生
            if (beepAudio != null && beepAudio.clip != null)
            {
                beepAudio.PlayOneShot(beepAudio.clip);
            }

            manager.OnTvEnter(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        LeaveZone();
    }

    void OnDisable()
    {
        LeaveZone();
    }

    void LeaveZone()
    {
        if (!isInside) return;
        isInside = false;

        StopBeep();
        if (manager != null) manager.OnTvExit(this);
    }

    public void StopBeep()
    {
        if (beepAudio != null && beepAudio.isPlaying)
        {
            beepAudio.Stop();
        }
    }

    public void RevealLetter()
    {
        if (letterKi != null)
        {
            letterKi.SetActive(true);
        }
        else
        {
            Debug.LogWarning("【BigTVZone】letterKi が未設定です。工場の真ん中に置いた『き』を指定してください。");
        }
    }
}
