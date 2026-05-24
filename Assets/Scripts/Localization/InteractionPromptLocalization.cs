public static class InteractionPromptLocalization
{
    private const string TableName = "Localization";
    private const string PromptKey = "INTERACT_PROMPT";

    public static string GetPrompt()
    {
        return GetLocalizedString(TableName, PromptKey);
    }

    public static string GetLocalizedString(string tableName, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return string.Empty;

        if (UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase == null)
            return key;

        string localized = UnityEngine.Localization.Settings.LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
        if (string.IsNullOrWhiteSpace(localized) || localized.StartsWith("No translation found for '"))
            return key;

        return localized;
    }
}
