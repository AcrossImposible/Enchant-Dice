using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Component for animating coin reward on a UI element using LeanTween for smooth tweens.
/// Attach to a UI GameObject and assign references to a TextMeshProUGUI and an Image in the inspector.
/// </summary>
public class CoinRewardAnimator : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("TextMeshProUGUI component showing the current coin count.")]
    public TMP_Text coinText;
    [Tooltip("Image component for the coin icon to pulse.")]
    public Image coinIcon;

    [Header("Pulse Settings")]
    [Tooltip("Scale multiplier for each pulse.")]
    public float pulseScale = 1.2f;
    [Tooltip("Duration of each pulse animation (up and down total).")]
    public float pulseDuration = 0.2f;
    [Tooltip("Minimum number of pulses during the animation.")]
    public int minPulses = 5;
    [Tooltip("Maximum number of pulses during the animation.")]
    public int maxPulses = 10;

    // Store the original scale of the icon
    private Vector3 originalScale;
    private Coroutine playCoroutine;

    private void Awake()
    {
        if (coinIcon != null)
            originalScale = coinIcon.rectTransform.localScale;
    }

    /// <summary>
    /// Plays the coin reward animation.
    /// Cancels any ongoing tweens on the icon and resets its scale before starting.
    /// </summary>
    /// <param name="duration">Total time of the animation in seconds.</param>
    /// <param name="newCoinValue">Target coin count to reach by the end.</param>
    /// <param name="onComplete">Callback invoked when animation finishes.</param>
    public void Play(float duration, int newCoinValue, Action onComplete)
    {
        // Stop previous routine
        if (playCoroutine != null)
            StopCoroutine(playCoroutine);

        // Cancel any LeanTween tweens on the icon
        LeanTween.cancel(coinIcon.rectTransform);

        // Reset scale immediately to avoid accumulation
        coinIcon.rectTransform.localScale = originalScale;

        playCoroutine = StartCoroutine(PlayRoutine(duration, newCoinValue, onComplete));
    }

    private IEnumerator PlayRoutine(float duration, int newCoinValue, Action onComplete)
    {
        // Parse starting coin count
        if (!int.TryParse(coinText.text, out int startCoins))
            startCoins = 0;
        int diff = newCoinValue - startCoins;

        // Randomize pulse times
        int pulses = UnityEngine.Random.Range(minPulses, maxPulses + 1);
        List<float> pulseTimes = new List<float>(pulses);
        for (int i = 0; i < pulses; i++)
            pulseTimes.Add(UnityEngine.Random.Range(0f, duration));
        pulseTimes.Sort();

        int pulseIndex = 0;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            // Update coin text
            float tNorm = Mathf.Clamp01(elapsed / duration);
            int currentValue = startCoins + Mathf.RoundToInt(diff * tNorm);
            coinText.text = currentValue.ToString();

            // Trigger a pulse when its time comes
            while (pulseIndex < pulses && elapsed >= pulseTimes[pulseIndex])
            {
                LeanTween.scale(coinIcon.rectTransform, originalScale * pulseScale, pulseDuration * 0.5f)
                    .setEase(LeanTweenType.easeOutQuad)
                    .setLoopPingPong(1)
                    .setOnComplete(() => {
                        // Make sure to reset scale exactly
                        coinIcon.rectTransform.localScale = originalScale;
                    });
                pulseIndex++;
            }

            yield return null;
        }

        // Finalize state
        coinText.text = newCoinValue.ToString();
        coinIcon.rectTransform.localScale = originalScale;
        onComplete?.Invoke();
        playCoroutine = null;
    }
}
