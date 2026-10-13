using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class JumpscareEnemy : MonoBehaviour
{
    [Header("コンポーネント参照")]
    public SkinnedMeshRenderer enemyMesh;
    public Animator enemyAnimator;
    public GameObject END2Panel;
    public Transform headCube;
    public AudioSource jumpscareAudioSource;
    public AudioClip jumpscareSE;

    [Header("ポストエフェクト参照")]
    public PostProcessVolume jumpscareVolume; // HierarchyのJumpscareVolumeをアタッチ

    [Header("ジャンプスケア演出設定")]
    public string attackAnimationTrigger = "Attack";
    public float targetDistance = 1.2f;
    public float jumpscareDuration = 2.0f; // じわじわ歪ませる時間（秒）

    [Header("参照（未設定なら自動取得）")]
    public MouseLook mouseLook;
    public PlayerMovement playerMovement;

    private bool isTriggered = false;

    void Start()
    {
        if (enemyMesh != null) enemyMesh.enabled = false;
        if (END2Panel != null) END2Panel.SetActive(false);

        // ゲーム開始時はエフェクトを切っておく
        if (jumpscareVolume != null) jumpscareVolume.weight = 0f;

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
        FreezePlayer(playerObj);

        Transform camTransform = GetCameraTransform(playerObj);

        // メッシュ表示＆アニメーション再生
        if (enemyMesh != null) enemyMesh.enabled = true;

        if (enemyAnimator != null && !string.IsNullOrEmpty(attackAnimationTrigger))
        {
            enemyAnimator.SetTrigger(attackAnimationTrigger);
        }

        // SE再生
        if (jumpscareAudioSource != null && jumpscareSE != null)
        {
            jumpscareAudioSource.PlayOneShot(jumpscareSE);
        }

        PlaceCameraInFrontOfHead(camTransform);

        // 時間をかけてじわじわと Weight を 0 → 1 に変化させる
        float elapsedTime = 0f;
        while (elapsedTime < jumpscareDuration)
        {
            elapsedTime += Time.deltaTime;

            if (jumpscareVolume != null)
            {
                jumpscareVolume.weight = Mathf.Lerp(0f, 1f, elapsedTime / jumpscareDuration);
            }

            // カメラの位置・視線を毎フレーム固定（揺れ防止）
            PlaceCameraInFrontOfHead(camTransform);

            yield return null;
        }

        if (jumpscareVolume != null) jumpscareVolume.weight = 1f;

        // END2パネル表示＆マウス解放
        if (END2Panel != null) END2Panel.SetActive(true);

        CursorUtil.SetLocked(false);
    }

    /// <summary>プレイヤーの操作・物理挙動を停止する</summary>
    void FreezePlayer(GameObject playerObj)
    {
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
    }

    Transform GetCameraTransform(GameObject playerObj)
    {
        Camera mainCam = Camera.main;
        return mainCam != null ? mainCam.transform : playerObj.transform;
    }

    void PlaceCameraInFrontOfHead(Transform camTransform)
    {
        if (headCube == null || camTransform == null) return;

        camTransform.position = headCube.position + (headCube.forward * targetDistance);
        camTransform.LookAt(headCube.position);
    }
}
