using UnityEngine;

public class SubtlePulse : MonoBehaviour
{
    [Header("Pulse Settings")]
    [Tooltip("How fast the object scales in and out")]
    public float pulseSpeed = 1.5f;

    [Tooltip("The size multiplier range (e.g., 0.08 means it expands up to 1.08x and contracts to 0.92x)")]
    public float pulseAmount = 0.05f;

    private Vector3 initialScale;

    void Start()
    {
        // Cache the original transform scale set in the Inspector
        initialScale = transform.localScale;
    }

    void Update()
    {
        // Calculate a smooth -1 to +1 sine wave value based on time
        float wave = Mathf.Sin(Time.time * pulseSpeed);

        // Scale factor fluctuates between (1 - pulseAmount) and (1 + pulseAmount)
        float scaleFactor = 1f + (wave * pulseAmount);

        // Apply updated scale to the object
        transform.localScale = initialScale * scaleFactor;
    }

    // Optional: Reset scale to original if component is disabled
    private void OnDisable()
    {
        transform.localScale = initialScale;
    }
}