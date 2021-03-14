using System;
using System.Collections.Generic;

namespace doctor.Services
{
    public static class Helper
    {
        /*
         * By historical reasons, there are two old characters used
         * to denote the boundary between lines (or paragraphs, depending
         * in interpretation) for text files.
         * 
         * One is "carrige return", "\r" and another one is "line feed", "\n" (also "new line")
         */
        public static List<string> LineFormat(string line)
        {
            var result = new List<string>();

            if (!string.IsNullOrEmpty(line))
            {
                foreach (string c in line
                    .Replace("\r\n", "\r")
                    .Split(new string[] { "\r" }, StringSplitOptions.None))
                {
                    if (c.Length > 2)
                    {
                        result.Add(c.Substring(2));
                    }
                }
            }

            return result;
        }
    }
}