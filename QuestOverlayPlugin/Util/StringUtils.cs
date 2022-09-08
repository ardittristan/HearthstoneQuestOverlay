namespace QuestOverlayPlugin.Util;

public static class StringUtils
{
    public static string UpperFirst(string value)
    {
        string result = string.Empty;

        if (string.IsNullOrEmpty(value)) return result;
        result = value.Length == 1
            ? value.ToUpper()
            : value.Substring(0, 1).ToUpper() + value.Substring(1).ToLower();

        return result;
    }
}