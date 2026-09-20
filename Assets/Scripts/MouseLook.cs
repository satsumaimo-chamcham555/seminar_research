using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    private float xRotation = 0f;
    public bool isChatActive = false;
    public bool canLook = true;

    private InvisibleEnemy currentEnemy;
    private PlayerMovement playerMovement;

    private bool hasCommentedMirror = false;

    [Header("視認判定の設定")]
    public Camera mainCamera;          // メインカメラ（未設定なら自動取得）
    public LayerMask obstacleMask;     // 壁などの遮蔽物レイヤー（壁越しの判定を防ぐ用）
    public float maxCheckDistance = 50.0f; // 見える最大距離

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

        // カメラの自動取得
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
        // ----------------------------------------------------
        // 1. 敵（InvisibleEnemy）の画面内判定
        // ----------------------------------------------------
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

        // ----------------------------------------------------
        // 2. 鏡（Mirrorタグのオブジェクト）の画面内判定
        // ----------------------------------------------------
        bool lookingAtMirrorThisFrame = IsTagObjectInScreen("Mirror");

        if (lookingAtMirrorThisFrame)
        {
            if (playerMovement != null && playerMovement.isInSlowZone && !hasCommentedMirror)
            {
                hasCommentedMirror = true;
                if (SubtitleManager.Instance != null)
                {
                    SubtitleManager.Instance.ShowSubtitle("「……今、普通に歩けた？」", 4.5f);
                }
            }
        }

        if (playerMovement != null)
        {
            playerMovement.isLookingAtMirror = lookingAtMirrorThisFrame;
        }
    }

    // 画面内に敵がいるか調べるメソッド
    InvisibleEnemy FindEnemyInScreen()
    {
        InvisibleEnemy[] enemies = FindObjectsOfType<InvisibleEnemy>();

        foreach (var enemy in enemies)
        {
            if (IsTargetInScreen(enemy.transform.position))
            {
                return enemy;
            }
        }
        return null;
    }

    // 特定のタグを持つオブジェクトが画面内にあるか調べるメソッド
    bool IsTagObjectInScreen(string tagName)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(tagName);

        foreach (var target in targets)
        {
            if (IsTargetInScreen(target.transform.position))
            {
                return true; // 画面内に1つでも映っていればOK
            }
        }
        return false;
    }

    // 指定された座標が「画面内にあり、かつ壁で遮られていないか」判定する共通メソッド
    bool IsTargetInScreen(Vector3 targetPos)
    {
        if (mainCamera == null) return false;

        // 画面内の描画領域（Viewport座標: 0.0〜1.0）に変換
        Vector3 viewPos = mainCamera.WorldToViewportPoint(targetPos);

        // z > 0（カメラの前方）かつ x, y が画面（0〜1）の範囲内にあるか
        if (viewPos.z > 0 && viewPos.x >= 0f && viewPos.x <= 1f && viewPos.y >= 0f && viewPos.y <= 1f)
        {
            Vector3 direction = targetPos - transform.position;
            float distance = direction.magnitude;

            // 距離チェック
            if (distance <= maxCheckDistance)
            {
                // 壁などの障害物（obstacleMask）に遮られていなければ true
                if (!Physics.Raycast(transform.position, direction.normalized, distance, obstacleMask))
                {
                    return true;
                }
            }
        }
        return false;
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
