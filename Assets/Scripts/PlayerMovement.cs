using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    [Header("移動設定")]
    public float speed = 6f;       // 通常速度
    public float slowSpeed = 3f;   // スロー速度
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("接地判定")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("足音設定")]
    public AudioSource footstepAudio;
    [Tooltip("通常時の足音の間隔（秒）。0.6前後が自然です")]
    public float stepInterval = 0.6f;
    [Tooltip("スローゾーン時の足音間隔の倍率（2.0ならテンポが半分のゆっくりになる）")]
    public float slowStepMultiplier = 2.2f;

    [Header("状態フラグ")]
    public bool isInSlowZone = false;      // スローゾーンの中にいるか
    public bool isLookingAtMirror = false; // 鏡を見ているか
    public bool canMove = true;            // 移動可能か

    private const float GroundedStickVelocity = -2f;
    private const float SlowFootstepPitch = 0.75f;

    private float stepTimer = 0f;
    private int airJumpCount = 0;
    private bool hasTriggeredFirstAirJump = false;
    private bool hasTriggeredSecondAirJump = false;

    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        if (controller == null) controller = GetComponent<CharacterController>();
        if (groundCheck == null) groundCheck = transform;
        if (footstepAudio != null) footstepAudio.loop = false;
    }

    void Update()
    {
        UpdateGroundedState();

        // 移動制限中は重力だけ適用
        if (!canMove)
        {
            StopFootstepsAndResetTimer();
            ApplyGravity();
            return;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        bool isSlowMovement = isInSlowZone && !isLookingAtMirror;

        MoveHorizontally(x, z, isSlowMovement);
        HandleJump();
        ApplyGravity();

        bool isMovingInput = Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f;
        HandleFootsteps(isMovingInput, isSlowMovement);
    }

    // ------------------------------------------------------------
    // 接地・移動・ジャンプ・重力
    // ------------------------------------------------------------

    void UpdateGroundedState()
    {
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
            velocity.y = GroundedStickVelocity;
            airJumpCount = 0;
        }
    }

    void MoveHorizontally(float x, float z, bool isSlowMovement)
    {
        if (controller == null) return;

        Vector3 move = transform.right * x + transform.forward * z;
        float currentSpeed = isSlowMovement ? slowSpeed : speed;
        controller.Move(move.normalized * currentSpeed * Time.deltaTime);
    }

    void HandleJump()
    {
        if (!Input.GetButtonDown("Jump")) return;

        airJumpCount++;

        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // ジャンプした瞬間、足音を即時停止＆タイマーリセット
        if (footstepAudio != null) footstepAudio.Stop();
        stepTimer = 0f;

        if (airJumpCount == 2 && !hasTriggeredFirstAirJump)
        {
            hasTriggeredFirstAirJump = true;
            SubtitleManager.Show("「……あれ？」", 2.5f);
        }
        else if (airJumpCount == 3 && !hasTriggeredSecondAirJump)
        {
            hasTriggeredSecondAirJump = true;
            SubtitleManager.Show("「……まだ、跳べる？」", 2.5f);
        }
    }

    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        if (controller != null)
        {
            controller.Move(velocity * Time.deltaTime);
        }
    }

    // ------------------------------------------------------------
    // 足音
    // ------------------------------------------------------------

    void StopFootstepsAndResetTimer()
    {
        if (footstepAudio != null && footstepAudio.isPlaying)
        {
            footstepAudio.Stop();
        }
        stepTimer = 0f;
    }

    void HandleFootsteps(bool isMovingInput, bool isSlowMovement)
    {
        if (footstepAudio == null) return;

        // 接地していて、かつ移動入力がある時だけ鳴らす
        if (!(isGrounded && isMovingInput))
        {
            StopFootstepsAndResetTimer();
            return;
        }

        // スロー歩行中は足音の間隔を引き伸ばす
        float currentInterval = isSlowMovement ? stepInterval * slowStepMultiplier : stepInterval;

        stepTimer += Time.deltaTime;
        if (stepTimer < currentInterval) return;

        stepTimer = 0f;

        footstepAudio.Stop();
        // スロー時はピッチも下げて重々しくする
        footstepAudio.pitch = isSlowMovement ? SlowFootstepPitch : Random.Range(0.95f, 1.05f);
        footstepAudio.Play();
    }
}
