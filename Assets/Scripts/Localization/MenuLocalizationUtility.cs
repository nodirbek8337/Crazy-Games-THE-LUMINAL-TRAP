using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public static class MenuLocalizationUtility
{
    private const string TableName = "Localization";

    private static readonly Dictionary<string, string> EntryKeys =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "sozlamalar", "MENU_SETTINGS" },
            { "options", "MENU_SETTINGS" },
            { "davom etish", "MENU_CONTINUE" },
            { "resume", "MENU_CONTINUE" },
            { "asosiy menyu", "MENU_MAIN_MENU" },
            { "mainmenu", "MENU_MAIN_MENU" },
            { "mualliflar", "MENU_CREDITS" },
            { "credits", "MENU_CREDITS" },
            { "chiqish", "MENU_EXIT" },
            { "exit", "MENU_EXIT" },
            { "ovozni sozlash", "MENU_VOLUME" },
            { "volume", "MENU_VOLUME" },
            { "sichqoncha harakati", "MENU_MOUSE_SENSITIVITY" },
            { "mousesettings", "MENU_MOUSE_SENSITIVITY" },
            { "to'liq ekran", "MENU_FULLSCREEN" },
            { "fullscreenbutton", "MENU_FULLSCREEN" },
            { "windowedbutton", "MENU_WINDOWED" },
            { "uzbek", "MENU_LANGUAGE_UZBEK" },
            { "russian", "MENU_LANGUAGE_RUSSIAN" },
            { "english", "MENU_LANGUAGE_ENGLISH" },
            { "turkish", "MENU_LANGUAGE_TURKISH" },
            { "ortga", "MENU_BACK" },
            { "back", "MENU_BACK" },
            { "past", "MENU_BACK" },
            { "sozlash", "MENU_SETTINGS" },
            { "startgame", "MENU_START_GAME" },
            { "boshlash", "MENU_START_GAME" },
            { "start game", "MENU_START_GAME" },
            { "load", "MENU_LOADING" },
            { "qoch", "MENU_FLEE" },
            { "run", "MENU_FLEE" },
            { "flee", "MENU_FLEE" },
            { "biz bilan aloqa", "MENU_CONTACT_US" },
            { "contact us", "MENU_CONTACT_US" },
            { "bizimle iletisim", "MENU_CONTACT_US" },
            { "svyazatsya s nami", "MENU_CONTACT_US" },
            { "youtube", "MENU_YOUTUBE" },
            { "telegram", "MENU_TELEGRAM" },
        };

    public static void LocalizeCanvas(Canvas canvas)
    {
        if (canvas == null)
            return;

        LocalizeTexts(canvas.GetComponentsInChildren<TMP_Text>(true));
    }

    public static void LocalizeGameObject(GameObject root)
    {
        if (root == null)
            return;

        LocalizeTexts(root.GetComponentsInChildren<TMP_Text>(true));
    }

    public static void LocalizeTexts(IEnumerable<TMP_Text> texts)
    {
        if (texts == null)
            return;

        foreach (TMP_Text text in texts)
        {
            if (text == null)
                continue;

            string localized = ResolveLocalizedText(text);
            if (!string.IsNullOrEmpty(localized))
                text.text = localized;
        }
    }

    private static string ResolveLocalizedText(TMP_Text text)
    {
        string objectName = NormalizeKey(text.gameObject.name);
        string currentText = NormalizeKey(text.text);

        if (TryGetEntryKey(objectName, out string entryKey))
            return GetLocalizedString(entryKey);

        if (TryGetEntryKey(currentText, out entryKey))
            return GetLocalizedString(entryKey);

        return null;
    }

    private static string GetLocalizedString(string entryKey)
    {
        if (LocalizationSettings.StringDatabase != null)
        {
            string localized = LocalizationSettings.StringDatabase.GetLocalizedString(TableName, entryKey);
            if (!string.IsNullOrEmpty(localized))
                return localized;
        }

        return entryKey;
    }

    private static bool TryGetEntryKey(string key, out string entryKey) => EntryKeys.TryGetValue(key, out entryKey);

    private static string NormalizeKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        string normalized = value.Trim().ToLowerInvariant();
        normalized = normalized.Replace("`", "'");
        normalized = normalized.Replace("  ", " ");
        return normalized;
    }
}
