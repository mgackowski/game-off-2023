using UnityEngine;

/**
 * A simple script to make an object float up and down using a sine function.
 */
public class FloatInPlace : MonoBehaviour
{
    [SerializeField] float frequency = 1f;
    [SerializeField] float amplitude = 0.2f;

    Vector3 originalPosition;

    private void Start()
    {
        originalPosition = transform.localPosition;
    }
    void Update()
    {
        Vector3 newPosition = originalPosition;
        newPosition.y += Mathf.Sin(Time.time * frequency) * amplitude;
        transform.localPosition = newPosition;
    }
}
