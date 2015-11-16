namespace ScreenshotTaker
{
    public static class StringExtentions
    {
        public static string RemoveInvalidCharacters(this string e)
        {
            e = e.Replace("\\", "");
            e = e.Replace("/", "");
            e = e.Replace(":", "");
            e = e.Replace("*", "");
            e = e.Replace("?", "");
            e = e.Replace("\"", "");
            e = e.Replace("<", "");
            e = e.Replace(">", "");
            e = e.Replace("|", "");
            return e;
        }
    }
}
