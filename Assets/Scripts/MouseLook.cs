using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    private float xRotation = 0f;
    public bool isChatActive = false;
    public bool canLook = true; // ★追加：視線操作を許可するかどうかのフラグ

    private InvisibleEnemy currentEnemy;
    private PlayerMovement playerMovement;

    private bool hasCommentedMirror = false;

    void Start()
    {
        LockCursor(true);
        if (playerBody != null)
        {
            playerMovement = playerBody.GetComponent<PlayerMovement>();
        }
        if (playerMovement == null)
        {
            playerMovement = FindObjectOfType<PlayerMovement>();
        }

        // 初期回転値の保持
        xRotation = transform.localEulerAngles.x;
        if (xRotation > 180f) xRotation -= 360f;
    }

    void Update()
    {
        // ★修正：canLook が false の時はマウス入力を完全に無視して処理を抜ける
        if (isChatActive || !canLook)
        {
            return;
        }

        CheckRaycast();

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }

    void CheckRaycast()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        bool lookingAtMirrorThisFrame = false;

        if (Physics.Raycast(ray, out hit, 50.0f))
        {
            InvisibleEnemy enemy = hit.collider.GetComponent<InvisibleEnemy>();
            if (enemy != null)
            {
                if (currentEnemy != enemy && currentEnemy != null)
                {
                    currentEnemy.SetLookedAt(false);
                }
                currentEnemy = enemy;
                currentEnemy.SetLookedAt(true);
            }
            else if (currentEnemy != null)
            {
                currentEnemy.SetLookedAt(false);
                currentEnemy = null;
            }

            if (hit.collider.CompareTag("Mirror"))
            {
                lookingAtMirrorThisFrame = true;

                if (playerMovement != null && playerMovement.isInSlowZone && !hasCommentedMirror)
                {
                    hasCommentedMirror = true;
                    if (SubtitleManager.Instance != null)
                    {
                        SubtitleManager.Instance.ShowSubtitle("「……今、普通に歩けた？」", 4.5f);
                    }
                }
            }
        }
        else
        {
            if (currentEnemy != null)
            {
                currentEnemy.SetLookedAt(false);
                currentEnemy = null;
            }
        }

        if (playerMovement != null)
        {
            playerMovement.isLookingAtMirror = lookingAtMirrorThisFrame;
        }
    }

    public void LockCursor(bool doLock)
    {
        if (doLock)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
