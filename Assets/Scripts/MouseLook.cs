using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    public bool isChatActive = false;
    public bool canLook = true;

    [Header("視認判定の設定")]
    public Camera mainCamera;              // メインカメラ（未設定なら自動取得）
    public LayerMask obstacleMask;         // 壁などの遮蔽物レイヤー（壁越し判定を防ぐ用）
    public float maxCheckDistance = 50.0f; // 見える最大距離

    private float xRotation = 0f;
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

        if (mainCamera == null)
        {
            mainCamera = GetComponent<Camera>();
            if (mainCamera == null) mainCamera = Camera.main;
        }

        // 初期回転値の保持
        xRotation = transform.localEulerAngles.x;
        if (xRotation > 180f) xRotation -= 360f;
    }

    void Update()
    {
        if (isChatActive || !canLook) return;

        UpdateEnemyLook();
        UpdateMirrorLook();
        RotateView();
    }

    void RotateView()
    {
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

    // ------------------------------------------------------------
    // 視認判定
    // ------------------------------------------------------------

    /// <summary>敵（InvisibleEnemy）が視界に入っているかを敵側に伝える</summary>
    void UpdateEnemyLook()
    {
        InvisibleEnemy detectedEnemy = FindEnemyInScreen();

        if (detectedEnemy != null)
        {
            if (currentEnemy != detectedEnemy && currentEnemy != null)
            {
                currentEnemy.SetLookedAt(false);
            }
            currentEnemy = detectedEnemy;
            currentEnemy.SetLookedAt(true);
        }
        else if (currentEnemy != null)
        {
            currentEnemy.SetLookedAt(false);
            currentEnemy = null;
        }
    }

    /// <summary>鏡（Mirrorタグ）を見ているかをプレイヤーに伝える</summary>
    void UpdateMirrorLook()
    {
        bool isLookingAtMirror = IsTagObjectInScreen("Mirror");

        if (isLookingAtMirror
            && playerMovement != null
            && playerMovement.isInSlowZone
            && !hasCommentedMirror)
        {
            hasCommentedMirror = true;
            SubtitleManager.Show("「……今、普通に歩けた？」", 4.5f);
        }

        if (playerMovement != null)
        {
            playerMovement.isLookingAtMirror = isLookingAtMirror;
        }
    }

    InvisibleEnemy FindEnemyInScreen()
    {
        foreach (InvisibleEnemy enemy in FindObjectsOfType<InvisibleEnemy>())
        {
            if (IsTargetInScreen(enemy.transform.position))
            {
                return enemy;
            }
        }
        return null;
    }

    bool IsTagObjectInScreen(string tagName)
    {
        foreach (GameObject target in GameObject.FindGameObjectsWithTag(tagName))
        {
            if (IsTargetInScreen(target.transform.position))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>指定座標が「画面内にあり、距離内で、壁に遮られていない」か</summary>
    bool IsTargetInScreen(Vector3 targetPos)
    {
        if (mainCamera == null) return false;

        Vector3 viewPos = mainCamera.WorldToViewportPoint(targetPos);
        bool isInViewport = viewPos.z > 0
                            && viewPos.x >= 0f && viewPos.x <= 1f
                            && viewPos.y >= 0f && viewPos.y <= 1f;
        if (!isInViewport) return false;

        Vector3 direction = targetPos - transform.position;
        float distance = direction.magnitude;
        if (distance > maxCheckDistance) return false;

        return !Physics.Raycast(transform.position, direction.normalized, distance, obstacleMask);
    }

    public void LockCursor(bool doLock)
    {
        CursorUtil.SetLocked(doLock);
    }
}
