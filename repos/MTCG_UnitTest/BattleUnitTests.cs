using MTCG.Repositories;
using MTCG.Request;

namespace MTCG_UnitTest
{
    [TestClass]
    public class BattleUnitTests
    {
        private BattleManager battleManager;
        private Battle battle;
        private User user1;
        private User user2;
        private Champion champion;
        private Spell spell;

        [TestMethod]
        public void TestExecuteRound_WinnerGetsCard()
        {
            user1 = new User();
            user1.Username = "maru";
            user1.Deck = new List<Card>();

            user2 = new User();
            user2.Username = "tobias";
            user2.Deck = new List<Card>();

            battle = new Battle(user1, user2);

            // User1 soll gewinnen
            Card cardUser1 = new Champion { Damage = 20 };
            Card cardUser2 = new Champion { Damage = 10 };

            user1.Deck.Add(cardUser1);
            user2.Deck.Add(cardUser2);

            battle.ExecuteRound();

            Assert.AreEqual(2, user1.Deck.Count);
            Assert.AreEqual(0, user2.Deck.Count);
        }

        [TestMethod]
        public void TestExecuteRound_Draw()
        {
            user1 = new User();
            user1.Username = "maru";
            user1.Deck = new List<Card>();

            user2 = new User();
            user2.Username = "tobias";
            user2.Deck = new List<Card>();

            for (int i = 1; i <= 4; i++)
            {
                champion = new Champion();
                champion.Damage = 10;

                user1.Deck.Add(champion);
                user2.Deck.Add(champion);
            }

            battle = new Battle(user1, user2);

            battle.ExecuteRound();

            Assert.AreEqual(4, user1.Deck.Count);
            Assert.AreEqual(4, user2.Deck.Count);
        }


        [TestMethod]
        public void TestStealCardFromOpponent()
        {
            user1 = new User();
            user1.Username = "maru";
            user1.Deck = new List<Card>();

            user2 = new User();
            user2.Username = "tobias";
            user2.Deck = new List<Card>();

            for (int i = 1; i <= 4; i++)
            {
                champion = new Champion();
                champion.Damage = 10;

                user1.Deck.Add(champion);
                user2.Deck.Add(champion);
            }

            battle = new Battle(user1, user2);

            battle.StealCardFromOpponent(user1);

            Assert.AreEqual(5, user1.Deck.Count);
            Assert.AreEqual(3, user2.Deck.Count);
        }

        [TestMethod]
        public void TestMoveCardToDeck()
        {
            user1 = new User();
            user1.Username = "maru";
            user1.Deck = new List<Card>();

            user2 = new User();
            user2.Username = "tobias";
            user2.Deck = new List<Card>();

            battle = new Battle(user1, user2);

            // Diese Karte wird bewegt
            Card cardUser1 = new Champion { Damage = 10 };
            user1.Deck.Add(cardUser1);

            battle.MoveCardToDeck(user1, user2, cardUser1);

            Assert.AreEqual(2, user1.Deck.Count);
            Assert.AreEqual(0, user2.Deck.Count);
        }

        [TestMethod]
        public void TestCheckRedemption_Successful()
        {
            user1 = new User();
            user1.Username = "maru";
            user1.Deck = new List<Card>();

            user2 = new User();
            user2.Username = "tobias";
            user2.Deck = new List<Card>(); 

            battle = new Battle(user1, user2);

            // Redemption wird getriggered
            user1.Deck.Clear();
            user1.Deck.Add(new Champion());

            battle.CheckRedemption(user1);

            Assert.AreEqual(1, user1.Deck.Count);
            Assert.AreEqual(0, user2.Deck.Count);
        }

        [TestMethod]
        public void TestCheckRedemption_Failed()
        {
            user1 = new User();
            user1.Username = "maru";
            user1.Deck = new List<Card>(); 

            user2 = new User();
            user2.Username = "tobias";
            user2.Deck = new List<Card>(); 

            battle = new Battle(user1, user2);

            // Redemption wird nicht getriggered
            user1.Deck.Clear();
            user1.Deck.Add(new Champion());

            battle.CheckRedemption(user1);

            // Die Karte bleibt im Deck
            Assert.AreEqual(1, user1.Deck.Count); 
            Assert.AreEqual(0, user2.Deck.Count);
        }

        [TestMethod]
        public void TestGetRandomPlayer()
        {
            user1 = new User();
            user1.Id = 1;
            user1.Username = "maru";
            user1.Deck = new List<Card>();

            battleManager = new BattleManager();

            User test = new User();
            test = battleManager.GetRandomPlayer(user1.Id);

            Assert.AreEqual(2, test.Id);
        }


        [TestMethod]
        public void TestBattle()
        {
            user1 = new User();
            user1.Username = "maru";
            user1.Deck = new List<Card>();

            user2 = new User();
            user2.Username = "tobias";
            user2.Deck = new List<Card>(); 

            for (int i = 1; i <= 4; i++)
            {
                spell = new Spell();
                spell.Damage = 10;

                user1.Deck.Add(spell);
                user2.Deck.Add(spell);
            }

            battle = new Battle(user1, user2);
            battle.Start();

            Assert.AreEqual(1, user1.Draws);
            Assert.AreEqual(1, user2.Draws);
        }

