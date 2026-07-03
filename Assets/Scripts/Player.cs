using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    PlayerInput input;
    Rigidbody rb;
    Animator anim;

    [SerializeField] float moveSpeed;
    [SerializeField] float roteSpeed;
    [SerializeField] float jumpSpeed;
    [Header("段差")]
    [SerializeField] float stepDis;
    [SerializeField] float stepWidth;
    [SerializeField] float stepHeight;
    [SerializeField] float stepSmooth;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        input = GetComponent<PlayerInput>();
        input.camera = FindAnyObjectByType<MyCamera>().gameObject.GetComponent<Camera>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        var moveVec = input.actions["Move"].ReadValue<Vector2>();
        var camF = Vector3.Scale(input.camera.transform.forward, new Vector3(1, 0, 1)).normalized;
        var movF = camF * moveVec.y + input.camera.transform.right * moveVec.x;

        rb.AddForce(movF * moveSpeed, ForceMode.Acceleration);
        
        if(moveVec != Vector2.zero)
        {
            //回転
            rb.rotation = Quaternion.RotateTowards(
                rb.rotation,
                Quaternion.LookRotation(movF.normalized),
                360 * roteSpeed * Time.deltaTime);

            //段差
            var sideDir = Vector3.Cross(Vector3.up, movF).normalized;

            Vector3[] offsets = new Vector3[]
            {
                -sideDir * stepWidth,
                Vector3.zero,
                -sideDir * stepWidth
            };
            foreach(var offset in offsets)
            {
                var lowPos = transform.position + offset + new Vector3(0, 0.02f, 0);
                var uppPos = transform.position + offset + new Vector3(0, stepHeight, 0);
                if (Physics.Raycast(lowPos, movF.normalized, out RaycastHit hitLower, stepDis))
                {
                    // 上のRayが何も遮るものがないか判定
                    if (!Physics.Raycast(uppPos, movF.normalized, stepDis))
                    {
                        // リジッドボディの位置をスムーズに上に持ち上げる
                        // ※物理演算の衝突判定を壊さないよう、rb.positionを変更する
                        rb.position += new Vector3(0, stepSmooth * Time.deltaTime, 0);
                        break;
                    }
                }
            }

        }

        var velocityXZ = rb.linearVelocity;
        velocityXZ.y = 0;
        anim.SetFloat("MoveSpeed", velocityXZ.magnitude);

        if (input.actions["Jump"].WasPressedThisFrame())
        {
            var jumpVec = new Vector3(0, jumpSpeed, 0);
            rb.AddForce(jumpVec, ForceMode.VelocityChange);
        }

    }
}

