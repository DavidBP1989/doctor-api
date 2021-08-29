using doctor.Models.Consults;
using System;
using System.Collections.Generic;
using System.Linq;
using static doctor.Models.Consults.General.GeneralConsult;

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
        public static List<Format> LineFormat(string line)
        {
            var result = new List<Format>
            {
                new Format
                {
                    Name = "",
                    Studies = new List<string>()
                }
            };

            var studiesList = new List<string>();

            if (!string.IsNullOrEmpty(line))
            {
                foreach (string c in line
                    .Replace("\r\n", "\r")
                    .Split(new string[] { "\r" }, StringSplitOptions.None))
                {
                    if (c.Length > 2)
                    {
                        studiesList.Add(c.Substring(2, c.Length - 3));
                    }
                }

                result[0].Studies = studiesList;
            }

            return result;
        }

        /*
         * [new]>name=x|x>name=x|x
         * 
         * si lines empieza con '[new]=' quiere decir que se trata del nuevo
         * formato, ya que la forma vieja no guarda correctamente los datos
         */
        public static List<Format> NewLineformat(string line)
        {
            var result = new List<Format>();

            if (!string.IsNullOrEmpty(line))
            {
                line = line.Substring(5);

                var list = line.Split('>');
                foreach(var l in list)
                {
                    if (l != "")
                    {
                        var studies = l.Split('=');
                        result.Add(new Format
                        {
                            Name = studies[0],
                            Studies = studies[1].Split('|').ToList()
                        });
                    }
                }
            }

            return result;
        }
    }
}