        [TestMethod]
        public void TestWinGame()
        {
            user1 = new User();
            user1.WinGame();

            Assert.AreEqual(1, user1.Victories);
            Assert.AreEqual(0, user1.Defeats);
            Assert.AreEqual(0, user1.Draws);
        }

        [TestMethod]
        public void TestLoseGame()
        {
            user1 = new User();
            user1.LoseGame();

            Assert.AreEqual(0, user1.Victories);
            Assert.AreEqual(1, user1.Defeats);
            Assert.AreEqual(0, user1.Draws);
        }

        [TestMethod]
        public void TestDrawGame()
        {
            user1 = new User();
            user1.DrawGame();

            Assert.AreEqual(0, user1.Victories);
            Assert.AreEqual(0, user1.Defeats);
            Assert.AreEqual(1, user1.Draws);
        }

        [TestMethod]
        public void TestBattleLog_Winner()
        {
            user1 = new User();
            user1.Username = "maru";
            user1.Deck = new List<Card>();

            user2 = new User();
            user2.Username = "tobias";
            user2.Deck = new List<Card>();

            battle = new Battle(user1, user2);

            // User1 soll gewinnen
            Card cardUser1 = new Champion { Damage = 20 };
            Card cardUser2 = new Champion { Damage = 10 };

            user1.Deck.Add(cardUser1);
            user2.Deck.Add(cardUser2);

            battle.Start();

            CollectionAssert.Contains(battle.BattleLog, $"{user1.Username} wins the round!");
        }

        [TestMethod]
        public void TestBattle_GameEndsDueToMaxRounds()
        {
            user1 = new User();
            user1.Username = "maru";
            user1.Deck = new List<Card>();

            user2 = new User();
            user2.Username = "tobias";
            user2.Deck = new List<Card>();

            // Zu viele Karten
            for (int i = 0; i < 30; i++)
            {
                champion = new Champion();
                champion.Damage = 10;

                user1.Deck.Add(champion);
                user2.Deck.Add(champion);
            }

            battle = new Battle(user1, user2);
            battle.Start();

            Assert.IsTrue(battle.IsBattleOver());
            Assert.AreEqual(30, user1.Deck.Count);
            Assert.AreEqual(30, user2.Deck.Count);
            Assert.AreEqual(battle.GetMaxRounds() + 1, battle.GetRounds());
        }

        [TestMethod]
        public void TestBattle_GameEndsDueToEmptyDecks()
        {
            user1 = new User();
            user1.Username = "maru";
            user1.Deck = new List<Card>();

            user2 = new User();
            user2.Username = "tobias";
            user2.Deck = new List<Card>();

            battle = new Battle(user1, user2);
            battle.Start();

            Assert.IsTrue(battle.IsBattleOver());
            Assert.AreEqual(0, user1.Deck.Count);
            Assert.AreEqual(0, user2.Deck.Count);
        }

        [TestMethod]
        public void TestEloPoints_Win()
        {
            User user1 = new User();
            user1.EloPoints = 100;
            user1.Deck = new List<Card>();

            Champion champion = new Champion();
            champion.Damage = 10;
            user1.Deck.Add(champion);

            User user2 = new User();
            user2.EloPoints = 80;
            user2.Deck = new List<Card>();

            battle = new Battle(user1, user2);
            battle.EndBattle();

            Assert.AreEqual(103, user1.EloPoints); // EloPoints sollten um 3 gestiegen sein
            Assert.AreEqual(75, user2.EloPoints);   // EloPoints sollten unverändert bleiben
        }

        [TestMethod]
        public void TestEloPoints_Loss()
        {
            User user1 = new User();
            user1.EloPoints = 100;
            user1.Deck = new List<Card>();

            User user2 = new User();
            user2.EloPoints = 80;
            user2.Deck = new List<Card>();

            Champion champion = new Champion();
            champion.Damage = 10;
            user2.Deck.Add(champion);

            battle = new Battle(user1, user2);
            battle.EndBattle();

            Assert.AreEqual(95, user1.EloPoints); // EloPoints sollten unverändert bleiben
            Assert.AreEqual(83, user2.EloPoints);    // EloPoints sollten um 5 gesunken sein
        }

        [TestMethod]
        public void TestEloPoints_Draw()
        {
            User user1 = new User();
            user1.EloPoints = 90;
            user1.Deck = new List<Card>();
            Champion champion = new Champion();
            champion.Damage = 10;
            user1.Deck.Add(champion);

            User user2 = new User();
            user2.EloPoints = 95;
            user2.Deck = new List<Card>();
            user2.Deck.Add(champion);

            battle = new Battle(user1, user2);
            battle.EndBattle();

            Assert.AreEqual(90, user1.EloPoints); // EloPoints sollten unverändert bleiben
            Assert.AreEqual(95, user2.EloPoints); // EloPoints sollten unverändert bleiben
        }

    }
}