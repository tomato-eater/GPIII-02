using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] float roteSpeed;
    Rigidbody rb;

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
        if (Physics.Raycast((rb.position + transform.up * 0.5f), dir.normalized, out var hitinfo)) 
        {
            if(hitinfo.collider == playerColl)
            {
                dir.y = 0;
                forward = dir.normalized;
                rb.linearVelocity = forward * moveSpeed;
            }
        }

        //‰ñ“]
        rb.rotation = Quaternion.RotateTowards(
            rb.rotation,
            Quaternion.LookRotation(forward),
            360 * roteSpeed * Time.deltaTime);

    }
}
