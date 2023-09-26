using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
    public class Shop
    {
        public List<Champion> Champions { get; set; }
        public List<Spell> Spells { get; set; }

        public Shop()
        {
            Champions = new List<Champion>();

            try
            {
                using (StreamReader sr = new StreamReader("champions.txt"))
                {
                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] parts = line.Split(';');

                        string name = parts[0];
                        string descr = parts[1];
                        int regionValue = int.Parse(parts[2]);
                        int hP = int.Parse(parts[3]);

                        ERegion region = (ERegion)regionValue;

                        Champion champion = new Champion(name, descr, region, hP);

                        Champions.Add(champion); 
                    }
                }
            }

            catch (Exception e)
            {
                Console.WriteLine("Die Datei konnte nicht gelesen werden:");
                Console.WriteLine(e.Message);
            }
        }

        public Champion SelectRandomChampion()
        {
            if (Champions.Count == 0)
            {
                return null;
            }

            Random random = new Random();
            int randomIndex = random.Next(0, Champions.Count);

            Champion selectedChampion = Champions[randomIndex];
            Champions.RemoveAt(randomIndex);

            return selectedChampion;
        }
    }
}
