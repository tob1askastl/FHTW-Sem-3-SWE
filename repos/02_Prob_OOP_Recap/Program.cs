using System.Security.AccessControl;

namespace _02_Prob_OOP_Recap
{
    internal class Program
    {
        static void Main(string[] args)
        {
            List<Animal> pets = new List<Animal>();

            Animal dog = new Dog("Jimmy", 5);
            pets.Add(dog);

            Animal cat = new Cat("Katsumi", 12);
            pets.Add(cat);

            Animal bird = new Bird("X", 7);
            pets.Add(bird);

            foreach (Animal a in pets)
            {
                a.Sound();

                if (a is IFlyable flyableAnimal)
                {
                    flyableAnimal.Fly();
                }
            }
        }
    }
}