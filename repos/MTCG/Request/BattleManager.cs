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

            User User1 = userRepository.GetUserByToken(initiatingPlayerToken);

            if (User1 != null)
            {
                // Suche einen Gegner (= User, der ebenfalls ein Deck hat und nicht der Initiating-Player ist)
                User User2 = GetRandomPlayer(User1.Id);

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

        public User GetRandomPlayer(int excludedUserId)
        {
            List<User> usersWithDecks = userRepository.GetUsersWithDecks();

            if (usersWithDecks.Count > 1)
            {
                Random random = new Random();
                int randomIndex = random.Next(0, usersWithDecks.Count);

                // Der "RandomPlayer" darf nicht die ID des Initiaters haben
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
