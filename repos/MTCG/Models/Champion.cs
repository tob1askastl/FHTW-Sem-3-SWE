﻿using System.Text;

namespace MTCG
{
    public class Champion : Card
    {
        public Champion(string name, ERegion region, int damage, string card_type) : base(name, region, damage, card_type)
        {
                
        }
        /*
        public Champion(string name, ERegion region, int dmg, string card_type, bool is_used, int owner_id) : base(name, region, dmg, card_type, is_used, owner_id)
        {

        }
        */
    }
}
