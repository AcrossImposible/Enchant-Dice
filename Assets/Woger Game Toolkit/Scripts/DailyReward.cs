using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DailyReward : MonoBehaviour
{
    private const string DateFormat = "yyyy-MM-dd";

    /// <summary>
    /// Проверяет, можно ли получить дневную награду.
    /// Возвращает true, если текущая дата позднее сохранённой.
    /// </summary>
    public static bool CanClaim(string LastClaimDateKey)
    {
        // Считаем, что если ключа нет, награду можно брать
        if (!PlayerPrefs.HasKey(LastClaimDateKey))
            return true;

        string lastDateString = PlayerPrefs.GetString(LastClaimDateKey);
        DateTime lastDate;
        if (DateTime.TryParseExact(lastDateString, DateFormat,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out lastDate))
        {
            // Сравниваем только даты (игнорируем время). 
            // Если сегодня > вчерашней сохранённой даты, даём награду.
            return DateTime.UtcNow.Date > lastDate.Date;
        }
        else
        {
            // Если по какой‑то причина дата сохранилась криво, считаем, что награду можно брать
            return true;
        }
    }

    public static bool CanClaim(string LastClaimDateKey, int minute)
    {
        // Считаем, что если ключа нет, награду можно брать
        if (!PlayerPrefs.HasKey(LastClaimDateKey))
            return true;

        string lastDateString = PlayerPrefs.GetString(LastClaimDateKey);
        DateTime lastDate;
        if (DateTime.TryParseExact(lastDateString, DateFormat,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out lastDate))
        {
            // Сравниваем только даты (игнорируем время). 
            // Если сегодня > вчерашней сохранённой даты, даём награду.
            return DateTime.UtcNow - lastDate.Date >= TimeSpan.FromMinutes(minute);
        }
        else
        {
            // Если по какой‑то причина дата сохранилась криво, считаем, что награду можно брать
            return true;
        }
    }

    public static void SaveLastClaimDateReward(string LastClaimDateKey)
    {
        // Сохраняем сегодняшнюю дату в формате yyyy-MM-dd
        string todayString = DateTime.UtcNow.Date.ToString(DateFormat);
        PlayerPrefs.SetString(LastClaimDateKey, todayString);
        PlayerPrefs.Save();
    }
}
