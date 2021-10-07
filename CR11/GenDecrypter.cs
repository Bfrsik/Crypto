using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;


namespace CR11
{
    public class Feature
    {
        public List<char> Gens;
        public double Fitness = 0;

        public Feature(List<char> gens, double fitness)
        {
            Gens = gens;
            Fitness = fitness;
        }

        public Feature(Feature feature)
        {
            Gens = new List<char>(feature.Gens);
            Fitness = feature.Fitness;
        }
    }

    public class GenDecrypter
    {
        public string text;
        public string BestText;
        public Cypher Crypter = new Cypher();
        public double BestFit = double.PositiveInfinity;
        public Feature BestFeature;

        private bool needMerge = true;
        private double T = 0.4;
        private double eps = 0.9;
        private int maxIters = 300;
        private List<Feature> population = new List<Feature>();
        private double mutationProbability = 0.3;
        private int popSize = 300;
        private double w1 = 1, w2 = 0.9, w3 = 0.6;

        FreqScore FreqsHandler;

        // FreqScore
        public GenDecrypter(string mPath, string biPath, string triPath)
        {
            FreqsHandler = new FreqScore();
            FreqsHandler.LoadControlScores(mPath, biPath, triPath);
        }

        public double CalculateLoss(Feature feature)
        {
            if (feature.Fitness!=0)
            {
                return feature.Fitness;
            }
            Crypter.Key = Crypter.ListToDict(feature.Gens);
            FreqsHandler.CalculateFreq(Crypter.DecryptText(text));

            double scoreM =
                FreqsHandler.MonoScore.Sum(mon => Math.Abs(mon.Value - FreqsHandler.controlMonoScore[mon.Key]));
            double scoreB = FreqsHandler.BeScore.Sum(mon => Math.Abs(mon.Value - FreqsHandler.controlBeScore[mon.Key]));
            double scoreT =
                FreqsHandler.TriScore.Sum(mon => Math.Abs(mon.Value - FreqsHandler.controlTriScore[mon.Key]));

            double score = scoreM * w1 + scoreB * w2 + scoreT * w3;
            feature.Fitness = score;
            return score;
        }

        public string Circle(List<char> first, List<char> second, int i)
        {
            char f = first[i];
            string circle = $"{f}";
            char s = second[i];
            while (s != f)
            {
                circle += s;
                s = second[first.IndexOf(s)];
            }

            return circle;
        }

        public List<char> Merge(List<char> first, List<char> second)
        {
            List<char> child = new List<char>(second);
            string c = Circle(first, second, 0);
            for (int i = 0; i < c.Length; i++)
            {
                child[first.IndexOf(c[i])] = c[i];
            }

            return child;
        }

        public List<Feature> GeneratePopulation(int n)
        {
            List<Feature> newPop = new List<Feature>();
            Random r = new Random();
            for (int i = 0; i < n; i++)
            {
                List<char> t = Crypter.GenerateRndKey(r);
                Feature newFeature = new Feature(t, 0);
                CalculateLoss(newFeature);
                newPop.Add(newFeature);
            }
            return newPop;
        }

        public Feature Mutation(Feature feature)
        {
            Feature newFeature = new Feature(feature);
            Random r = new Random();
            int j = r.Next(33);
            int i = r.Next(33);
            (newFeature.Gens[i], newFeature.Gens[j]) = (newFeature.Gens[j], newFeature.Gens[i]);
            feature.Fitness = 0;
            return newFeature;
        }

        [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH", MessageId = "type: System.String")]
        [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH", MessageId = "type: System.String; size: 414883MB")]
        [SuppressMessage("ReSharper.DPA", "DPA0002: Excessive memory allocations in SOH", MessageId = "type: System.Char")]
        public void Process()
        {
            population = GeneratePopulation(popSize);
            for (int j = 0; j < maxIters && BestFit >= eps; j++)
            {
                double currentBestFit = 0;
                Feature currentBestFeature;
                List<Feature> newBoys = new List<Feature>();
                Random r = new Random();
                
                //cross
                if (needMerge)
                {
                    for (int i = 0; i < popSize; i++)
                    {
                        newBoys.Add(new Feature(Merge(population[i].Gens, population[r.Next(popSize)].Gens), 0));
                    }

                    population.AddRange(newBoys);
                }
               
                //mutations
                foreach (var feature in population)
                {
                    double a = r.NextDouble();
                    if (a <= mutationProbability)
                    {
                        newBoys.Add(Mutation(feature));
                    }
                }
                population.AddRange(newBoys);
                
                //selection
                foreach (var feature in population)
                {
                    CalculateLoss(feature);
                }
                population = population.OrderBy(x => x.Fitness).ToList();
                
                currentBestFit = population[0].Fitness;
                currentBestFeature = population[0];
                int eliteSize = (int) (popSize * T);
                newBoys = new List<Feature>();
                newBoys.AddRange(population.GetRange(0, eliteSize));
                int least = popSize - newBoys.Count;
                newBoys.AddRange(GeneratePopulation(least));

                if (currentBestFit <= BestFit)
                {
                    BestFit = currentBestFit;
                    BestFeature = currentBestFeature;
                }

                Console.WriteLine($"Iter number: {j} current bestFit: {currentBestFit}");
                Crypter.Key = Crypter.ListToDict(BestFeature.Gens);
                BestText = Crypter.DecryptText(text);
                File.WriteAllText("DecryptedText.txt",BestText);

            }
            
            Crypter.Key = Crypter.ListToDict(BestFeature.Gens);
            BestText = Crypter.DecryptText(text);

        }

        //No More Genetic
        public void StepWithoutGenetic()
        {
            for (int i = 0; i < 32; i++)
            {
                for (int j = i + 1; j < 33; j++)
                {
                    List<char> testingKey = new List<char>(BestFeature.Gens);
                    (testingKey[i], testingKey[j]) = (testingKey[j], testingKey[i]);
                    Feature newFeature = new Feature(testingKey, 0);
                    double newFit = CalculateLoss(newFeature);
                    if (newFit < BestFit)
                    {
                        BestFit = newFit;
                        BestFeature = newFeature;
                    }
                }
            }
        }

        public void GenerateFirstKey()
        {
            Feature rusDictFeature = new Feature(new List<char>(Crypter.alphabet.ToArray()), 0);
            FreqsHandler.CalculateFreq(text);
            Dictionary<char, char> keyAssociation = new Dictionary<char, char>();
            var mono = FreqsHandler.MonoScore.OrderBy(pair => pair.Value).ToList();
            var control = FreqsHandler.controlMonoScore.OrderBy(pair => pair.Value).ToList();
            for (int i = 0; i < 33; i++)
            {
                keyAssociation.Add(char.Parse(control[i].Key), char.Parse(mono[i].Key));
            }
            
            List<char> bestKey = new List<char>();
            foreach (var item in Crypter.alphabet)
            {
                bestKey.Add(keyAssociation[item]);
            }

            Feature newFeature = new Feature(bestKey, 0);
            CalculateLoss(newFeature);
            BestFeature = newFeature;
            BestFit = BestFeature.Fitness;
        }

        public void DecryptTextNoGen()
        {
            GenerateFirstKey();
            for (int i = 0; i < maxIters && BestFit>=eps; i++)
            {
                StepWithoutGenetic();
                Console.WriteLine($"Iter number: {i} current bestFit: {BestFit}");
            }
            BestText = Crypter.DecryptText(text);
        }
    }
}