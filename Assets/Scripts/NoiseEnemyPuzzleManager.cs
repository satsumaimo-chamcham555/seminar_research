using UnityEngine;

/// <summary>
/// ステージ2「ノイズ × エネミー」の謎の司令塔。シーンに1つだけ置く（工場プレハブには入れない）。
///
/// 流れ：
///   開始Cube(NoiseEnemyStartTrigger) に触れる → ノイズ上昇開始 ＋ 敵が追跡開始
///   BigTV(BigTVZone) の範囲内 → ノイズ停止（ピー音）／範囲外 → ノイズ再開
///   BigTV範囲内で、ノイズが止まっていて、かつ
///     A) 敵を lookHoldSeconds 秒見つめ続けた（敵が停止・足音なし）
///     B) 明るさ0で敵が退却し消えた
///   のどちらかを満たす → クリア。今いる工場の真ん中に『き』が出現
///
/// ※ 明るさ0の判定・ポーズ画面は、既存の NoisePuzzleManager をシーンに置いておけばそのまま使われる
///   （InvisibleEnemy が FindObjectOfType<NoisePuzzleManager>() で拾う）。
/// </summary>
public class NoiseEnemyPuzzleManager : MonoBehaviour
{
    [Header("ノイズ")]
    public AudioSource noiseAudio;
    public float maxVolume = 1.0f;
    [Tooltip("ノイズが最大に達してゲームオーバーになるまでの秒数（TVの外にいる時間だけ進む）")]
    public float fadeDuration = 45.0f;
    [Tooltip("TVの範囲に入った時にノイズの進行度をリセットするか")]
    public bool resetNoiseAtTv = true;

    [Header("敵（シーン直下に置いた InvisibleEnemy。最初は非アクティブでOK）")]
    public InvisibleEnemy enemy;
    public Transform player;
    [Tooltip("開始時、プレイヤーの背後この距離に敵を出す")]
    public float enemySpawnDistance = 25.0f;

    [Header("クリア条件")]
    [Tooltip("敵をこの秒数見つめ続けたらクリア")]
    public float lookHoldSeconds = 5.0f;
    [Tooltip("PuzzleManager に登録する名前（RandomPuzzleTrigger の子オブジェクト名と合わせる）")]
    public string puzzleName = "Puzzle_NoiseEnemy";

    [Header("ゲームオーバー・参照")]
    public GameObject END1Panel;
    public MouseLook mouseLook;
    public PlayerMovement playerMovement;

    private bool started = false;
    private bool solved = false;
    private bool isGameOver = false;

    private float noiseTimer = 0f;
    private float lookTimer = 0f;
    private bool flag5 = false;
    private bool flag15 = false;

    private BigTVZone currentZone;

    /// <summary>イベント進行中か（BigTVZone が参照）</summary>
    public bool IsRunning => started && !solved && !isGameOver;

    // 工場が破棄された場合も自動で「範囲外」扱いになる（Unity の null 判定）
    private bool InTv => currentZone != null;

    void Start()
    {
        NoisePuzzleManager stage1 = FindObjectOfType<NoisePuzzleManager>();
    if (stage1 != null)
    {
        if (noiseAudio == null) noiseAudio = stage1.noiseAudio;
        if (END1Panel == null) END1Panel = stage1.END1Panel;
    }
    if (enemy == null) enemy = FindObjectOfType<InvisibleEnemy>(true); // trueで非アクティブも探す

        if (noiseAudio != null)
        {
            noiseAudio.loop = true;
            noiseAudio.volume = 0f;
            noiseAudio.pitch = 1f;
            noiseAudio.Stop();
        }

        if (END1Panel != null) END1Panel.SetActive(false);

        if (enemy != null)
        {
            enemy.suppressRewards = true; // 『ろ』やPuzzle_Enemyのクリア登録は出さない
        }

        if (mouseLook == null) mouseLook = FindObjectOfType<MouseLook>();
        if (playerMovement == null) playerMovement = FindObjectOfType<PlayerMovement>();
    }

    void Update()
    {
        if (!IsRunning) return;

        UpdateNoise();
        CheckClear();
    }

    // ------------------------------------------------------------
    // 開始
    // ------------------------------------------------------------

    /// <summary>開始Cubeから呼ばれる（2回目以降は無視）</summary>
    public void BeginEvent()
    {
        if (started || solved) return;
        started = true;

        StartEnemy();
    }

    void StartEnemy()
    {
        if (enemy == null) return;

        Transform p = player != null ? player : (playerMovement != null ? playerMovement.transform : null);
        if (p != null)
        {
            enemy.targetPlayer = p;

            Vector3 back = -p.forward;
            back.y = 0f;
            back = back.sqrMagnitude > 0.001f ? back.normalized : Vector3.back;

            Vector3 pos = p.position + back * enemySpawnDistance;
            pos.y = enemy.transform.position.y; // 高さは敵の元のまま
            enemy.transform.position = pos;
        }

        // ★追加: 敵の当たり判定と追撃フラグを強制的にONにしてリセットする
        Collider col = enemy.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        enemy.gameObject.SetActive(true);

        // ★修正: リセット用のメソッドを呼ぶか、強制的に追撃状態にする
        enemy.ForceStartChasing();
    }

