using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG_UnitTest
{
    [TestClass]
    public class CardUnitTests
    {
        [TestMethod]
        public void TestCardInitialization()
        {
            Card card = new Spell();
            card.Name = "Fireball";
            card.Damage = 15;
            card.CardType = "Spell";

            Assert.AreEqual("Fireball", card.Name);
            Assert.AreEqual(15, card.Damage);
            Assert.AreEqual("Spell", card.CardType);
        }
    }
}
