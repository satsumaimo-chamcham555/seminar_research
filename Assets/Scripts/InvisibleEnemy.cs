using System.Collections.Generic;
using UnityEngine;

public class InvisibleEnemy : MonoBehaviour
{
    [Header("移動・動作設定")]
    public float speed = 3.5f;         // 敵が走るスピード
    public Transform targetPlayer;     // プレイヤーのTransform
    public AudioSource footstepAudio;  // 足音のAudioSource
    public GameObject letterRo;        // クリア時に出す「ろ」の文字

    [Header("暗闇（明るさ0）での挙動")]
    [Tooltip("退却先。エネミーパズルのプレハブ内にある Cube を指定")]
    public Transform retreatPoint;
    [Tooltip("明るさ0がこの秒数続くと退却を始める")]
    public float darkDurationBeforeRetreat = 5.0f;
    [Tooltip("退却時の移動スピード")]
    public float retreatSpeed = 3.5f;

    [Header("UI参照")]
    public GameObject END2Panel;       // ゲームオーバーパネル

    [Header("状態フラグ")]
    public bool isChasing = false;     // 追撃中か（暗闇で停止・退却している間は false）
    public bool isLookedAt = false;    // プレイヤーに見られているか

    // 敵の状態
    //   Idle       : 未起動 / 撃退済み
    //   Chasing    : 追撃中
    //   DarkFreeze : 明るさ0になって停止中（darkDurationBeforeRetreat 秒続くと退却へ）
    //   Retreating : retreatPoint へ遠ざかっている最中
    private enum Phase { Idle, Chasing, DarkFreeze, Retreating }
    private Phase phase = Phase.Idle;

    private const float ArriveThreshold = 0.01f;

    // セリフ制御用
    private int lookCount = 0;
    private bool hasShownLookOff = false;

    // 暗闇・退却用
    private NoisePuzzleManager noiseManager;
    private float darkTimer = 0f;
    private float retreatStartDistance = 0f;
    private float initialFootstepVolume = 1f;

    // アクティブな敵の一覧（MouseLook が毎フレーム FindObjectsOfType しなくて済むように）
    private static readonly List<InvisibleEnemy> activeEnemies = new List<InvisibleEnemy>();
    public static IReadOnlyList<InvisibleEnemy> ActiveEnemies => activeEnemies;

    /// <summary>プレイヤーが明るさを0にしているか</summary>
    private bool IsPlayerInDark => noiseManager != null && noiseManager.IsBrightnessZero;

    void OnEnable()
    {
        activeEnemies.Add(this);
    }

    void OnDisable()
    {
        activeEnemies.Remove(this);
    }

    void Start()
    {
        if (letterRo != null) letterRo.SetActive(false);

        if (footstepAudio != null)
        {
            initialFootstepVolume = footstepAudio.volume;
            footstepAudio.Stop();
        }

        // Inspector で最初から追撃中にしている場合に備える
        if (isChasing) phase = Phase.Chasing;
    }

    void Update()
    {
        switch (phase)
        {
            case Phase.Chasing:    UpdateChasing();    break;
            case Phase.DarkFreeze: UpdateDarkFreeze(); break;
            case Phase.Retreating: UpdateRetreating(); break;
        }
    }

    void SetPhase(Phase newPhase)
    {
        phase = newPhase;
        isChasing = (newPhase == Phase.Chasing);
    }

    // ------------------------------------------------------------
    // 追撃
    // ------------------------------------------------------------

