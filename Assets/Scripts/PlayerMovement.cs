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
        Transform checkTarget = groundCheck != null ? groundCheck : transform;
        int mask = groundMask.value != 0 ? groundMask.value : -1;
        isGrounded = Physics.CheckSphere(checkTarget.position, groundDistance, mask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (!canMove)
        {
            if (footstepAudio != null && footstepAudio.isPlaying)
            {
                footstepAudio.Stop();
            }
            return;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        // ★速度決定ロジック：
        // 「スローゾーン内にいて」かつ「鏡を見ていない」時だけ減速（slowSpeed）
        // それ以外（ゾーン外、またはゾーン内で鏡を見ている時）は通常速度（speed）
        float currentSpeed = speed;
        if (isInSlowZone && !isLookingAtMirror)
        {
            currentSpeed = slowSpeed;
        }

        if (controller != null)
        {
            controller.Move(move * currentSpeed * Time.deltaTime);
        }

        HandleFootsteps(move.magnitude);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

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
