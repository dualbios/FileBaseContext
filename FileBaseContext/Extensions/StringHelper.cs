namespace System.Text;

internal static class StringHelperExtensions
{
    public static string GetValidFileName(this string input)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            if (c == '\\' || c == '/')
            {
                continue;
            }

            input = input.Replace(c, '_');
        }

        return input;
    }
}