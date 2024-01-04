namespace MTCG
{
    public class Shop
    {
        public List<Champion> Champions { get; set; }
        public List<Spell> Spells { get; set; }

        public Shop()
        {
            Champions = ReadChampionsFromFile("champions.txt");
            Spells = ReadSpellsFromFile("spells.txt");
        }

        private List<Champion> ReadChampionsFromFile(string path)
        {
            List<Champion> champions = new List<Champion>();

            try
            {
                using (StreamReader sr = new StreamReader(path))
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

                        champions.Add(champion);
                    }
                }
            }

            catch (FileNotFoundException e)
            {
                Console.WriteLine($"Datei {path} wurde nicht gefunden.");
            }

            catch (IOException e)
            {
                Console.WriteLine($"Fehler beim Lesen der Datei {path}: {e.Message}");
            }

            catch (FormatException e)
            {
                Console.WriteLine($"Fehler beim Parsen der Datei {path}: {e.Message}");
            }

            return champions;
        }

        private List<Spell> ReadSpellsFromFile(string path)
        {
            List<Spell> spells = new List<Spell>();

            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] parts = line.Split(';');

                        string name = parts[0];
                        string descr = parts[1];
                        int manaCost = int.Parse(parts[2]);
                        int regionValue = int.Parse(parts[3]);

                        ERegion region = (ERegion)regionValue;

                        Spell spell = new Spell(name, descr, region, manaCost);

                        spells.Add(spell);
                    }
                }
            }

            catch (FileNotFoundException e)
            {
                Console.WriteLine($"Datei {path} wurde nicht gefunden.");
            }

            catch (IOException e)
            {
                Console.WriteLine($"Fehler beim Lesen der Datei {path}: {e.Message}");
            }

            catch (FormatException e)
            {
                Console.WriteLine($"Fehler beim Parsen der Datei {path}: {e.Message}");
            }

            return spells;
        }

        public Champion SelectRandomChampion()
        {
            if (Champions.Count == 0)
            {
                // Exception
                return null;
            }

            Random random = new Random();
            int randomIndex = random.Next(0, Champions.Count);

            Champion selectedChampion = Champions[randomIndex];
            Champions.RemoveAt(randomIndex);

            return selectedChampion;
        }

        public Spell SelectRandomSpell()
        {
            if (Spells.Count == 0)
            {
                return null;
            }

            Random random = new Random();
            int randomIndex = random.Next(0, Spells.Count);

            Spell selectedSpell = Spells[randomIndex];
            Spells.RemoveAt(randomIndex);

            return selectedSpell;
        }
    }
}