/*
 using UnityEngine;

public class RigidbodyStepClimb : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    
    [Header("段差設定")]
    [SerializeField] private float stepHeight = 0.3f;       // 登れる段差の最大高さ
    [SerializeField] private float stepLookDistance = 0.1f; // 段差を検知する前方への距離
    [SerializeField] private float stepSmooth = 2f;         // 段差を上るスムーズさ

    private void FixedUpdate()
    {
        // プレイヤーの現在の移動入力を取得（例: 入力値から計算した進行方向ベクトル）
        Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        
        // ローカルの進行方向に変換（前方に進んでいる場合のみ処理するため）
        Vector3 worldMoveDir = transform.TransformDirection(moveDirection);

        if (worldMoveDir != Vector3.zero)
        {
            StepClimb(worldMoveDir);
        }
    }

    private void StepClimb(Vector3 moveDir)
    {
        // 1. 下のRay（足元直上から進行方向へ）
        Vector3 lowerPos = transform.position + new Vector3(0, 0.05f, 0); 
        // 2. 上のRay（登れる限界の高さから進行方向へ）
        Vector3 upperPos = transform.position + new Vector3(0, stepHeight, 0);

        // 下のRayが何かに当たっているか判定
        if (Physics.Raycast(lowerPos, moveDir, out RaycastHit hitLower, stepLookDistance))
        {
            // 上のRayが何も遮るものがないか判定
            if (!Physics.Raycast(upperPos, moveDir, stepLookDistance))
            {
                // リジッドボディの位置をスムーズに上に持ち上げる
                // ※物理演算の衝突判定を壊さないよう、rb.positionを変更する
                rb.position += new Vector3(0, stepSmooth * Time.fixedDeltaTime, 0);
            }
        }
    }
}

using UnityEngine;

public class RigidbodyStepClimb : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    
    [Header("段差設定")]
    [SerializeField] private float stepHeight = 0.3f;       // 登れる段差の最大高さ
    [SerializeField] private float stepLookDistance = 0.1f; // 段差を検知する前方への距離
    [SerializeField] private float stepSmooth = 2f;         // 段差を上るスムーズさ

    [Header("横幅設定")]
    [SerializeField] private float stepWidth = 0.25f;       // 左右に広げる幅（カプセルの半径に合わせる）

    private void FixedUpdate()
    {
        Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        Vector3 worldMoveDir = transform.TransformDirection(moveDirection);

        if (worldMoveDir != Vector3.zero)
        {
            StepClimb(worldMoveDir);
        }
    }

    private void StepClimb(Vector3 moveDir)
    {
        // 進行方向（moveDir）に対する「真横（右方向）」のベクトルを計算する
        Vector3 sideDir = Vector3.Cross(Vector3.up, moveDir).normalized;

        // 横に広げるためのオフセット値（3パターン）
        Vector3[] offsets = new Vector3[]
        {
            Vector3.zero,               // 1. 中央
            sideDir * stepWidth,        // 2. 右側
            -sideDir * stepWidth        // 3. 左側
        };

        // 3つのラインのどこか1つでも「段差」を検知したら登る
        foreach (Vector3 offset in offsets)
        {
            // オフセット（横幅）を足した発射位置
            Vector3 lowerPos = transform.position + offset + new Vector3(0, 0.05f, 0);
            Vector3 upperPos = transform.position + offset + new Vector3(0, stepHeight, 0);

            // 下のRayが当たり、かつ上のRayが空いているか
            if (Physics.Raycast(lowerPos, moveDir, out RaycastHit hitLower, stepLookDistance))
            {
                if (!Physics.Raycast(upperPos, moveDir, stepLookDistance))
                {
                    rb.position += new Vector3(0, stepSmooth * Time.fixedDeltaTime, 0);
                    break; // 1つでも検知したらループを抜けて上昇処理へ
                }
            }
        }
    }
}

using UnityEngine;

public class RigidbodyStepClimb : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    
    [Header("段差設定")]
    [SerializeField] private float stepHeight = 0.3f;       // 登れる段差の最大高さ
    [SerializeField] private float stepLookDistance = 0.15f;// 検知距離（少し長めにすると安定します）
    [SerializeField] private float stepSmooth = 4f;         // 上るスピード（止まりそうなときは少し速めが吉）

    [Header("横幅設定")]
    [SerializeField] private float stepWidth = 0.25f;       // 左右のRayの広がり幅

    private void FixedUpdate()
    {
        Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        Vector3 worldMoveDir = transform.TransformDirection(moveDirection);

        if (worldMoveDir != Vector3.zero)
        {
            // 1. 進行方向の段差をチェック
            bool climbed = CheckAndClimb(worldMoveDir);

            // 2. 進行方向で登れず、かつ平行に近い角度で引っかかっている場合、
            // キャラクターの「正面（forward）」でも段差をチェックする
            if (!climbed)
            {
                CheckAndClimb(transform.forward);
            }
        }
    }

    private bool CheckAndClimb(Vector3 checkDir)
    {
        // 判定する方向に対する「真横」のベクトルを計算
        Vector3 sideDir = Vector3.Cross(Vector3.up, checkDir).normalized;

        Vector3[] offsets = new Vector3[]
        {
            Vector3.zero,
            sideDir * stepWidth,
            -sideDir * stepWidth
        };

        foreach (Vector3 offset in offsets)
        {
            Vector3 lowerPos = transform.position + offset + new Vector3(0, 0.05f, 0);
            Vector3 upperPos = transform.position + offset + new Vector3(0, stepHeight, 0);

            // デバッグ用にSceneビューにRayを表示（緑＝下、赤＝上）
            Debug.DrawRay(lowerPos, checkDir * stepLookDistance, Color.green);
            Debug.DrawRay(upperPos, checkDir * stepLookDistance, Color.red);

            if (Physics.Raycast(lowerPos, checkDir, out RaycastHit hitLower, stepLookDistance))
            {
                if (!Physics.Raycast(upperPos, checkDir, stepLookDistance))
                {
                    // 段差を検知したら位置を持ち上げる
                    rb.position += new Vector3(0, stepSmooth * Time.fixedDeltaTime, 0);
                    return true; // 登る処理を行った
                }
            }
        }

        return false; // 段差を検知しなかった
    }
}

 
 */