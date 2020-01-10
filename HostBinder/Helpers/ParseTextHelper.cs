using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostBinder.Helpers
{
    public static class ParseTextHelper
    {
        public static string[] GetTextLines(string content)
        {
            return content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        }

        public static string ReplaceLine(string content, int lineNumber, string newText)
        {
            var lines = GetTextLines(content);
            lines[lineNumber] = newText;
            return string.Join("\r\n", lines);
        }
    }
}
