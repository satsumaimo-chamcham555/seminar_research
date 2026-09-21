using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    [Header("移動設定")]
    public float speed = 6f;              // 通常速度
    public float slowSpeed = 3f;          // スロー速度
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("接地判定")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("足音設定")]
    public AudioSource footstepAudio;
    [Tooltip("通常時の足音の間隔（秒）。0.6前後が自然です")]
    public float stepInterval = 0.6f;     // 通常歩行時の足音間隔（少し大きめに設定）
    [Tooltip("スローゾーン時の足音間隔の倍率（2.0ならテンポが半分のゆっくりになる）")]
    public float slowStepMultiplier = 2.2f; // ★スロー時のテンポ遅延倍率
    
    private float stepTimer = 0f;

    [Header("状態フラグ")]
    public bool isInSlowZone = false;     // スローゾーンの中にいるか
    public bool isLookingAtMirror = false; // 鏡を見ているか
    public bool canMove = true;            // 移動可能か

    private int airJumpCount = 0;
    private bool hasTriggeredFirstAirJump = false;
    private bool hasTriggeredSecondAirJump = false;

    Vector3 velocity;
    bool isGrounded;

    void Start()
    {
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }

        if (groundCheck == null)
        {
            groundCheck = transform;
        }

        if (footstepAudio != null)
        {
            footstepAudio.loop = false;
        }
    }

    void Update()
    {
        // 1. 接地判定
        if (controller != null)
        {
            isGrounded = controller.isGrounded;
        }
        else
        {
            Transform checkTarget = groundCheck != null ? groundCheck : transform;
            int mask = groundMask.value != 0 ? groundMask.value : -1;
            isGrounded = Physics.CheckSphere(checkTarget.position, groundDistance, mask);
        }

        // 着地時の初期化
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            airJumpCount = 0;
        }

        // 移動制限時の処理
        if (!canMove)
        {
            if (footstepAudio != null && footstepAudio.isPlaying)
            {
                footstepAudio.Stop();
            }
            stepTimer = 0f;

            velocity.y += gravity * Time.deltaTime;
            if (controller != null) controller.Move(velocity * Time.deltaTime);
            return;
        }

        // 2. 移動処理
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;

        // ★現在スロー歩行中かどうかの判別
        bool isSlowMovement = (isInSlowZone && !isLookingAtMirror);
        float currentSpeed = isSlowMovement ? slowSpeed : speed;

        if (controller != null)
        {
            controller.Move(move.normalized * currentSpeed * Time.deltaTime);
        }

        // 3. ジャンプ・空中ジャンプ＆セリフ処理
        if (Input.GetButtonDown("Jump"))
        {
            airJumpCount++;

            // 跳躍処理
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            // ジャンプした瞬間、足音を即時停止＆タイマーリセット（ジャンプ中音を出さない）
            if (footstepAudio != null) footstepAudio.Stop();
            stepTimer = 0f;

            if (SubtitleManager.Instance != null)
            {
                if (airJumpCount == 2 && !hasTriggeredFirstAirJump)
                {
                    hasTriggeredFirstAirJump = true;
                    SubtitleManager.Instance.ShowSubtitle("「……あれ？」", 2.5f);
                }
                else if (airJumpCount == 3 && !hasTriggeredSecondAirJump)
                {
                    hasTriggeredSecondAirJump = true;
                    SubtitleManager.Instance.ShowSubtitle("「……まだ、跳べる？」", 2.5f);
                }
            }
        }

        // 4. 重力処理
        velocity.y += gravity * Time.deltaTime;
        if (controller != null)
        {
            controller.Move(velocity * Time.deltaTime);
        }

        // 5. 足音処理
        bool isMovingInput = (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f);
        HandleFootsteps(isMovingInput, isSlowMovement);
    }

    // ★足音のテンポ（間隔）制御メイン処理
    void HandleFootsteps(bool isMovingInput, bool isSlowMovement)
    {
        if (footstepAudio == null) return;

        // 【完全に地面に足がついていて、かつ移動入力がある時だけ】
        if (isGrounded && isMovingInput)
        {
            // ★スロー歩行中の場合は、足音の間隔をslowStepMultiplier倍に引き伸ばす（ゆっくりにする）
            float currentInterval = isSlowMovement ? (stepInterval * slowStepMultiplier) : stepInterval;

            stepTimer += Time.deltaTime;

            if (stepTimer >= currentInterval)
            {
                stepTimer = 0f; // タイマーリセット

                footstepAudio.Stop();

                // スロー時はピッチ（再生スピード・音高）も少し下げて重々しくする
                footstepAudio.pitch = isSlowMovement ? 0.75f : Random.Range(0.95f, 1.05f);
                footstepAudio.Play();
            }
        }
        else
        {
            // 【空中・ジャンプ中・静止中】
            if (footstepAudio.isPlaying)
            {
                footstepAudio.Stop();
            }
            stepTimer = 0f; // 空中にいる間は常にタイマーを0に固定
        }
    }
}
