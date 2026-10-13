using UnityEngine;

public class InvisibleEnemy : MonoBehaviour
{
    [Header("移動・動作設定")]
    public float speed = 3.5f;         // 敵が走るスピード
    public Transform targetPlayer;     // プレイヤーのTransform
    public AudioSource footstepAudio;  // 足音のAudioSource
    public GameObject letterRo;        // クリア時に出す「ろ」の文字

    [Header("UI参照")]
    public GameObject END2Panel;       // ゲームオーバーパネル

    [Header("状態フラグ")]
    public bool isChasing = false;     // 追撃中か
    public bool isLookedAt = false;    // プレイヤーに見られているか

    // セリフ制御用
    private int lookCount = 0;              // 視線で捉えた回数
    private bool hasShownLookOff = false;   // 「目を逸らした時」のセリフを出したか

    void Start()
    {
        if (letterRo != null) letterRo.SetActive(false);
        if (footstepAudio != null) footstepAudio.Stop();
    }

    void Update()
    {
        if (!isChasing) return;

        // 視線に入っている間は完全停止＆足音も停止
        if (isLookedAt)
        {
            if (footstepAudio != null && footstepAudio.isPlaying)
            {
                footstepAudio.Pause();
            }
            return;
        }

        if (targetPlayer == null) return;

        ResumeFootstep();
        MoveTowardsPlayer();
    }

    void ResumeFootstep()
    {
        if (footstepAudio == null || footstepAudio.isPlaying) return;

        footstepAudio.UnPause();
        if (!footstepAudio.isPlaying) footstepAudio.Play();
    }

    void MoveTowardsPlayer()
    {
        // 高さは自分のまま、水平方向だけプレイヤーへ
        Vector3 target = new Vector3(targetPlayer.position.x, transform.position.y, targetPlayer.position.z);
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
    }

    /// <summary>MouseLook側から「見ている/見ていない」を切り替える</summary>
    public void SetLookedAt(bool state)
    {
        if (!isChasing) return;
        if (isLookedAt == state) return;

        isLookedAt = state;

        if (state) OnStartedBeingLookedAt();
        else OnStoppedBeingLookedAt();
    }

    void OnStartedBeingLookedAt()
    {
        lookCount++;

        if (lookCount == 1)
        {
            SubtitleManager.Show("「……止まった？」", 3.5f);
        }
        else if (lookCount == 2)
        {
            SubtitleManager.Show("「……やっぱり。」", 5.0f);
        }
    }

    void OnStoppedBeingLookedAt()
    {
        if (lookCount == 1 && !hasShownLookOff)
        {
            hasShownLookOff = true;
            SubtitleManager.Show("「……さっき、止まったよな？」", 4.5f);
        }
    }

    /// <summary>エリア侵入で呼び出す追撃開始</summary>
    public void StartChasing()
    {
        if (isChasing) return;

        isChasing = true;

        // 追跡フラグを NoisePuzzleManager にも伝える
        NoisePuzzleManager manager = FindObjectOfType<NoisePuzzleManager>();
        if (manager != null)
        {
            manager.isBeingChased = true;
        }

        SubtitleManager.Show("「……！ なんだ、この足音……こっちに来てる！？」", 4.0f);
    }

    /// <summary>ゴール到達またはパズルクリアで呼び出す撃退処理</summary>
    public void DefeatEnemy()
    {
        if (!isChasing) return;

        isChasing = false;
        isLookedAt = false;

        if (footstepAudio != null) footstepAudio.Stop();

        // 接触によるゲームオーバー事故を防ぐため、当たり判定を即座に無効化
        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider != null) enemyCollider.enabled = false;

        // JumpscareEnemy側（コライダー処理含む）も無効化
        JumpscareEnemy jumpscare = GetComponent<JumpscareEnemy>();
        if (jumpscare != null) jumpscare.enabled = false;

        // 「ろ」の文字を出現
        if (letterRo != null) letterRo.SetActive(true);

        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.MarkAsCleared("Puzzle_Enemy");
        }

        SubtitleManager.Show("「……いなくなった……？」", 4.0f);

        Debug.Log("【3つ目の謎クリア】敵が消滅！");
    }

    /// <summary>プレイヤーに追いつかれたらゲームオーバー</summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!isChasing || !other.CompareTag("Player")) return;

        Debug.Log("【GAME OVER】見えない敵に追いつかれた！");

        isChasing = false;

        if (footstepAudio != null) footstepAudio.Stop();

        if (END2Panel != null)
        {
            END2Panel.SetActive(true);
            CursorUtil.SetLocked(false);
        }
    }
}
