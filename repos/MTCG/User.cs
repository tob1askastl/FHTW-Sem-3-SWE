using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
    public class User
    {
        public string Username { get; private set; }
        public string Password { get; private set; }

        // Währung für Cards
        public int RitoPoints { get; private set; } = 20;
        public int EloPoints { get; private set; }

        // Stack: alle verfügbaren Karten
        public List<Card>? Stack { get; private set; }

        // Deck: 4 ausgewählte Karten für den Kampf
        public List<Card>? Deck { get; private set; }

        public User(string uname, string pwd)
        {
            Username = uname;
            Password = pwd;
            Stack = new List<Card>();
            Deck = new List<Card>();
        }

        public void BuyCard(Card card)
        {
            Stack.Add(card);
            RitoPoints -= 5;
        }

        public Card TradeCards(Card card)
        {
            return null;
        }

        public void Login()
        {

        }

        public void Logout()
        {

        }
    }
}
