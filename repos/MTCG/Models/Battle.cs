using MTCG.Repositories;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    public class Battle
    {
        public User User1 { get; private set; }
        public User User2 { get; private set; }
        public List<string> BattleLog { get; private set; }
        private const int MaxRounds = 100;
        private int rounds;
        private readonly UserRepository userRepository;

        public Battle(User user1, User user2)
        {
            User1 = user1;
            User2 = user2;
            BattleLog = new List<string>();
            userRepository = new UserRepository();
        }

        public void Start()
        {
            BattleLog.Add($"{User1.Username} vs {User2.Username}");
            rounds = 1;

            while (!IsBattleOver())
            {
                ExecuteRound();
            }

            Console.WriteLine("game is over");

            EndBattle();
        }

        private void ExecuteRound()
        {
            Console.WriteLine("--- Round {0} ---", rounds);

            Card cardUser1 = GetRandomCard(User1.Deck);
            Card cardUser2 = GetRandomCard(User2.Deck);

            Console.WriteLine("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
            Console.WriteLine("cnt u1: {0} ({1})", User1.Deck.Count, User1.Username);
            Console.WriteLine("cnt u2: {0} ({1})", User2.Deck.Count, User2.Username);
            Console.WriteLine("rdmcard u1:" + cardUser1.ToString());
            Console.WriteLine("rdmcard u2:" + cardUser2.ToString());

            // Überprüfe, ob beide Spieler eine Karte gezogen haben
            if (cardUser1 != null && cardUser2 != null)
            {
                /*
                // Führe die Schadensberechnung durch
                int damageUser1 = CalculateDamage(cardUser1, cardUser2);
                int damageUser2 = CalculateDamage(cardUser2, cardUser1);
                */

                int damageUser1 = cardUser1.Damage;
                int damageUser2 = cardUser2.Damage;

                // Logge die Ergebnisse der Runde
                BattleLog.Add($"{User1.Username}'s {cardUser1.Name} ({cardUser1.Damage} Damage) vs {User2.Username}'s {cardUser2.Name} ({cardUser2.Damage} Damage)");

                // Überprüfe den Schaden und handle die Karten entsprechend
                if (damageUser1 > damageUser2)
                {
                    Console.WriteLine($"{User1.Username} wins the round!");
                    BattleLog.Add($"{User1.Username} wins the round!");
                    MoveCardToDeck(User1, User2, cardUser2);
                }

                else if (damageUser2 > damageUser1)
                {
                    Console.WriteLine($"{User2.Username} wins the round!");
                    BattleLog.Add($"{User2.Username} wins the round!");
                    MoveCardToDeck(User2, User1, cardUser1);
                }

                else
                {
                    Console.WriteLine("Draw in the round!");
                    BattleLog.Add("Draw in the round!");
                    // Bei einem Unentschieden passiert nichts
                }
            }

            rounds++;
        }

        // Füge die Methode CalculateDamage im BattleManager hinzu
        private int CalculateDamage(Card attacker, Card defender)
        {
            // Führe hier die Logik für die Schadensberechnung basierend auf den Kartentypen durch
            // Beispiel: Bei einem Monsterkampf ist der Schaden einfach der auf der Karte angegebene Wert
            // Beachte die Elemente und Spezialfälle (Goblins, Wizzards, Knights, Kraken, FireElves)
            return attacker.Damage;
        }

        // Füge die Methode MoveCardToDeck im BattleManager hinzu
        private void MoveCardToDeck(User winner, User loser, Card card)
        {
            Console.WriteLine("Karte {0} wird zum Deck von {1} hinzugefügt", card.Name, winner.Username);
            // Füge die gewonnene Karte zum Deck des Benutzers hinzu (während des Kampfes)
            loser.Deck.Remove(card);
            winner.Deck.Add(card);

            Console.WriteLine("Karten von {0}", loser.Username);
            foreach (Card c in loser.Deck)
            {
                Console.WriteLine(c.Name);
            }

            Console.WriteLine("Karten von {0}", winner.Username);
            foreach (Card b in winner.Deck)
            {
                Console.WriteLine(b.Name);
            }
        }

        private Card GetRandomCard(List<Card> deck)
        {
            if (deck.Count == 0)
            {
                // Deck ist leer, es gibt keine Karten mehr
                return null;
            }

            Random random = new Random();
            int randomIndex = random.Next(deck.Count);

            Card randomCard = deck[randomIndex];
            //deck.RemoveAt(randomIndex);

            return randomCard;
        }

        public bool IsBattleOver()
        {
            return (User1.Deck.Count == 0 || User2.Deck.Count == 0 || rounds >= MaxRounds);
        }

        public void EndBattle()
        {
            if (rounds >= MaxRounds)
            {
                // Unentschieden
                User1.DrawGame();
                User2.DrawGame();
            }

            else if (User1.Deck.Count == 0)
            {
                Console.WriteLine("spieler 2 gewinnt");
                // Spieler 2 gewinnt
                User1.LoseGame();
                User2.WinGame();

                // Update(Winner, Loser);
                UpdateEloPoints(User2, User1);
            }

            else if (User2.Deck.Count == 0)
            {
                Console.WriteLine("spieler 1 gewinnt");
                // Spieler 1 gewinnt
                User1.WinGame();
                User2.LoseGame();

                // Update(Winner, Loser);
                UpdateEloPoints(User1, User2);
            }

            else
            {
                // Maximale Rundenanzahl erreicht
                User1.DrawGame();
                User2.DrawGame();
            }

            Console.WriteLine("\n\nBATTLELOG\n");
            foreach (string line in BattleLog)
            {
                Console.WriteLine(line);
            }
        }

        private void UpdateEloPoints(User winner, User loser)
        {
            const int eloChange = 20;

            // Aktualisiere die Elo-Punkte basierend auf dem festgelegten Wert
            winner.EloPoints += eloChange;
            loser.EloPoints -= eloChange;

            // Aktualisiere die Elo-Punkte in der Datenbank für beide Benutzer
            userRepository.EditStats(winner);
            userRepository.EditStats(loser);
        }
    }
}
