using UnityEngine;

public class ItemShadowManager : MonoBehaviour
{
    public GameObject shadow;           
    static public float maxOffset = 0.5f;      
    static public float speedMultiplier = 0.1f;

    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        if (shadow == null) return;

        if (transform.parent == null)
        {
            Vector3 velocity = (transform.position - lastPosition) / Time.deltaTime;
            float speed = velocity.magnitude;

            float offset = Mathf.Clamp(speed * speedMultiplier, 0f, maxOffset);

            Vector3 shadowPos = transform.position;
            shadowPos.y -= offset;
            shadow.transform.position = Vector3.Lerp(shadow.transform.position, shadowPos, 10f * Time.deltaTime);
        }

        lastPosition = transform.position;
    }
}