    // ------------------------------------------------------------
    // ノイズ
    // ------------------------------------------------------------

    void UpdateNoise()
    {
        if (noiseAudio == null) return;

        // TVの範囲内：ノイズ停止
        if (InTv)
        {
            if (noiseAudio.isPlaying) noiseAudio.Stop();
            if (resetNoiseAtTv)
            {
                noiseTimer = 0f;
                noiseAudio.pitch = 1f;
            }
            return;
        }

        // TVの範囲外：ノイズ再開＆上昇
        if (!noiseAudio.isPlaying) noiseAudio.Play();

        noiseTimer += Time.deltaTime;
        noiseAudio.volume = Mathf.Clamp01(noiseTimer / fadeDuration) * maxVolume;

        float pitchStart = fadeDuration * 0.66f;
        if (noiseTimer > pitchStart)
        {
            noiseAudio.pitch = 1.0f + ((noiseTimer - pitchStart) / (fadeDuration - pitchStart)) * 0.3f;
        }

        if (noiseTimer >= 5.0f && !flag5)
        {
            flag5 = true;
            SubtitleManager.Show("「……なんだこの音。」", 4.0f);
        }
        else if (noiseTimer >= 15.0f && !flag15)
        {
            flag15 = true;
            SubtitleManager.Show("「うるさい……あのテレビなら……！」", 4.0f);
        }

        if (noiseTimer >= fadeDuration)
        {
            GameOver();
        }
    }

    // ------------------------------------------------------------
    // TV（BigTVZone から呼ばれる）
    // ------------------------------------------------------------

    public void OnTvEnter(BigTVZone zone)
    {
        if (!IsRunning) return;

        currentZone = zone;
        lookTimer = 0f;
        SubtitleManager.Show("「……音が、止んだ？」", 3.0f);
    }

    public void OnTvExit(BigTVZone zone)
    {
        if (currentZone == zone) currentZone = null;
        lookTimer = 0f;

        if (IsRunning)
        {
            SubtitleManager.Show("「……！ また聞こえる……！」", 3.0f);
        }
    }

    // ------------------------------------------------------------
    // クリア判定
    // ------------------------------------------------------------

    void CheckClear()
    {
        if (!InTv || enemy == null) return; // ノイズが止まっている＝TV範囲内が前提

        // B) 明るさ0で敵が退却して消えた
        if (enemy.IsRetreatedAway)
        {
            Solve();
            return;
        }

        // A) 敵を見つめ続けて停止させている（足音なし）
        bool staring = enemy.isActiveAndEnabled && enemy.isChasing && enemy.isLookedAt;
        lookTimer = staring ? lookTimer + Time.deltaTime : 0f;

        if (lookTimer >= lookHoldSeconds)
        {
            Solve();
        }
    }

    void Solve()
    {
        solved = true;

        BigTVZone zone = currentZone;

        if (noiseAudio != null)
        {
            noiseAudio.Stop();
            noiseAudio.pitch = 1f;
        }

        if (enemy != null)
        {
            enemy.DefeatEnemy();              // 追撃中なら無力化（当たり判定OFF・足音停止）
            enemy.gameObject.SetActive(false);
        }

        if (zone != null)
        {
            zone.StopBeep();
            zone.RevealLetter(); // これで複製された「その工場の中の『き』」がアクティブになります！
        }

        if (PuzzleManager.Instance != null)
        {
            PuzzleManager.Instance.MarkAsCleared(puzzleName);
        }

        SubtitleManager.Show("「……静かだ。……あれは、文字……？」", 5.0f);
        Debug.Log("【謎クリア】ノイズ×エネミー：『き』が出現！");
    }

    // ------------------------------------------------------------
    // ゲームオーバー
    // ------------------------------------------------------------

    void GameOver()
    {
        isGameOver = true;
        Debug.Log("【GAME OVER】ノイズの限界に達しました。");

        if (noiseAudio != null) noiseAudio.Stop();

        if (END1Panel != null) END1Panel.SetActive(true);
        CursorUtil.SetLocked(false);

        if (mouseLook != null) mouseLook.isChatActive = true;
        if (playerMovement != null) playerMovement.canMove = false;
        if (enemy != null) enemy.HaltForGameOver();
    }
    /// <summary>InvisibleEnemy に捕まった時に呼ばれる</summary>
    public void OnCaughtByEnemy()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (noiseAudio != null) noiseAudio.Stop();
        if (mouseLook != null) mouseLook.isChatActive = true;
        if (playerMovement != null) playerMovement.canMove = false;
    }
}
