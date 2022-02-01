namespace HSReflection.Util
{
    public static class Locale
    {
        public static string Get(dynamic localeObj)
        {
            return localeObj["m_currentLocaleValue"];
        }
    }
}
