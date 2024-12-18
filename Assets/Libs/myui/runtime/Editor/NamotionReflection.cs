using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Namotion
{
    public class NamotionReflection
    {
        public int value;
        public string summaryDesc;
        public string fieldName;
        public static List<NamotionReflection> GetNamotionsSimple(string path)
        {
            List<NamotionReflection> list = new List<NamotionReflection>();
            if (File.Exists(path))
            {
                string content = File.ReadAllText(path, System.Text.UTF8Encoding.UTF8);
                if (content != "")
                {
                    string[] te = content.Split('\n');
                    bool isStart = false;
                    NamotionReflection namotion = null;
                    for (int i = 0; i < te.Length; i++)
                    {
                        string line = te[i].Trim('\r').Trim();
                        if (!string.IsNullOrEmpty(line))
                        {
                            if (isStart)
                            {
                                int signIndex = line.IndexOf('=');
                                int descIndex = line.IndexOf("//");
                                if (!line.Contains("#"))
                                {
                                    if (descIndex > -1)
                                    {
                                        namotion = new NamotionReflection();
                                        namotion.summaryDesc = line.Substring(descIndex + 2).Trim();
                                    }
                                    if (namotion != null && signIndex > -1)
                                    {
                                        int.TryParse(line.Substring(signIndex + 1, descIndex - signIndex - 1).Trim().Trim(',').Trim(';'), out namotion.value);
                                        string t1 = line.Split('=')[0].TrimEnd();
                                        namotion.fieldName = t1.Substring(t1.LastIndexOf(' ')).Trim().Trim(',').Trim(';');
                                    }
                                    if (namotion != null)
                                    {
                                        list.Add(namotion);
                                    }
                                }
                                namotion = null;
                            }
                            else if (line.Contains("enum") || line.Contains("class"))
                            {
                                isStart = true;
                            }
                        }
                    }
                }
            }
            return list;
        }

        public static List<NamotionReflection> GetNamotions(string path)
        {
            List<NamotionReflection> list = new List<NamotionReflection>();
            if (File.Exists(path))
            {
                string content = File.ReadAllText(path, System.Text.UTF8Encoding.UTF8);
                if (content != "")
                {
                    string[] te = content.Split('\n');
                    bool isStart = false;
                    bool isEnd = false;
                    NamotionReflection namotion = null;
                    for (int i = 0; i < te.Length; i++)
                    {
                        string line = te[i].Trim('\r').Trim();
                        if (!string.IsNullOrEmpty(line))
                        {
                            if (isEnd)
                            {
                                int signIndex = line.IndexOf('=');
                                if (signIndex > -1)
                                {
                                    int.TryParse(line.Substring(signIndex + 1).Trim().Trim(','), out namotion.value);
                                    isEnd = false;
                                    list.Add(namotion);
                                }
                            }
                            else if (isStart)
                            {
                                namotion.summaryDesc = line.Replace("///", "").Trim();
                                isStart = false;
                            }
                            else if (line.Contains("<summary>"))
                            {
                                namotion = new NamotionReflection();
                                isStart = true;
                            }
                            else if (line.Contains("</summary>"))
                            {
                                isEnd = true;
                            }
                        }
                    }
                }
            }
            return list;
        }
    }
}
