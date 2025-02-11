using System.Text;

namespace ZoDream.Shared.Converters
{
    public static class StringConverter
    {
        public static string Studly(string? val)
        {
            if (string.IsNullOrWhiteSpace(val))
            {
                return string.Empty;
            }
            var i = 0;
            var res = new StringBuilder();
            var nextIsUpper = true;
            var diff = 'A' - 'a';
            while (i < val.Length)
            {
                var code = val[i ++];
                if (code is '_' or '-' or ' ')
                {
                    nextIsUpper = true;
                    continue;
                }
                if (nextIsUpper && code is >= 'a' and <= 'z')
                {
                    res.Append((char)(code + diff));
                }
                else
                {
                    res.Append(code);
                }
                nextIsUpper = false;
            }
            return res.ToString();
        }

        public static string UnStudly(string val)
        {
            var res = new StringBuilder();
            for (int i = 0; i < val.Length; i++)
            {
                var code = val[i];
                if (code < 65 || code > 90)
                {
                    res.Append(code);
                    continue;
                }
                if (i > 0)
                {
                    res.Append('_');
                }
                res.Append((char)(code + 32));

            }
            return res.ToString();
        }
    }
}
