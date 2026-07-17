using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] float roteSpeed;
    Rigidbody rb;
    [SerializeField] int hp = 2;
    [SerializeField] float invincibleTimeMax = 0.5f;
    float invincibleTime;
    [SerializeField] float knockBackPower = 5;
    

    public Collider playerColl {  get; set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerColl = FindAnyObjectByType<Player>().GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        var forward = transform.forward;
        var dir = playerColl.bounds.center - rb.position;
        if (invincibleTime <= 0)
        {
            if (Physics.Raycast((rb.position + transform.up * 0.5f), dir.normalized, out var hitinfo))
            {
                if (hitinfo.collider == playerColl)
                {
                    dir.y = 0;
                    forward = dir.normalized;
                    rb.linearVelocity = forward * moveSpeed;
                }
            }
        }

        //回転
        rb.rotation = Quaternion.RotateTowards(
            rb.rotation,
            Quaternion.LookRotation(forward),
            360 * roteSpeed * Time.deltaTime);


        if (0 < invincibleTime)
        {
            invincibleTime -= Time.deltaTime;
            if(invincibleTime < 0)
                invincibleTime = 0;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Enemy"))
        {
            if (collision.gameObject.TryGetComponent<AttackObj>(out var attackObj))
            {
                if (invincibleTime <= 0)
                {
                    hp -= attackObj.power;
                    if (hp <= 0)
                    {
                        gameObject.SetActive(false);
                    }
                    invincibleTime = invincibleTimeMax;

                }

                //ノックバック
                var dir = transform.position - collision.transform.position;
                dir.y = 0;
                var knockbackVec = dir.normalized * knockBackPower;
                rb.linearVelocity = knockbackVec;
            }
        }

        if (Physics.SphereCast(transform.position + new Vector3(0, 0.9f, 0), 0.5f, Vector3.down, out var hit, 0.1f)) 
        {
            Debug.Log(hit.collider.gameObject.name);
        }
        else
        {
            rb.AddForce(Vector3.down * 3f, ForceMode.Acceleration);
        }
        
    }
}
