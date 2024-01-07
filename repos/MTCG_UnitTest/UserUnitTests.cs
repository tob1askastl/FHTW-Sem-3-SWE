using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG_UnitTest
{
    [TestClass]
    public class UserUnitTests
    {
        [TestMethod]
        public void TestDecreaseRitoPoints_ValidAmount()
        {
            User user = new User();
            user.RitoPoints = 100;

            user.DecreaseRitoPoints(30);

            Assert.AreEqual(70, user.RitoPoints);
        }

        [TestMethod]
        public void TestDecreaseRitoPoints_ZeroAmount()
        {
            User user = new User();
            user.RitoPoints = 100;

            user.DecreaseRitoPoints(0);

            Assert.AreEqual(100, user.RitoPoints);
        }

        [TestMethod]
        public void TestDecreaseRitoPoints_NegativeAmount()
        {
            User user = new User();
            user.RitoPoints = 100;

            user.DecreaseRitoPoints(-20);

            Assert.AreEqual(100, user.RitoPoints);
        }

        [TestMethod]
        public void TestDecreaseRitoPoints_InsufficientBalance()
        {
            User user = new User();
            user.RitoPoints = 30;

            user.DecreaseRitoPoints(50);

            Assert.AreEqual(30, user.RitoPoints);
        }
    }
}
