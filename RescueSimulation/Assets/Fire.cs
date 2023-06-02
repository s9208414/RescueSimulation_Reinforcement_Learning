using UnityEngine;

public class Fire : MonoBehaviour
{
    public float speed = 1f;
    public float amplitude = 0.5f;
    public float frequency = 1f;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = startPos + new Vector3(0f, offset, 0f) * speed;
    }
}
