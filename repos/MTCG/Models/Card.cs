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
        public string Name { get; private set; }
        public ERegion Region { get; private set; }
        public int Damage { get; private set; }

        [JsonProperty("card_type")] // This attribute maps the JSON key to the property
        public string CardType { get; private set; }
        public bool IsUsed { get; set; } = false;
        public int OwnerID { get; set; } = -1;
        public bool Is_In_Deck { get; set; } = false;


        [JsonConstructor] // This constructor is used during deserialization
        protected Card(string name, ERegion region, int damage, string cardType)
        {
            Name = name;
            Region = region;
            Damage = damage;
            CardType = cardType;
            IsUsed = false;
            OwnerID = -1;
            Is_In_Deck = false;
        }
        /*
        protected Card(string name, ERegion region, int dmg, string cardType, bool is_used, int owner_id)
        {
            Name = name;
            Region = region;
            Damage = dmg;
            CardType = cardType;
            IsUsed = is_used;
            OwnerID = owner_id;
        }
        */
        // Check if the "card_type" attribute is present
        public bool HasCardTypeAttribute()
        {
            return !string.IsNullOrEmpty(CardType);
        }

        // Get the value of the "card_type" attribute
        public string GetCardTypeAttribute()
        {
            return CardType;
        }

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
