using System.Collections;
using Rokid.UXR.Interaction;
using UnityEngine;

public class Inspect : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private InteractableUnityEventWrapper unityEvent;

    [Header("Color Settings")]
    public Color defaultColor = new Color(0f, 0.4f, 1f);
    public Color hoverColor = new Color(0.2f, 0.8f, 1f);
    public Color pressedColor = Color.white;

    [Header("Rotation Settings")]
    public float spinSpeed = 90.0f;

    [Header("3DoF World Shift Settings")]
    [Tooltip("Drag WorldContainer here")]
    public Transform worldContainer;

    [Tooltip("Drag InspectionPoint here")]
    public Transform inspectionPoint;

    [Tooltip("Drag RKCameraRig here")]
    public Transform cameraTransform;

    public float glideDuration = 1.5f;

    [Header("UI Objects")]
    public GameObject welcomeCanvas;
    public GameObject infoCanvas;
    public GameObject nextCanvas;

    [Header("UI Timing & Animations")]
    public float infoCanvasDisplayTime = 4.0f;
    public float nextCanvasDisplayTime = 4.0f;
    public float fadeOutDuration = 0.8f;

    private bool isGliding = false;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        unityEvent = GetComponent<InteractableUnityEventWrapper>();

        transform.rotation = Quaternion.Euler(45f, 0f, 45f);

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        // Hide secondary canvases on start
        if (infoCanvas != null) infoCanvas.SetActive(false);
        if (nextCanvas != null) nextCanvas.SetActive(false);

        SetMaterialColor(defaultColor);

        if (unityEvent != null)
        {
            unityEvent.WhenHover.AddListener(() => SetMaterialColor(hoverColor));
            unityEvent.WhenUnhover.AddListener(() => SetMaterialColor(defaultColor));

            unityEvent.WhenSelect.AddListener(() =>
            {
                SetMaterialColor(pressedColor);
                StartWorldGlide();
            });

            unityEvent.WhenUnselect.AddListener(() => SetMaterialColor(hoverColor));
        }
    }

    void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }

    private void StartWorldGlide()
    {
        if (worldContainer != null && inspectionPoint != null && cameraTransform != null && !isGliding)
        {
            Vector3 offset = cameraTransform.position - inspectionPoint.position;
            Vector3 targetWorldPos = worldContainer.position + offset;

            StartCoroutine(GlideWorldAndSwapUI(targetWorldPos, glideDuration));
        }
    }

    private IEnumerator GlideWorldAndSwapUI(Vector3 targetPos, float duration)
    {
        isGliding = true;
        Vector3 startPos = worldContainer.position;
        float elapsedTime = 0f;

        // 1. Move the world smoothly to InspectionPoint
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);

            worldContainer.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        worldContainer.position = targetPos;
        isGliding = false;

        // 2. Destroy WelcomeCanvas
        if (welcomeCanvas != null)
        {
            Destroy(welcomeCanvas);
        }

        // 3. Display InfoCanvas, wait, then fade it out
        if (infoCanvas != null)
        {
            infoCanvas.SetActive(true);

            yield return new WaitForSeconds(infoCanvasDisplayTime);
            yield return StartCoroutine(FadeOutAndDestroyCanvas(infoCanvas, fadeOutDuration));
        }

        // 4. Reveal NextCanvas, wait, then fade it out
        if (nextCanvas != null)
        {
            nextCanvas.SetActive(true);

            yield return new WaitForSeconds(nextCanvasDisplayTime);
            yield return StartCoroutine(FadeOutAndDestroyCanvas(nextCanvas, fadeOutDuration));
        }

        // 5. Cleanup InspectCube after all UI sequences complete
        Destroy(gameObject);
    }

    private IEnumerator FadeOutAndDestroyCanvas(GameObject canvasObj, float duration)
    {
        CanvasGroup canvasGroup = canvasObj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = canvasObj.AddComponent<CanvasGroup>();
        }

        Vector3 originalScale = canvasObj.transform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            canvasObj.transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);
            yield return null;
        }

        Destroy(canvasObj);
    }

    private void SetMaterialColor(Color targetColor)
    {
        if (meshRenderer != null && meshRenderer.material != null)
        {
            meshRenderer.material.SetColor("_BaseColor", targetColor);
        }
    }
}