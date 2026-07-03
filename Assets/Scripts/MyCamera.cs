using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class MyCamera : MonoBehaviour
{
    PlayerInput input;
    [SerializeField] Transform lookTarget;
    [SerializeField] Vector3 offset;
    [SerializeField] float2 targetDisArea;//2-5
    [SerializeField] float rotSpeed;
    [SerializeField] float2 max;
    float pitch;
    float yaw;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        input = FindAnyObjectByType<PlayerInput>();
        yaw = 90;
        pitch = 0;
    }

    // Update is called once per frame
    void Update()
    {
        var lookVector = input.actions["Look"].ReadValue<Vector2>();

        yaw += lookVector.x * rotSpeed * Time.deltaTime;
        pitch -= lookVector.y * rotSpeed * Time.deltaTime;

        pitch = Mathf.Clamp(pitch, max.x, max.y);

        var target = lookTarget.position + offset;
        var rotation = Quaternion.Euler(pitch, yaw, 0);

        var pp = Mathf.InverseLerp(max.x, max.y, pitch);
        var targetDis = Mathf.Lerp(targetDisArea.x, targetDisArea.y, pp);

        var pos = rotation * new Vector3(0, 0, -targetDis) + target;

        transform.rotation = rotation;
        transform.position = pos;
    }
}
