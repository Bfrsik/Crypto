using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    }
    public class GenDecrypter
    {
        public string text;
        public string BestText;
        public Cypher Crypter = new Cypher();
        public double BestFit = double.PositiveInfinity;
        public Feature BestFeature;

        private double T = 0.9;
        private double eps = 1e-7;
        private int maxIters = 10;
        private List<Feature> population = new List<Feature>();
        private double mutationProbability=0.2;
        private int popSize = 300;
        private double w1=1, w2=0.9, w3=0.6;
        private string bestKey;
        FreqScore FreqsHandler;
        // FreqScore
        public GenDecrypter(string mPath, string biPath, string triPath)
        {
            FreqsHandler = new FreqScore();
            FreqsHandler.LoadControlScores(mPath,biPath,triPath);
        }
        public double CalculateLoss(List<char> gen)
        {
            Crypter.Key=Crypter.ListToDict(gen);
            FreqsHandler.CalculateFreq(Crypter.DecryptText(text));

            double scoreM = FreqsHandler.MonoScore.Sum(mon => Math.Abs(mon.Value - FreqsHandler.controlMonoScore[mon.Key]));
            double scoreB = FreqsHandler.BeScore.Sum(mon => Math.Abs(mon.Value - FreqsHandler.controlBeScore[mon.Key]));
            double scoreT = FreqsHandler.TriScore.Sum(mon => Math.Abs(mon.Value - FreqsHandler.controlTriScore[mon.Key]));

            double score = scoreM * w1 + scoreB * w2 + scoreT * w3;
            return score;
        }
        public string Circle(List<char> first, List<char> second, int i)
        {
            
            char f = first[i];
            string circle=$"{f}";
            char s = second[i];
            while (s!=f)
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
            for (int i=0; i<c.Length; i++)
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
                newPop.Add(new Feature(t,0));
            }

            return newPop;
        }

        public void Mutation(Feature feature)
        {
            Random r = new Random();
            int j = r.Next(33);
            int i= r.Next(33);
            (feature.Gens[i], feature.Gens[j]) = (feature.Gens[j], feature.Gens[i]);
        }

        public void Process()
        {
            population = GeneratePopulation(popSize);
            for (int j = 0; j < maxIters && BestFit>=eps; j++)
            {
                double currentBestFit = 0;
                Feature currentBestFeature;
                List<Feature> newBoys = new List<Feature>();
                Random r = new Random();
                for (int i = 0; i < popSize; i++)
                {
                    newBoys.Add(new Feature(Merge(population[i].Gens,population[r.Next(popSize)].Gens),0));
                }
                
                population.AddRange(newBoys);
            
                foreach (var feature in population)
                {
                    double a = r.NextDouble();
                    if (a<=mutationProbability)
                    {
                        Mutation(feature);
                    }
                }
                foreach (var feature in population)
                {
                    feature.Fitness = CalculateLoss(feature.Gens);
                }
                population = population.OrderBy(x => x.Fitness).ToList();
                currentBestFit = population[0].Fitness;
                currentBestFeature = population[0];
                int eliteSize = (int) (popSize * T);
                newBoys = new List<Feature>();
                newBoys.AddRange(population.GetRange(0,eliteSize));
                int least = popSize - newBoys.Count;
                newBoys.AddRange(GeneratePopulation(least));
            
                if (currentBestFit<=BestFit)
                {
                    BestFit = currentBestFit;
                    BestFeature = currentBestFeature;
                }
                Console.WriteLine($"Iter number: {j} current bestFit: {currentBestFit}");
            }
            Crypter.Key = Crypter.ListToDict(BestFeature.Gens);
            BestText = Crypter.DecryptText(text);
        }
        
        public void StepWithoutGenetic()
        {
            for (int i = 0; i < 32; i++)
            {
                for (int j = i+1; j < 33; j++)
                {
                    List<char> testingKey = new List<char>(bestKey);
                    (testingKey[i], testingKey[j]) = (testingKey[j], testingKey[i]);
                    double newFit = CalculateLoss(testingKey);
                    if (newFit < BestFit)
                    {
                        BestFit = newFit;
                        BestFeature = new Feature(testingKey,BestFit);
                    }          
                }
            }
        }
    }
}