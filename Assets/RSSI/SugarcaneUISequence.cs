using System.Collections;
using Rokid.UXR.Interaction;
using UnityEngine;

public class SugarcaneUISequence : MonoBehaviour
{
    [Header("Phase 1: Insect Canvases")]
    [Tooltip("The 4 stage canvases (Egg, Crawler, Nymph, Adult)")]
    public GameObject[] insectCanvases;

    [Tooltip("How many seconds the 4 insect canvases stay visible")]
    public float insectDisplayTime = 5.0f;

    [Header("Phase 2: Control Canvas")]
    [Tooltip("Drag ControlCanvas here (disabled at start)")]
    public GameObject controlCanvas;

    [Tooltip("How many seconds ControlCanvas stays visible")]
    public float controlDisplayTime = 5.0f;

    [Header("Animation Settings")]
    [Tooltip("Duration of the fade-out/shrink animation in seconds")]
    public float fadeOutDuration = 0.8f;

    private InteractableUnityEventWrapper unityEventWrapper;
    private bool isRunningSequence = false;

    void Awake()
    {
        unityEventWrapper = GetComponent<InteractableUnityEventWrapper>();
    }

    void Start()
    {
        // Make sure all canvases are hidden initially
        SetCanvasesState(insectCanvases, false);

        if (controlCanvas != null)
        {
            controlCanvas.SetActive(false);
        }
    }

    void OnEnable()
    {
        if (unityEventWrapper != null)
        {
            unityEventWrapper.WhenSelect.AddListener(TriggerUISequence);
        }
    }

    void OnDisable()
    {
        if (unityEventWrapper != null)
        {
            unityEventWrapper.WhenSelect.RemoveListener(TriggerUISequence);
        }
    }

    public void TriggerUISequence()
    {
        if (!isRunningSequence)
        {
            StartCoroutine(RunCanvasSequence());
        }
    }

    private IEnumerator RunCanvasSequence()
    {
        isRunningSequence = true;

        // 1. Show the 4 insect canvases simultaneously
        SetCanvasesState(insectCanvases, true);

        // 2. Wait for display duration
        yield return new WaitForSeconds(insectDisplayTime);

        // 3. Fade out and destroy all 4 insect canvases
        foreach (GameObject canvasObj in insectCanvases)
        {
            if (canvasObj != null)
            {
                StartCoroutine(FadeOutAndDestroyCanvas(canvasObj, fadeOutDuration));
            }
        }

        // Wait for the fade animation to complete
        yield return new WaitForSeconds(fadeOutDuration);

        // 4. Show ControlCanvas
        if (controlCanvas != null)
        {
            controlCanvas.SetActive(true);

            // Wait for display duration
            yield return new WaitForSeconds(controlDisplayTime);

            // 5. Fade out and destroy ControlCanvas
            yield return StartCoroutine(FadeOutAndDestroyCanvas(controlCanvas, fadeOutDuration));
        }

        isRunningSequence = false;
    }

    private void SetCanvasesState(GameObject[] canvases, bool isActive)
    {
        if (canvases == null) return;

        foreach (GameObject c in canvases)
        {
            if (c != null)
            {
                c.SetActive(isActive);
            }
        }
    }

    // Smoothly shrinks and fades out a CanvasGroup before destroying it
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
}