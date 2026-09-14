using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5.0f;
    public float slowSpeed = 0.5f;

    [Header("ジャンプ・重力設定")]
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("操作可否フラグ")]
    public bool canMove = true; // ★追加：外部から移動を停止させるフラグ

    [Header("デバッグ表示")]
    public bool isInSlowZone = false;
    public bool isLookingAtMirror = false;

    // 字幕の重複表示を防ぐフラグ
    private bool hasShownSlowText = false;
    private bool hasShownMirrorText = false;
    private bool hasShownAirJumpText = false;

    private CharacterController controller;
    private Vector3 velocity;
    private int jumpCount = 0;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // ★重要：canMove が false の時は一切の操作と移動（重力適用除く）を停止する
        if (!canMove)
        {
            // 空中にいる場合は最低限の重力だけ落とす
            if (!controller.isGrounded)
            {
                velocity.y += gravity * Time.deltaTime;
                controller.Move(velocity * Time.deltaTime);
            }
            return;
        }

        // 地下にいる時は落下速度とジャンプ回数をリセット
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            jumpCount = 0;
        }

        // 移動速度の決定
        float currentSpeed = moveSpeed;

        if (isInSlowZone)
        {
            if (isLookingAtMirror)
            {
                currentSpeed = moveSpeed;

                if (!hasShownMirrorText && SubtitleManager.Instance != null)
                {
                    hasShownMirrorText = true;
                    SubtitleManager.Instance.ShowSubtitle("「……あれ？ 鏡を見てると……普通に動ける？ なんで……？」", 4.0f);
                }
            }
            else
            {
                currentSpeed = slowSpeed;
            }
        }

        // WASD移動
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // ジャンプ処理
        if (Input.GetButtonDown("Jump"))
        {
            jumpCount++;

            if (jumpCount >= 2)
            {
                if (!hasShownAirJumpText && SubtitleManager.Instance != null)
                {
                    hasShownAirJumpText = true;
                    SubtitleManager.Instance.ShowSubtitle("「……空中でも蹴れる……！？ なんだこれ……夢だから……？」", 4.0f);
                }
            }

            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 重力適用
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SlowZone"))
        {
            isInSlowZone = true;

            if (!hasShownSlowText && SubtitleManager.Instance != null)
            {
                hasShownSlowText = true;
                SubtitleManager.Instance.ShowSubtitle("「うっ……！？ 急に体が重い……歩きにくい……」", 4.0f);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SlowZone"))
        {
            isInSlowZone = false;
        }
    }
}
