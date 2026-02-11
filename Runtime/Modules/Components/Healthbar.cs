using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Healthbar : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] private float visibilitySpeed = 5f;
    [SerializeField] private float healthUpdateSpeed = 15f;
    [SerializeField] private CanvasGroup cg;

    [Header("Advanced")]
    [SerializeField] private Image healthImage;
    [SerializeField] private Image healthDiffImage;
    [SerializeField] private LerpKernel kernel = LerpKernel.SmoothStep;
    [SerializeField] private float differentialDelay = 0.75f;
    [SerializeField] private float healthDiffUpdateSpeed = 25f;

    public enum LerpKernel { Linear, Quadratic, Cubic, SmoothStep, Sine }

    // Visibility
    private float targetVisibility = 1f;

    // Health Targets
    private float targetHealth = 1f;

    // Coroutines
    private Coroutine healthRoutine;
    private Coroutine healthDiffRoutine;

    public void Initialize(float initialHealth)
    {
        if(healthImage != null) healthImage.fillAmount = 1f;
        if(healthDiffImage != null) healthDiffImage.fillAmount = 1f;
        
        targetHealth = initialHealth;
    }

    void Start()
    {
        if (cg == null) cg = GetComponent<CanvasGroup>();
        if (healthImage != null) healthImage.fillAmount = targetHealth;
        if (healthDiffImage != null) healthDiffImage.fillAmount = targetHealth;

        HideInstantly();
    }

    void Update()
    {
        UpdateVisibility();
    }

    public void SetHealthPercentage(float percentage)
    {
        if(this.gameObject.activeSelf == false) return;

        percentage = Mathf.Clamp01(percentage);
        if (!Mathf.Approximately(percentage, targetHealth))
        {
            targetHealth = percentage;
            UpdateHealth();
        }
    }

    private void UpdateHealth()
    {
        // Main bar
        if (healthImage != null)
        {
            if (healthRoutine != null) StopCoroutine(healthRoutine);
            healthRoutine = StartCoroutine(LerpFill(healthImage, targetHealth, healthUpdateSpeed));
        }

        // Damage (differential) bar
        if (healthDiffImage != null)
        {
            if (healthDiffRoutine != null) StopCoroutine(healthDiffRoutine);
            healthDiffRoutine = StartCoroutine(LerpFill(healthDiffImage, targetHealth, healthDiffUpdateSpeed, differentialDelay));
        }
    }

    private IEnumerator LerpFill(Image image, float target, float speed, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);

        float start = image.fillAmount;
        float time = 0f;

        while (Mathf.Abs(image.fillAmount - target) > 0.001f)
        {
            time += Time.deltaTime * speed;
            float t = Mathf.Clamp01(time);
            image.fillAmount = Mathf.Lerp(start, target, ApplyKernel(t));
            yield return null;
        }

        image.fillAmount = target;
    }

    private float ApplyKernel(float t)
    {
        switch (kernel)
        {
            case LerpKernel.Linear: return t;
            case LerpKernel.Quadratic: return 1f - (1f - t) * (1f - t);
            case LerpKernel.Cubic: return 1f - Mathf.Pow(1f - t, 3);
            case LerpKernel.SmoothStep:
                t = 1f - t; t = t * t * (3f - 2f * t); return 1f - t;
            case LerpKernel.Sine: return Mathf.Sin(t * Mathf.PI * 0.5f);
            default: return t;
        }
    }

    private void UpdateVisibility()
    {
        if (cg == null) return;
        cg.alpha = Mathf.Lerp(cg.alpha, targetVisibility, Time.deltaTime * visibilitySpeed);
    }

    public void Show(float speed) { 
        visibilitySpeed = speed;
        if (cg != null) targetVisibility = 1f; 
    }
    public void Hide(float speed) { 
        visibilitySpeed = speed;
        if (cg != null) targetVisibility = 0f; 
    }

    public void ShowInstantly() { 
        if (cg != null) { 
            targetVisibility = 1f; 
            cg.alpha = 1f; 
        } 
    }
    public void HideInstantly() { 
        if (cg != null) { 
            targetVisibility = 0f; 
            cg.alpha = 0f; 
        } 
    }
    public bool IsVisible() { return cg != null && cg.alpha > 0.001f; }
    public float GetHealth() { return healthImage != null ? healthImage.fillAmount : 0f; }
}
