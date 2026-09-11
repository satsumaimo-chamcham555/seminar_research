using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5.0f; // 基本の移動スピード
    public float slowSpeed = 0.5f; // 遅くなった時のスピード

    [Header("ジャンプ・重力設定")]
    public float jumpHeight = 1.5f; // 通常のジャンプ力
    public float gravity = -9.81f; // 重力
    public float floatForce = 3.0f; // 空中ジャンプで浮く力

    [Header("デバッグ表示")]
    public bool isInSlowZone = false; // ゾーン内にいるかどうか

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // ゾーン内にいる時は遅いスピード、外の時は通常のスピード
        float currentSpeed = isInSlowZone ? slowSpeed : moveSpeed;

        // WASD移動
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // ジャンプ・浮遊処理
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                // 地面にいる時の通常ジャンプ
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            else if (isInSlowZone)
            {
                // 遅くなる現象の最中かつ空中にいる時、連打でさらに上に浮く！
                velocity.y += floatForce;
            }
        }

        // 重力の適用
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // ゾーンに入った時
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SlowZone"))
        {
            isInSlowZone = true;
        }
    }

    // ゾーンから出た時
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SlowZone"))
        {
            isInSlowZone = false;
        }
    }
}
