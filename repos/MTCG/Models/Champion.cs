namespace MTCG
{
    public class Champion : Card
    {
        public int HealthPoints { get; set; }
        public Champion(string name, string descr, ERegion region, int hP) : base(name, descr, region)
        {
            HealthPoints = hP;
        }

        public void Attack()
        {

        }

        // Stringbuilder, Interpolation
        public override string ToString() 
        {
            return Name + " - " + Description + "\nDer Champion stammt aus der Region " + Region + " und hat " + HealthPoints + " HP.";
        }
    }
}
