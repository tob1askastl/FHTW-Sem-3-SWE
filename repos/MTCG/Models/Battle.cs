using Microsoft.Extensions.Logging;
using MTCG.Repositories;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            BattleLog.Add($"--- {User1.Username} vs {User2.Username} ---");
            rounds = 1;

            while (!IsBattleOver())
            {
                ExecuteRound();
            }

            Console.WriteLine("--- GG ---");
            BattleLog.Add("--- GG ---");

            EndBattle();
        }

        public void ExecuteRound()
        {
            Console.WriteLine("--- Round {0} ---", rounds);

            Card cardUser1 = GetRandomCard(User1.Deck);
            Card cardUser2 = GetRandomCard(User2.Deck);

            if (cardUser1 != null && cardUser2 != null)
            {
                int damageUser1 = cardUser1.Damage;
                int damageUser2 = cardUser2.Damage;

                BattleLog.Add($"{User1.Username}'s {cardUser1.Name} ({cardUser1.Damage} Damage) vs {User2.Username}'s {cardUser2.Name} ({cardUser2.Damage} Damage)");

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
                }

                CheckRedemption(User1);
                CheckRedemption(User2);
            }

            rounds++;
        }

        public void CheckRedemption(User user)
        {
            if (user.Deck.Count == 1)
            {
                Random random = new Random();

                if (random.Next(1, 101) <= 25)
                {
                    StealCardFromOpponent(user);
                }

                else
                {
                    BattleLog.Add("Redemption failed");
                    Console.WriteLine("Redemption failed");
                }
            }
        }

        public void StealCardFromOpponent(User user)
        {
            User opponent = (user == User1) ? User2 : User1;
            Card stolenCard = GetRandomCard(opponent.Deck);

            if (stolenCard != null)
            {
                Console.WriteLine($"{user.Username} triggered Redemption and stole the card {stolenCard.Name} from {opponent.Username}");
                BattleLog.Add($"{user.Username} triggered Redemption and stole the card {stolenCard.Name} from {opponent.Username}");

                MoveCardToDeck(user, opponent, stolenCard);
            }
        }

        public void MoveCardToDeck(User winner, User loser, Card card)
        {
            Console.WriteLine($"Card {card.Name} gets added to {winner.Username}'s Deck");
            BattleLog.Add($"Card {card.Name} gets added to {winner.Username}'s Deck\n");

            // Verlierer gibt die Karte an den Gewinner ab
            loser.Deck.Remove(card);
            winner.Deck.Add(card);
        }

        public Card GetRandomCard(List<Card> deck)
        {
            // Deck ist leer
            if (deck.Count == 0)
            {
                return null;
            }

            Random random = new Random();
            int randomIndex = random.Next(deck.Count);

            Card randomCard = deck[randomIndex];

            return randomCard;
        }

        public bool IsBattleOver()
        {
            return (User1.Deck.Count == 0 || User2.Deck.Count == 0 || rounds > MaxRounds);
        }

        public void EndBattle()
        {
            // Spieler 2 gewinnt
            if (User1.Deck.Count == 0)
            {
                Console.WriteLine($"{User2.Username} wins!");
                BattleLog.Add($"{User2.Username} wins!\n");

                User1.LoseGame();
                User2.WinGame();

                // Update(Winner, Loser);
                UpdateEloPoints(User2, User1);
            }

            // Spieler 1 gewinnt
            else if (User2.Deck.Count == 0)
            {
                Console.WriteLine($"{User1.Username} wins!");
                BattleLog.Add($"{User1.Username} wins!\n");

                User1.WinGame();
                User2.LoseGame();

                // Update(Winner, Loser);
                UpdateEloPoints(User1, User2);
            }

            // Unentschieden
            else
            {
                Console.WriteLine("Draw!");
                BattleLog.Add("Draw!\n");

                User1.DrawGame();
                User2.DrawGame();
            }

            userRepository.EditStats(User1);
            userRepository.EditStats(User2);

            Console.WriteLine("\n\nBATTLELOG\n");
            foreach (string line in BattleLog)
            {
                Console.WriteLine(line);
            }
        }

        public void UpdateEloPoints(User winner, User loser)
        {
            // Further Features: elo value
            winner.EloPoints += 3;
            loser.EloPoints -= 5;
        }

        public int GetMaxRounds()
        {
            return MaxRounds;
        }

        public int GetRounds()
        {
            return rounds;
        }
    }
}
