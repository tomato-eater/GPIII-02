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
    [SerializeField] float gNormal;
    [SerializeField] float gDamping;
    [SerializeField] float airDamping;

    [Header("段差")]
    [SerializeField] float stepDis;
    [SerializeField] float stepWidth;
    [SerializeField] float stepHeight;
    [SerializeField] float stepSmooth;
    [SerializeField] float stepAngle;

    bool isGlounded;
    bool isGravity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        input = GetComponent<PlayerInput>();
        input.camera = FindAnyObjectByType<MyCamera>().gameObject.GetComponent<Camera>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();

        isGravity = true;
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
                -sideDir * -stepWidth
            };
            foreach(var offset in offsets)
            {
                var lowPos = transform.position + offset + new Vector3(0, 0.02f, 0);
                var uppPos = transform.position + offset + new Vector3(0, stepHeight, 0);
                if (Physics.Raycast(lowPos, movF.normalized, out RaycastHit hitLower, stepDis))
                {
                    float hitAngle = Vector3.Angle(hitLower.normal, Vector3.up);
                    if (hitAngle < stepAngle)
                        continue;

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

        if (input.actions["Jump"].WasPressedThisFrame() && isGlounded)
        {
            var jumpVec = new Vector3(0, jumpSpeed, 0);
            rb.AddForce(jumpVec, ForceMode.VelocityChange);
        }

    }

    private void FixedUpdate()
    {
        if (!isGlounded)
        {
            if (isGravity)
            {

            }
        }

        isGlounded = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach(var cont in collision.contacts)
        {
            if (cont.normal.y >= gNormal)
            {
                isGlounded = true;
            }
        }
    }
}
