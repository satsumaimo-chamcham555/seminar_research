using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    private float xRotation = 0f;
    public bool isChatActive = false; // チャット中かどうか

    private InvisibleEnemy currentEnemy; // 現在検知中の敵の参照

    void Start()
    {
        LockCursor(true);
    }

    void Update()
    {
        // 毎フレーム視線判定を実行
        CheckEnemyInSight();

        // チャット中はカメラ操作をストップする
        if (isChatActive) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    // ★画面中央のRayで敵を見ている時だけ停止フラグを送る
    void CheckEnemyInSight()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // 20メートル以内の判定
        if (Physics.Raycast(ray, out hit, 20.0f))
        {
            InvisibleEnemy enemy = hit.collider.GetComponent<InvisibleEnemy>();
            
            if (enemy != null)
            {
                // 敵を視界に捕らえている
                currentEnemy = enemy;
                currentEnemy.SetLookedAt(true);
                return;
            }
        }

        // 視線が敵から外れた場合、停止を解除して動き出させる
        if (currentEnemy != null)
        {
            currentEnemy.SetLookedAt(false);
            currentEnemy = null;
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
