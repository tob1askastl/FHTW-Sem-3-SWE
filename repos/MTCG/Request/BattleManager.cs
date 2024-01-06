using MTCG.Models;
using MTCG.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Request
{
    public class BattleManager
    {
        private readonly UserRepository userRepository;

        public BattleManager()
        {
            userRepository = new UserRepository();
        }

        public void StartBattle(string initiatingPlayerToken)
        {
            CardRepository cardRepository = new CardRepository();

            // Hole den ersten Spieler basierend auf dem Token
            User User1 = userRepository.GetUserByToken(initiatingPlayerToken);

            // Überprüfe, ob der erste Spieler verfügbar ist
            if (User1 != null)
            {
                // Hole einen zufälligen zweiten Spieler aus der Datenbank, der ein Deck hat
                User User2 = GetRandomPlayer(User1.Id);

                // Überprüfe, ob der zweite Spieler verfügbar ist
                if (User2 != null)
                {
                    User1.Deck = cardRepository.GetCardsInDeckForUser(User1.Id);
                    User2.Deck = cardRepository.GetCardsInDeckForUser(User2.Id);

                    Battle battle = new Battle(User1, User2);
                    battle.Start();
                }

                else
                {
                    Console.WriteLine("Error: Kein zweiter Spieler gefunden.");
                }
            }

            else
            {
                Console.WriteLine("Error: Spieler nicht gefunden.");
            }
        }

        private User GetRandomPlayer(int excludedUserId)
        {
            // Hole einen zufälligen Spieler aus der Datenbank, der ein Deck hat und nicht der erste Spieler ist
            List<User> usersWithDecks = userRepository.GetUsersWithDecks();

            if (usersWithDecks.Count > 1)
            {
                Random random = new Random();
                int randomIndex = random.Next(0, usersWithDecks.Count);

                // Stelle sicher, dass der zufällige Spieler nicht der erste Spieler ist
                while (usersWithDecks[randomIndex].Id == excludedUserId)
                {
                    randomIndex = random.Next(0, usersWithDecks.Count);
                }

                return usersWithDecks[randomIndex];
            }

            return null;
        }
    }
}
