using System.Collections;
using UnityEngine;

public class JumpscareEnemy : MonoBehaviour
{
    [Header("コンポーネント参照")]
    public SkinnedMeshRenderer enemyMesh; 
    public Animator enemyAnimator;        
    public GameObject END2Panel;          
    public Transform headCube;            // ★追加：顔位置のCube（またはTarget）

    [Header("ジャンプスケア演出設定")]
    public string attackAnimationTrigger = "Attack"; 
    public float targetDistance = 1.2f;   // ★Face Cubeからの固定距離
    public float jumpscareDuration = 2.0f; 

    [Header("参照（未設定なら自動取得）")]
    public MouseLook mouseLook;
    public PlayerMovement playerMovement;

    private bool isTriggered = false;

    void Start()
    {
        if (enemyMesh != null) enemyMesh.enabled = false;
        if (END2Panel != null) END2Panel.SetActive(false);

        // headCube未設定の場合、子要素から自動取得を試みる
        if (headCube == null)
        {
            Transform found = transform.Find("Cube");
            if (found != null) headCube = found;
        }

        if (mouseLook == null) mouseLook = FindObjectOfType<MouseLook>();
        if (playerMovement == null) playerMovement = FindObjectOfType<PlayerMovement>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;
            StartCoroutine(GameOverSequence(other.gameObject));
        }
    }

    IEnumerator GameOverSequence(GameObject playerObj)
    {
        // 1. 物理と移動の停止
        if (playerMovement != null) playerMovement.enabled = false;
        if (mouseLook != null) mouseLook.isChatActive = true; 

        CharacterController controller = playerObj.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        Rigidbody rb = playerObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // 2. カメラ取得
        Camera mainCam = Camera.main;
        Transform camTransform = mainCam != null ? mainCam.transform : playerObj.transform;

        // 3. メッシュ表示＆アニメーション再生
        if (enemyMesh != null) enemyMesh.enabled = true;

        if (enemyAnimator != null && !string.IsNullOrEmpty(attackAnimationTrigger))
        {
            enemyAnimator.SetTrigger(attackAnimationTrigger);
        }

        // 4. headCubeが設定されている場合、固定距離・固定視線に合わせる
        if (headCube != null && camTransform != null)
        {
            // エネミー（Cube）の正面方向へ一定距離（targetDistance）離れた位置にカメラを固定配置
            camTransform.position = headCube.position + (headCube.forward * targetDistance);
            
            // カメラを常にheadCubeの絶対中心に向かせる
            camTransform.LookAt(headCube.position);
        }

        Debug.Log("【GAME OVER】Face Cube基準でジャンプスケア固定！");

        // 5. 演出終了待ち
        yield return new WaitForSeconds(jumpscareDuration);

        // 6. END2パネル表示＆マウス解放
        if (END2Panel != null)
        {
            END2Panel.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
