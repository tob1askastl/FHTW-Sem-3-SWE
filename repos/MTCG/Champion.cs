﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
    public class Champion : Card
    {
        public int HealthPoints { get; set; }
        public Champion(string name, string descr, ERegion region, int hP) : base(name, descr, region)
        {
            HealthPoints = hP;
        }

        public void Attack()
        {

        }

        public override string ToString() 
        {
            return Name + " - " + Description + "\nVon " + Region + " hat " + HealthPoints + " HP.";
        }
    }
}