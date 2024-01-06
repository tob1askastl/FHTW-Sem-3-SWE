using Newtonsoft.Json;
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
        public int Id { get; set; }
        public string Name { get; set; }
        public ERegion Region { get; set; }
        public int Damage { get; set; }

        [JsonProperty("card_type")] // This attribute maps the JSON key to the property
        public string CardType { get; set; }
        public bool IsUsed { get; set; } = false;
        public int OwnerID { get; set; } = -1;
        public bool Is_In_Deck { get; set; } = false;

        public void SetOwnerID(int id)
        {
            OwnerID = id;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Id).Append(") ").Append(Name).Append(" | Region: ").Append(Region).Append(" | Damage: ").Append(Damage);

            if (IsUsed)
            {
                sb.Append(" | Used by: ").Append(OwnerID);
            }

            sb.Append("\n");
            return sb.ToString();
        }
    }
}
