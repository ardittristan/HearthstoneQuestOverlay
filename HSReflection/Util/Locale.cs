namespace HSReflection.Util
{
    public static class Locale
    {
        public static string Get(dynamic localeObj) =>
            localeObj["m_locValues"]["_size"] == 0 ? "" : localeObj["m_locValues"]["_items"][0];
    }
}
