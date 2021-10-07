using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace CR11
{
    public class Cypher
    {
        public string alphabet = "абвгдеёжзийклмнопрстуфхцчшщъыьэюя";
        public Dictionary<char, char> Key = new Dictionary<char, char>();

        public Dictionary<char, char> ListToDict(List<char> key)
        {
            Dictionary<char, char> toReturn = new Dictionary<char, char>();
            int k = 0;
            foreach (var item in key)
            {
                toReturn[alphabet[k]] = item;
                k++;
            }
            return toReturn;
        }
        

        public List<char> GenerateRndKey(Random r)
        {
           
            List<char> key = new List<char>("абвгдеёжзийклмнопрстуфхцчшщъыьэюя");
    
            for (int i = 0; i < key.Count; i++)
            {
                int j = r.Next(33);
                (key[i], key[j]) = (key[j], key[i]);
            }
            
            Key = ListToDict(key);
            return key;
        }

        public string  EncryptText(string text)
        {
            string newText = "";
            foreach (var c in text)
            {
                newText += Key[c];
            }

            return newText;
        }
        
        public string DecryptText(string text)
        {
            Dictionary <char, char> reverseKey = new Dictionary <char, char>();
            foreach (var pair in Key)
            {
                reverseKey[pair.Value]=pair.Key;
            }

            return text.Aggregate("", (current, c) => current + reverseKey[c]);
        }
    }
}