using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;

namespace CR11
{
    internal class Program
    {
        static public void Main(String[] args)
        {
            
            
            
            FreqScore a = new FreqScore();
            a.LoadControlScores("MonoScore.txt","BeScore.txt","TriScore.txt");
            // string text =new string(File.ReadAllText(@"input.txt").ToLower().Where(
            //     x =>x>='а'&&x<='я' || x=='ё').ToArray());
            // a.CalculateFreq(text);
            // a.DumpControlScores();
            string text = new string(File.ReadAllText(@"textToDecrypt.txt").ToLower().Where(
                x =>x>='а'&&x<='я' || x=='ё').ToArray());new string(text.Where(c => (c >= 'а' && c <= 'я' || c == 'ё')).ToArray());
            //FreqScore f = new FreqScore();
            GenDecrypter gd = new GenDecrypter("MonoScore.txt","BeScore.txt","TriScore.txt");
            gd.text = text;
            gd.Process();
            File.WriteAllText("DecryptedText.txt",gd.BestText);
        }
    }
}