using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
    // Piltover countert Shadow Isles wegen Highminded-Technologie
    // Shadow Isles countert Bandle City wegen Ehrfurcht
    // Bandle City countert Piltover wegen Winzigkeit von Yordles
    public enum ERegion { SHADOWISLES = 0, BANDLECITY = 1, PILTOVER = 2 };
    public abstract class Card
    {
        public string Name { get; private set; }
        public string Description { get; private set; }
        public ERegion Region { get; private set; }

        public Card(string name, string descr, ERegion region)
        {
            Name = name;
            Description = descr;
            Region = region;
        }
    }
}
