using UnityEngine;
using UnityEngine.VFX;

public class Fire : MonoBehaviour
{
    [SerializeField] float lifespan;
    float span;

    private void OnEnable()
    {
        span = lifespan;
    }

    void Update()
    {
        span -= Time.deltaTime;        
        if(span < 0 )
        {
            gameObject.SetActive(false);
        }
        else if(span < 0.5f)
        {
            GetComponentInChildren<VisualEffect>().Stop();
        }
    }
}
