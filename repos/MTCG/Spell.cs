using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
    public class Spell : Card
    {
        public int ManaCost { get; private set; }
        public Spell(string name, string descr, ERegion region, int mana) : base(name, descr, region)
        {
            ManaCost = mana;
        }

        public void UseSpell()
        {

        }

        public override string ToString()
        {
            return Name + " - " + Description + "\nDer Spell stammt aus der Region " + Region + " und kostet " + ManaCost + " Mana.";
        }
    }
}
