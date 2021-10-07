using System.Collections.Generic;
using System.IO;

namespace CR11
{
    public class FreqScore
    {
        public Dictionary<string, double> MonoScore = new Dictionary<string, double>();
        public Dictionary<string, double> BeScore = new Dictionary<string, double>();
        public Dictionary<string, double> TriScore = new Dictionary<string, double>();

        public Dictionary<string, double> controlMonoScore = new Dictionary<string, double>();
        public Dictionary<string, double> controlBeScore = new Dictionary<string, double>();
        public Dictionary<string, double> controlTriScore = new Dictionary<string, double>();
        public string alphabet = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя";

        public void MonoFreq(string text)
        {
            for (int i = 0; i < alphabet.Length; i++)
            {
                MonoScore[$"{alphabet[i]}"] = 0;
            }

            for (int i = 0; i < text.Length; i++)
            {
                MonoScore[$"{text[i]}"] += 1;
            }
        }

        public void BeFreq(string text)
        {
            for (int i = 0; i < alphabet.Length; i++)
            {
                for (int j = 0; j < alphabet.Length; j++)
                {
                    BeScore[$"{alphabet[i]}{alphabet[j]}"] = 0;
                }
            }

            for (int i = 0; i < text.Length - 1; i++)
            {
                BeScore[$"{text[i]}{text[i + 1]}"] += 1;
            }
        }

        public void TriFreq(string text)
        {
            for (int i = 0; i < alphabet.Length; i++)
            {
                for (int j = 0; j < alphabet.Length; j++)
                {
                    for (int k = 0; k < alphabet.Length; k++)
                    {
                        TriScore[$"{alphabet[i]}{alphabet[j]}{alphabet[k]}"] = 0;
                    }
                }
            }

            for (int i = 0; i < text.Length - 2; i++)
            {
                TriScore[$"{text[i]}{text[i + 1]}{text[i + 2]}"] += 1;
            }
        }

        public void LoadControlScores(string mPath, string bPath, string triPath)
        {
            string[] scores = File.ReadAllLines(mPath);
            foreach (var score in scores)
            {
                controlMonoScore[score.Split(' ')[0]] = double.Parse(score.Split(' ')[1]);
            }

            scores = File.ReadAllLines(bPath);
            foreach (var score in scores)
            {
                controlBeScore[score.Split(' ')[0]] = double.Parse(score.Split(' ')[1]);
            }

            scores = File.ReadAllLines(triPath);
            foreach (var score in scores)
            {
                controlTriScore[score.Split(' ')[0]] = double.Parse(score.Split(' ')[1]);
            }
        }

        public void DumpControlScores()
        {
            string mono = "";
            string be = "";
            string tri = "";

            foreach (var score in MonoScore)
            {
                mono += $"{score.Key} {score.Value}\n";
            }

            foreach (var score in BeScore)
            {
                be += $"{score.Key} {score.Value}\n";
            }

            foreach (var score in TriScore)
            {
                tri += $"{score.Key} {score.Value}\n";
            }

            File.WriteAllText("MonoScore.txt", mono);
            File.WriteAllText("BeScore.txt", be);
            File.WriteAllText("TriScore.txt", tri);
        }

        public void CalculateFreq(string text)
        {
            MonoFreq(text);
            BeFreq(text);
            TriFreq(text);
            foreach (char first in alphabet)
            {
                MonoScore[$"{first}"] /= text.Length;
                foreach (char second in alphabet)
                {
                    BeScore[$"{first}{second}"] /= text.Length;
                    foreach (char third in alphabet)
                    {
                        TriScore[$"{first}{second}{third}"] /= text.Length;
                    }
                }
            }
        }
    }
}