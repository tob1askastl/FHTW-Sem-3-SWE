namespace MTCG
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Shop shop = new Shop();

            Champion c1 = shop.SelectRandomChampion();

            Console.WriteLine(c1);
        }
    }
}