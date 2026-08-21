using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    PlayerInput input;
    Rigidbody rb;
    Animator anim;
    [SerializeField] int hp;
    [SerializeField] float invincibleTimeMax;
    float invincibleTime;
    [SerializeField] float knockBackPower;

    [SerializeField] float moveSpeed;
    [SerializeField] float roteSpeed;
    [SerializeField] float jumpSpeed;
    [SerializeField] float gNormal;
    [SerializeField] float gDamping;
    [SerializeField] float airDamping;
    [SerializeField] GameObject pre_Fire;
    //List<GameObject> fire = new List<GameObject>();
    [SerializeField] Vector3 fireOffset;
    [SerializeField] float fireSpeed;

    [Header("段差")]
    [SerializeField] float stepDis;
    [SerializeField] float stepWidth;
    [SerializeField] float stepHeight;
    [SerializeField] float stepSmooth;
    [SerializeField] float stepAngle;

    bool isGrounded;
    bool isAttack;
    bool isJump;

    public ReactiveProperty<int> getCoin { get; private set; } = new(0);
    public ReactiveProperty<int> HP { get; private set; } = new(0);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        HP.Value = hp;

        //カーソルを画面内から出なくする
        Cursor.lockState = CursorLockMode.Confined;

        input = GetComponent<PlayerInput>();
        input.camera = FindAnyObjectByType<MyCamera>().gameObject.GetComponent<Camera>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();

        //for(int i=0; i < 5; i++)
        //{
        //    fire.Add(Instantiate(pre_Fire));
        //    fire.Last().SetActive(false);
        //}
        isAttack = false;
        isJump = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!rb.useGravity && !isJump) return;
        var moveVec = input.actions["Move"].ReadValue<Vector2>();
        var camF = Vector3.Scale(input.camera.transform.forward, new Vector3(1, 0, 1)).normalized;
        var movF = camF * moveVec.y + input.camera.transform.right * moveVec.x;

        if (!isAttack)
        {
            rb.AddForce(movF * moveSpeed, ForceMode.Acceleration);

            if (moveVec != Vector2.zero)
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
                sideDir * stepWidth
                };
                foreach (var offset in offsets)
                {
                    var lowPos = transform.position + offset + new Vector3(0, 0.02f, 0);
                    var uppPos = transform.position + offset + new Vector3(0, stepHeight, 0);
                    if (Physics.Raycast(lowPos, movF.normalized, out RaycastHit hitLower, stepDis))
                    {
                        float hitAngle = Vector3.Angle(hitLower.normal, Vector3.up);
                        if (hitAngle < stepAngle)
                            continue;

                        if (!Physics.Raycast(uppPos, movF.normalized, stepDis))
                        {
                            rb.position += new Vector3(0, stepSmooth * Time.deltaTime, 0);
                            isGrounded = true;
                            break;
                        }
                    }
                }
            }
        }

        var velocityXZ = rb.linearVelocity;
        velocityXZ.y = 0;
        anim.SetFloat("MoveSpeed", velocityXZ.magnitude);

        if (input.actions["Jump"].WasPressedThisFrame() && isGrounded && !isJump)
            Jump(movF).Forget();

        if (!isAttack && input.actions["Attack"].WasPressedThisFrame())
            Attack().Forget();

        if (0 < invincibleTime)
        {
            invincibleTime -= Time.deltaTime;
            if (invincibleTime <= 0) invincibleTime = 0;
        }
    }


    private void FixedUpdate()
    {
        if (!isGrounded)
        {
            if (rb.useGravity)
                rb.AddForce(Vector3.down * 1f, ForceMode.VelocityChange);
            else
                rb.AddForce(Vector3.down * 0.5f, ForceMode.VelocityChange);
        }

        isGrounded = false;
    }

    private void OnCollisionStay(Collision collision)
    {
        foreach(var cont in collision.contacts)
        {
            if (cont.normal.y >= gNormal)
            {
                isGrounded = true;
            }
        }

        if (collision.gameObject.TryGetComponent<AttackObj>(out var attackObj))
        {
            if (invincibleTime <= 0)
            {
                HP.Value -= attackObj.power;
                if (HP.Value <= 0)
                {
                    Death().Forget();
                }
                invincibleTime = invincibleTimeMax;

                //ノックバック
                var dir = transform.position - collision.transform.position;
                dir.y = 0;
                var knockbackVec = dir.normalized * knockBackPower;
                rb.AddForce(knockbackVec, ForceMode.Impulse);
            }
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Coin"))
        {
            getCoin.Value++;
            other.gameObject.SetActive(false);
        }
    }

    async UniTask Jump(Vector3 movF )
    {
        var jumpVec = new Vector3(0, jumpSpeed, 0);
        rb.AddForce(jumpVec + movF * (moveSpeed * 0.5f), ForceMode.Impulse);
        rb.useGravity = false;
        await UniTask.Delay(500, cancellationToken: this.GetCancellationTokenOnDestroy());
        rb.useGravity = true;
    }

    async UniTask Attack()
    {
        isAttack = true;
        anim.Play("Attack");
        /*
        bool set = false;
         foreach (var o in fire)
        {
            if (!o.activeSelf)
            {
                o.SetActive(true);
                Fire(o);
                set = true;
                break;
            }
        }
        if (!set)
        {
            fire.Add(Instantiate(pre_Fire));
            Fire(fire.Last());
        }
         */

        await UniTask.Delay(1000, cancellationToken: this.GetCancellationTokenOnDestroy());
        isAttack = false;
    }
    /// <summary>
    /// 炎を飛ばす
    /// </summary>
    /// <param name="fire"></param>
    /*
    void Fire(GameObject fire)
    {
        var location = transform.position + transform.TransformVector(fireOffset);
        fire.transform.position = location;

        var camF = Vector3.Scale(input.camera.transform.forward, new Vector3(1, 0, 1)).normalized;

        fire.transform.rotation = Quaternion.LookRotation(camF);
        if (fire.TryGetComponent<Rigidbody>(out var frb))
        {
            frb.linearVelocity = camF * fireSpeed;
        }
    }
    */

    async UniTask Death()
    {
        anim.Play("Death");
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        if (TryGetComponent<CapsuleCollider>(out var col))
        {
            col.enabled = false;
        }
        await UniTask.Delay(100, cancellationToken: this.GetCancellationTokenOnDestroy());
        var ui = FindAnyObjectByType<UI>();
        if(ui != null)
        {
            ui.Death();
        }
    }
}
