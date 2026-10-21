using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BossFightHUD : MonoBehaviour
{
    public static BossFightHUD Instance { get; private set; }

    [Header("UI Containers & References")]
    [SerializeField] private CanvasGroup hudCanvasGroup;
    [SerializeField] private Text bossNameText;
    [SerializeField] private Text bossTitleText;
    [SerializeField] private Text phaseIndicatorText;
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider healthGhostBar;
    [SerializeField] private Slider poiseBar;
    [SerializeField] private Text timelineTimerText;

    [Header("Screen Feedbacks")]
    [SerializeField] private Image damageFlashOverlay;
    [SerializeField] private Text phaseAlertBanner;
    [SerializeField] private Text combatLogText;

    private Coroutine flashRoutine;
    private Coroutine bannerRoutine;
    private Coroutine ghostBarRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Initialize(string bossName, string title, float maxHp, float maxPoise)
    {
        if (bossNameText) bossNameText.text = bossName;
        if (bossTitleText) bossTitleText.text = title;
        
        if (healthBar)
        {
            healthBar.maxValue = maxHp;
            healthBar.value = maxHp;
        }
        if (healthGhostBar)
        {
            healthGhostBar.maxValue = maxHp;
            healthGhostBar.value = maxHp;
        }
        if (poiseBar)
        {
            poiseBar.maxValue = maxPoise;
            poiseBar.value = maxPoise;
        }

        if (damageFlashOverlay) damageFlashOverlay.color = new Color(1, 0, 0, 0);
        if (phaseAlertBanner) phaseAlertBanner.text = "";
    }

    public void UpdateHealth(float currentHp, float maxHp)
    {
        if (healthBar)
        {
            healthBar.maxValue = maxHp;
            healthBar.value = currentHp;
        }

        if (ghostBarRoutine != null) StopCoroutine(ghostBarRoutine);
        ghostBarRoutine = StartCoroutine(SmoothGhostBar(currentHp));
    }

    public void UpdatePoise(float currentPoise, float maxPoise)
    {
        if (poiseBar)
        {
            poiseBar.maxValue = maxPoise;
            poiseBar.value = currentPoise;
        }
    }

    public void UpdateTimelineProgress(string phaseName, float elapsed, float total)
    {
        if (phaseIndicatorText)
            phaseIndicatorText.text = phaseName.ToUpper();

        if (timelineTimerText)
            timelineTimerText.text = $"{elapsed:F1}s / {total:F1}s";
    }

    public void TriggerDamageFlash(Color color, float duration = 0.25f)
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(DoDamageFlash(color, duration));
    }

    public void ShowPhaseTransitionBanner(string message, Color bannerColor)
    {
        if (bannerRoutine != null) StopCoroutine(bannerRoutine);
        bannerRoutine = StartCoroutine(DoBannerAnimation(message, bannerColor));
    }

    public void PushCombatLog(string message)
    {
        if (combatLogText)
        {
            combatLogText.text = $"• {message}\n" + combatLogText.text;
            if (combatLogText.text.Length > 250)
                combatLogText.text = combatLogText.text.Substring(0, 250);
        }
    }

    private IEnumerator SmoothGhostBar(float targetVal)
    {
        yield return new WaitForSeconds(0.4f);
        if (healthGhostBar)
        {
            while (Mathf.Abs(healthGhostBar.value - targetVal) > 0.5f)
            {
                healthGhostBar.value = Mathf.Lerp(healthGhostBar.value, targetVal, Time.deltaTime * 6f);
                yield return null;
            }
            healthGhostBar.value = targetVal;
        }
    }

    private IEnumerator DoDamageFlash(Color col, float duration)
    {
        if (damageFlashOverlay == null) yield break;

        float half = duration * 0.5f;
        float elapsed = 0f;

        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            damageFlashOverlay.color = Color.Lerp(new Color(col.r, col.g, col.b, 0), col, elapsed / half);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.deltaTime;
            damageFlashOverlay.color = Color.Lerp(col, new Color(col.r, col.g, col.b, 0), elapsed / half);
            yield return null;
        }

        damageFlashOverlay.color = new Color(col.r, col.g, col.b, 0);
    }

    private IEnumerator DoBannerAnimation(string message, Color color)
    {
        if (phaseAlertBanner == null) yield break;

        phaseAlertBanner.text = message;
        phaseAlertBanner.color = color;
        phaseAlertBanner.transform.localScale = Vector3.one * 1.5f;

        float timer = 0f;
        while (timer < 0.3f)
        {
            timer += Time.deltaTime;
            phaseAlertBanner.transform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, timer / 0.3f);
            yield return null;
        }

        yield return new WaitForSeconds(2.0f);

        timer = 0f;
        while (timer < 0.5f)
        {
            timer += Time.deltaTime;
            var c = phaseAlertBanner.color;
            c.a = Mathf.Lerp(1f, 0f, timer / 0.5f);
            phaseAlertBanner.color = c;
            yield return null;
        }

        phaseAlertBanner.text = "";
    }
}
