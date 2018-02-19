namespace EUTK
{
    public static class StringUtility
    {
        ///Convert camelCase to words as the name implies.
        public static string SplitCamelCase(this string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            s = s.Replace("_", " ");
            s = char.ToUpper(s[0]) + s.Substring(1);
            return System.Text.RegularExpressions.Regex.Replace(s, "(?<=[a-z])([A-Z])", " $1").Trim();
        }
    }
}
