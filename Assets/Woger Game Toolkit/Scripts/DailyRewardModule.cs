using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static module for managing multiple daily rewards with independent timers.
/// Supports two reset modes: exact interval (hours) and calendar reset (UTC midnight).
/// </summary>
public static class DailyRewardModule
{
    // Define available reset modes
    public enum ResetMode
    {
        FixedInterval,   // resets after a given TimeSpan
        DailyUtcReset    // resets at next UTC midnight
    }

    // Configuration for each reward type
    public class RewardConfig
    {
        public string Key;             // Unique identifier
        public ResetMode Mode;
        public TimeSpan Interval;      // Used if Mode == FixedInterval
    }

    // Internal storage of configs by key
    private static readonly Dictionary<string, RewardConfig> _configs = new Dictionary<string, RewardConfig>();

    private const string PrefsPrefix = "DailyReward_";
    private const string TimeFormat = "o"; // ISO 8601 round-trip

    /// <summary>
    /// Register a reward type with the system.
    /// Must be called before checking or claiming any reward.
    /// </summary>
    public static void RegisterReward(string key, ResetMode mode, TimeSpan interval = default)
    {
        if (_configs.ContainsKey(key))
        {
            Debug.Log($"Reward '{key}' is already registered.");
            return;
        }

        var config = new RewardConfig { Key = key, Mode = mode, Interval = interval };
        _configs[key] = config;
    }

    /// <summary>
    /// Check if the reward is available to claim.
    /// </summary>
    public static bool CanClaim(string key)
    {
        if (!_configs.TryGetValue(key, out var config))
        {
            Debug.LogError($"Reward '{key}' is not registered.");
            return false;
        }

        DateTime last;
        string prefKey = PrefsPrefix + key;
        if (PlayerPrefs.HasKey(prefKey))
        {
            if (!DateTime.TryParseExact(PlayerPrefs.GetString(prefKey), TimeFormat,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind,
                out last))
            {
                last = DateTime.MinValue;
            }
        }
        else
        {
            last = DateTime.MinValue;
        }

        DateTime next = GetNextResetTime(last, config);
        return DateTime.UtcNow >= next;
    }

    /// <summary>
    /// Claim the reward: if available, execute callback and reset timer.
    /// </summary>
    public static bool Claim(string key, Action onClaimed)
    {
        if (!CanClaim(key)) return false;

        // Perform the reward action
        onClaimed?.Invoke();

        // Save the new claim time
        string prefKey = PrefsPrefix + key;
        string now = DateTime.UtcNow.ToString(TimeFormat);
        PlayerPrefs.SetString(prefKey, now);
        PlayerPrefs.Save();
        return true;
    }

    /// <summary>
    /// Get remaining time until next claim (zero if available now).
    /// </summary>
    public static TimeSpan GetTimeRemaining(string key)
    {
        if (!_configs.TryGetValue(key, out var config))
        {
            Debug.LogError($"Reward '{key}' is not registered.");
            return TimeSpan.Zero;
        }

        DateTime last;
        string prefKey = PrefsPrefix + key;
        if (PlayerPrefs.HasKey(prefKey))
        {
            if (!DateTime.TryParseExact(PlayerPrefs.GetString(prefKey), TimeFormat,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind,
                out last))
            {
                last = DateTime.MinValue;
            }
        }
        else
        {
            last = DateTime.MinValue;
        }

        DateTime next = GetNextResetTime(last, config);
        var remaining = next - DateTime.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    /// <summary>
    /// Compute the next reset time based on the config and last claim.
    /// </summary>
    private static DateTime GetNextResetTime(DateTime lastClaim, RewardConfig config)
    {
        switch (config.Mode)
        {
            case ResetMode.FixedInterval:
                return lastClaim.Add(config.Interval);

            case ResetMode.DailyUtcReset:
                var now = DateTime.UtcNow;
                var nextMidnight = now.Date.AddDays(1);
                return nextMidnight;

            default:
                return DateTime.MinValue;
        }
    }
}

// ================== Example Usage ==================
// In some initialization script or before use:
// DailyRewardModule.RegisterReward("daily_coins", DailyRewardModule.ResetMode.DailyUtcReset);
// DailyRewardModule.RegisterReward("special_interval", DailyRewardModule.ResetMode.FixedInterval, TimeSpan.FromHours(6));

// Checking availability:
// bool canGetCoins = DailyRewardModule.CanClaim("daily_coins");

// Claiming:
// bool success = DailyRewardModule.Claim("daily_coins", () => GiveCoins(100));

// Getting remaining time:
// TimeSpan remaining = DailyRewardModule.GetTimeRemaining("daily_coins");
// timerText.text = remaining > TimeSpan.Zero ?
//     $"{remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}" :
//     "Available!";
