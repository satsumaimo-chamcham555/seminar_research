using UnityEngine;

public class InvisibleEnemy : MonoBehaviour
{
    [Header("移動・動作設定")]
    public float speed = 3.5f;          // 敵が走るスピード
    public Transform targetPlayer;     // プレイヤーのTransform
    public AudioSource footstepAudio;  // 足音のAudioSource
    public GameObject letterRo;        // クリア時に出す「ろ」の文字

    [Header("UI参照")]
    public GameObject gameOverPanel;   // ゲームオーバーパネル

    [Header("状態フラグ")]
    public bool isChasing = false;     // 追撃中か
    public bool isLookedAt = false;    // プレイヤーに見られているか

    // ★セリフ制御用の変数
    private int lookCount = 0;          // 視線で捉えた回数
    private bool hasShownLookOff = false; // 「目を逸らした時」のセリフを出したか

    void Start()
    {
        if (letterRo != null) letterRo.SetActive(false);
        if (footstepAudio != null) footstepAudio.Stop();
    }

    void Update()
    {
        if (!isChasing) return;

        // 視線に入っている間は完全停止＆音も停止
        if (isLookedAt)
        {
            if (footstepAudio != null && footstepAudio.isPlaying)
            {
                footstepAudio.Pause();
            }
            return;
        }

        // 視線から外れたら移動＆足音再生
        if (targetPlayer != null)
        {
            if (footstepAudio != null && !footstepAudio.isPlaying)
            {
                footstepAudio.UnPause();
                if (!footstepAudio.isPlaying) footstepAudio.Play();
            }

            // プレイヤーの方向へ移動
            Vector3 targetPosition = new Vector3(targetPlayer.position.x, transform.position.y, targetPlayer.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        }
    }

    // ★カメラ（MouseLook）側から「見ている/見ていない」を切り替える関数（セリフ判定追加）
    public void SetLookedAt(bool state)
    {
        // 追撃中でない場合はセリフ判定を行わない
        if (!isChasing) return;

        // 状態が変わった時だけ判定を行う
        if (isLookedAt != state)
        {
            isLookedAt = state;

            if (isLookedAt)
            {
                // 【視線が当たった瞬間】
                lookCount++;

                if (lookCount == 1)
                {
                    ShowSubtitle("「……？ 足音が止まった……？」", 3.5f);
                }
                else if (lookCount == 2)
                {
                    ShowSubtitle("「こっちの方にいるのか……？ 見えていないが、見ている間は動かない……のか？」", 5.0f);
                }
            }
            else
            {
                // 【視線が外れた瞬間】
                if (lookCount == 1 && !hasShownLookOff)
                {
                    hasShownLookOff = true;
                    ShowSubtitle("「違う、まだいる……！ なんでさっきは一瞬止まったんだ……？」", 4.5f);
                }
            }
        }
    }

    // エリア侵入で呼び出す追撃開始関数
    public void StartChasing()
    {
        if (isChasing) return; // 既に追撃中なら無視

        isChasing = true;
        if (SubtitleManager.Instance != null)
        {
            SubtitleManager.Instance.ShowSubtitle("「……ッ！？ なんだ、この足音……こっちに向かってくる！？」", 4.0f);
        }
    }

    // ゴール到達で呼び出す撃退関数
    public void DefeatEnemy()
    {
        if (!isChasing) return; // 追撃中でなければ無視

        isChasing = false;
        isLookedAt = false;

        if (footstepAudio != null) footstepAudio.Stop();
        if (letterRo != null) letterRo.SetActive(true);

        // ★ゴールゾーン到達時のセリフ
        ShowSubtitle("「……いなくなった気がする。」", 4.0f);

        Debug.Log("【3つ目の謎クリア】敵が消滅し、『ろ』が出現！");
    }

    // プレイヤーに追いつかれたらゲームオーバー画面を表示
    private void OnTriggerEnter(Collider other)
    {
        if (isChasing && other.CompareTag("Player"))
        {
            Debug.Log("【GAME OVER】見えない敵に追いつかれた！");

            isChasing = false;

            if (footstepAudio != null)
            {
                footstepAudio.Stop();
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    void ShowSubtitle(string text, float duration)
    {
        if (SubtitleManager.Instance != null)
        {
            SubtitleManager.Instance.ShowSubtitle(text, duration);
        }
    }
}
