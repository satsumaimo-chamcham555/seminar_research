using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    [Header("移動設定")]
    public float speed = 6f;              // 通常速度（鏡を見ている時 / ゾーン外）
    public float slowSpeed = 3f;          // スロー速度（ゾーン内で鏡を見ていない時）
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("接地判定")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    [Header("足音設定")]
    public AudioSource footstepAudio;
    public float stepInterval = 0.5f;
    private float stepTimer = 0f;

    [Header("状態フラグ")]
    public bool isInSlowZone = false;     // ★追加：スローゾーンの中にいるか
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
        Transform checkTarget = groundCheck != null ? groundCheck : transform;
        int mask = groundMask.value != 0 ? groundMask.value : -1;
        isGrounded = Physics.CheckSphere(checkTarget.position, groundDistance, mask);

        // 着地時の初期化（空中ジャンプカウントをリセット）
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
            // 移動不能でも重力だけは計算して処理を終える
            velocity.y += gravity * Time.deltaTime;
            if (controller != null) controller.Move(velocity * Time.deltaTime);
            return;
        }

        // 2. 移動処理
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;

        float currentSpeed = (isInSlowZone && !isLookingAtMirror) ? slowSpeed : speed;

        if (controller != null)
        {
            controller.Move(move * currentSpeed * Time.deltaTime);
        }

        HandleFootsteps(move.magnitude);

        // 3. ジャンプ・空中ジャンプ＆セリフ処理
        if (Input.GetButtonDown("Jump"))
        {
            // ジャンプボタンを押したら地上・空中問わずカウントアップ
            airJumpCount++;

            // 跳躍処理
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            // 1回目の「着地前ジャンプ（＝通算2回目のジャンプ）」でセリフ
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
    }
    void HandleFootsteps(float moveMagnitude)
    {
        if (footstepAudio == null) return;

        if (isGrounded && moveMagnitude > 0.1f)
        {
            stepTimer += Time.deltaTime;

            if (stepTimer >= stepInterval)
            {
                footstepAudio.Stop();
                footstepAudio.pitch = Random.Range(0.85f, 1.15f);
                footstepAudio.Play();
                stepTimer = 0f;
            }
        }
        else
        {
            if (footstepAudio.isPlaying)
            {
                footstepAudio.Stop();
            }
            stepTimer = stepInterval;
        }
    }
}
