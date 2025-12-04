// DailyRewardModule.cs Ч исправленна€ верси€
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class DailyRewardModule
{
    public enum ResetMode { FixedInterval, DailyUtcReset }

    public class RewardConfig
    {
        public string Key;
        public ResetMode Mode;
        public TimeSpan Interval;
    }

    private static readonly Dictionary<string, RewardConfig> _configs = new Dictionary<string, RewardConfig>();
    private const string PrefsPrefix = "DailyReward_";
    private const string TimeFormat = "o";

    public static void RegisterReward(string key, ResetMode mode, TimeSpan interval = default)
    {
        // allow re-registration (update) to avoid silent misconfigurations
        var config = new RewardConfig { Key = key, Mode = mode, Interval = interval };
        _configs[key] = config;
    }

    public static bool CanClaim(string key)
    {
        if (!_configs.TryGetValue(key, out var config))
        {
            Debug.LogError($"Reward '{key}' is not registered.");
            return false;
        }

        DateTime last = ReadLastClaimTime(key);

        // If never claimed -> available
        if (last == DateTime.MinValue) return true;

        DateTime next = GetNextResetTime(last, config);
        return DateTime.UtcNow >= next;
    }

    public static bool Claim(string key, Action onClaimed)
    {
        if (!CanClaim(key)) return false;

        onClaimed?.Invoke();

        string prefKey = PrefsPrefix + key;
        string now = DateTime.UtcNow.ToString(TimeFormat, CultureInfo.InvariantCulture);
        PlayerPrefs.SetString(prefKey, now);
        PlayerPrefs.Save();
        return true;
    }

    public static TimeSpan GetTimeRemaining(string key)
    {
        if (!_configs.TryGetValue(key, out var config))
        {
            Debug.LogError($"Reward '{key}' is not registered.");
            return TimeSpan.Zero;
        }

        DateTime last = ReadLastClaimTime(key);

        // If never claimed -> zero remaining (available now)
        if (last == DateTime.MinValue) return TimeSpan.Zero;

        DateTime next = GetNextResetTime(last, config);
        var remaining = next - DateTime.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    private static DateTime ReadLastClaimTime(string key)
    {
        string prefKey = PrefsPrefix + key;
        if (!PlayerPrefs.HasKey(prefKey)) return DateTime.MinValue;

        string stored = PlayerPrefs.GetString(prefKey);
        if (DateTime.TryParseExact(stored, TimeFormat, CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind, out var last))
        {
            return last;
        }

        return DateTime.MinValue;
    }

    private static DateTime GetNextResetTime(DateTime lastClaim, RewardConfig config)
    {
        // If never claimed, treat as available (caller handles special-case), return MinValue
        if (lastClaim == DateTime.MinValue) return DateTime.MinValue;

        switch (config.Mode)
        {
            case ResetMode.FixedInterval:
                return lastClaim.Add(config.Interval);

            case ResetMode.DailyUtcReset:
                // Next reset is midnight (UTC) after the day of lastClaim.
                // Example: lastClaim 2025-10-20T15:00 -> next reset = 2025-10-21T00:00 UTC
                return lastClaim.ToUniversalTime().Date.AddDays(1);

            default:
                return DateTime.MinValue;
        }
    }
}
