using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _02_Prob_OOP_Recap
{
    public abstract class Animal
    {
        protected string Nickname { get; set; }
        protected int Age { get; set; }

        public virtual void Sound()
        {
            Console.WriteLine("The animal {0} is {1} years old.", Nickname, Age);
        }
    }

    public class Cat : Animal
    {
        public Cat(string name, int age)
        {
            Nickname = name;
            Age = age;
        }

        public override void Sound()
        {
            Console.WriteLine($"The cat {Nickname} is {Age} years old and meows.");
        }
    }

    public class Dog : Animal
    {
        public Dog(string name, int age)
        {
            Nickname = name;
            Age = age;
        }

        public override void Sound() 
        {
            Console.WriteLine($"The dog {Nickname} is {Age} years old and barks.");
        }
    }

    public class Bird : Animal, IFlyable
    {
        public Bird(string name, int age)
        {
            Nickname = name;
            Age = age;
        }

        public void Fly()
        {
            Console.WriteLine($"{Nickname} is able to fly.");
        }

        public override void Sound()
        {
            Console.WriteLine($"The bird {Nickname} is {Age} years old and chirps.");
        }
    }
}