    void UpdateChasing()
    {
        // 明るさが0になったら停止
        if (IsPlayerInDark)
        {
            EnterDarkFreeze();
            return;
        }

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

    /// <summary>エリア侵入で呼び出す追撃開始</summary>
    public void StartChasing()
    {
        if (isChasing || phase != Phase.Idle) return;

        // 明るさスライダーの状態を参照するため、マネージャーを保持しておく
        noiseManager = FindObjectOfType<NoisePuzzleManager>();

        SetPhase(Phase.Chasing);

        SubtitleManager.Show("「……！ なんだ、この足音……こっちに来てる！？」", 4.0f);
    }

    // ------------------------------------------------------------
    // 暗闇（明るさ0）：停止 → 5秒続いたら退却 → 到達で消滅
    // ------------------------------------------------------------

    void EnterDarkFreeze()
    {
        SetPhase(Phase.DarkFreeze);
        darkTimer = 0f;
        isLookedAt = false;

        if (footstepAudio != null) footstepAudio.Stop();

        // 暗闇の間は接触してもゲームオーバーにならないよう当たり判定を切る
        SetColliderEnabled(false);

        SubtitleManager.Show("「……足音が止まった？」", 3.0f);
    }

    void UpdateDarkFreeze()
    {
        if (!IsPlayerInDark)
        {
            ResumeChasing();
            return;
        }

        darkTimer += Time.deltaTime;
        if (darkTimer >= darkDurationBeforeRetreat)
        {
            StartRetreat();
        }
    }

    void StartRetreat()
    {
        if (retreatPoint == null)
        {
            Debug.LogWarning("【InvisibleEnemy】retreatPoint が未設定のため、その場で消滅します。");
            ArriveAtRetreatPoint();
            return;
        }

        SetPhase(Phase.Retreating);
        retreatStartDistance = HorizontalDistanceTo(retreatPoint.position);

        // 遠ざかる足音（距離に応じて小さくなっていく）
        if (footstepAudio != null && !footstepAudio.isPlaying) footstepAudio.Play();
    }

    void UpdateRetreating()
    {
        if (!IsPlayerInDark)
        {
            ResumeChasing();
            return;
        }

        // 高さは自分のまま、水平方向だけ退却先へ
        Vector3 target = new Vector3(retreatPoint.position.x, transform.position.y, retreatPoint.position.z);
        transform.position = Vector3.MoveTowards(transform.position, target, retreatSpeed * Time.deltaTime);

        float remaining = HorizontalDistanceTo(retreatPoint.position);

        if (footstepAudio != null)
        {
            float ratio = retreatStartDistance > 0f ? Mathf.Clamp01(remaining / retreatStartDistance) : 0f;
            footstepAudio.volume = initialFootstepVolume * ratio;
        }

        if (remaining <= ArriveThreshold)
        {
            ArriveAtRetreatPoint();
        }
    }

    /// <summary>退却先に到達：停止して撃退を確定し、非アクティブ化する</summary>
    void ArriveAtRetreatPoint()
    {
        ApplyDefeat();
        gameObject.SetActive(false);
    }

    /// <summary>退却完了前に明るさが戻された：再び追いかける</summary>
    void ResumeChasing()
    {
        SetColliderEnabled(true);
        RestoreFootstepVolume();
        SetPhase(Phase.Chasing);

        SubtitleManager.Show("「……ダメだ、また来る！！」", 3.0f);
        Debug.Log("【演出キャンセル】明るさが戻されたため、敵が追跡を再開します！");
    }

    float HorizontalDistanceTo(Vector3 worldPos)
    {
        Vector3 diff = worldPos - transform.position;
        diff.y = 0f;
        return diff.magnitude;
    }

    void RestoreFootstepVolume()
    {
        if (footstepAudio != null) footstepAudio.volume = initialFootstepVolume;
    }

    void SetColliderEnabled(bool enabled)
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = enabled;
    }

    // ------------------------------------------------------------
    // 視線
    // ------------------------------------------------------------

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

    // ------------------------------------------------------------
    // 撃退（ゴール到達 / 退却完了）
    // ------------------------------------------------------------

    /// <summary>ゴール到達で呼び出す撃退処理（追撃中でなければ無視）</summary>
    public void DefeatEnemy()
    {
        if (!isChasing) return;

        ApplyDefeat();
    }

    /// <summary>撃退の共通処理：当たり判定の無効化、「ろ」の出現、クリアフラグ</summary>
    void ApplyDefeat()
    {
        SetPhase(Phase.Idle);
        isLookedAt = false;

        if (footstepAudio != null) footstepAudio.Stop();
        RestoreFootstepVolume();

        // 接触によるゲームオーバー事故を防ぐため、当たり判定を即座に無効化
        SetColliderEnabled(false);

        // JumpscareEnemy側（コライダー処理含む）も無効化
        JumpscareEnemy jumpscare = GetComponent<JumpscareEnemy>();
        if (jumpscare != null) jumpscare.enabled = false;

        // 「ろ」を表示（敵側に未設定ならマネージャー側のものを表示）
        if (letterRo != null) letterRo.SetActive(true);
        else if (noiseManager != null) noiseManager.ShowLetterRo();

        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.MarkAsCleared("Puzzle_Enemy");
        }

        SubtitleManager.Show("「……いなくなった……？」", 4.0f);

        Debug.Log("【3つ目の謎クリア】敵が消滅！");
    }

    // ------------------------------------------------------------
    // ゲームオーバー
    // ------------------------------------------------------------

    /// <summary>プレイヤーに追いつかれたらゲームオーバー</summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!isChasing || !other.CompareTag("Player")) return;

        Debug.Log("【GAME OVER】見えない敵に追いつかれた！");

        SetPhase(Phase.Idle);

        if (footstepAudio != null) footstepAudio.Stop();

        if (END2Panel != null)
        {
            END2Panel.SetActive(true);
            CursorUtil.SetLocked(false);
        }
    }
}
