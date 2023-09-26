﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
    public class Spell : Card
    {
        public int ManaCost { get; private set; }
        public Spell(string name, string descr, int mana, ERegion region) : base(name, descr, region)
        {
            ManaCost = mana;
        }

        public void UseSpell()
        {

        }
    }
}