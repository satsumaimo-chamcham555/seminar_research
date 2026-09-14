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
    public bool canMove = true; 

    [Header("消去対象のエリア設定")]
    public GameObject slowZoneObject; // ★InspectorでSlowZoneのゲームオブジェクトをアタッチ（未設定でも自動検知します）

    [Header("デバッグ表示")]
    public bool isInSlowZone = false;
    public bool isLookingAtMirror = false;

    // 字幕の重複表示を防ぐフラグ
    private bool hasShownSlowText = false;
    private bool hasShownMirrorText = false;
    private bool hasShownAirJumpText = false;
    private bool hasShownGoalSlowText = false;

    private CharacterController controller;
    private Vector3 velocity;
    private int jumpCount = 0;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // ★操作不能フラグが立っている場合、移動もジャンプも1ミリも処理させない
    if (!canMove || !enabled)
    {
        // 空中にいる場合の最低限の重力落下のみ適用（着地していれば何もしない）
        if (controller != null && !controller.isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
        return; // ★ここで処理を完全遮断！WASDやジャンプの処理へ行かせない
    }

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            jumpCount = 0;
        }

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

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

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
        else if (other.CompareTag("GoalSlowZone"))
        {
            isInSlowZone = false; 

            // ★追加：SlowZoneオブジェクト自体を非アクティブ化して消去
            if (slowZoneObject != null)
            {
                slowZoneObject.SetActive(false);
            }
            else
            {
                // Inspectorでアタッチしていない場合はTagから自動検索してオフにする
                GameObject foundSlowZone = GameObject.FindWithTag("SlowZone");
                if (foundSlowZone != null)
                {
                    foundSlowZone.SetActive(false);
                }
            }

            if (!hasShownGoalSlowText && SubtitleManager.Instance != null)
            {
                hasShownGoalSlowText = true;
                SubtitleManager.Instance.ShowSubtitle("「……？ 普通に歩けるようになった……？」", 4.0f);
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
