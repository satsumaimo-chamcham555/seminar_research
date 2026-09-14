using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    private float xRotation = 0f;
    public bool isChatActive = false;

    private InvisibleEnemy currentEnemy;
    private PlayerMovement playerMovement;

    // ★鏡を見た時のセリフ用フラグ
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
    }

    void Update()
    {
        if (isChatActive || !enabled)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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
            // 1. 敵判定
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

            // 2. 鏡（スローゾーン）判定
            if (hit.collider.CompareTag("Mirror"))
            {
                lookingAtMirrorThisFrame = true;

                // ★スローゾーン内で初めて鏡を見た時のセリフ演出
                if (playerMovement != null && playerMovement.isInSlowZone && !hasCommentedMirror)
                {
                    hasCommentedMirror = true;
                    if (SubtitleManager.Instance != null)
                    {
                        SubtitleManager.Instance.ShowSubtitle("「あれ……鏡を見ている間は普通に歩ける……？ なんでだ……？」", 4.5f);
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
