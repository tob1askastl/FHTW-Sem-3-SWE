using System.Text;
using System.Text.Json.Serialization;

namespace MTCG
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Bio { get; set; } = "Basic biographie";
        public string Image { get; set; } = ":^(";
        public int Id { get; set; } = -1;

        // Währung für Cards
        public int RitoPoints { get; set; } = 20;
        public int EloPoints { get; set; } = 100;
        public int Victories { get; set; } = 0;
        public int Defeats { get; set; } = 0;
        public int Draws { get; set; } = 0;

        // Stack: alle verfügbaren Karten
        public List<Card>? Stack { get; private set; }

        // Deck: 4 ausgewählte Karten für den Kampf
        public List<Card>? Deck { get; private set; }

        /*
        public User(string uname, string pwd)
        {
            Username = uname;
            Password = pwd;
            Stack = new List<Card>();
            Deck = new List<Card>();
        }

        public User(int id, string uname, string pwd, string bio, string image, int rp, int elo, int victs, int losses, int draws) : this(uname, pwd)
        {
            Id = id;
            Username = uname;
            Password = pwd;
            Bio = bio;
            Image = image;
            RitoPoints = rp;
            EloPoints = elo;
            Victories = victs;
            Defeats = losses;
            Draws = draws;
            Stack = new List<Card>();
            Deck = new List<Card>();
        }

        public User(int id, string uname, string pwd, string bio, string image, int rp)
        {
            Id = id;
            Username = uname;
            Password = pwd;
            Bio = bio;
            Image = image;
            RitoPoints = rp;
        }

        public User()
        {
            // only 4 serializer               
        }

        public User(string username, string bio, string image)
        {
            Username = username;
            Bio = bio; 
            Image = image;
        }
        */

        public void DecreaseRitoPoints(int amount)
        {
            if (amount >= 0 && RitoPoints - amount >= 0)
            {
                RitoPoints -= amount;
            }
            else
            {
                throw new InvalidOperationException("Ungültiger Betrag für die Verringerung der RitoPoints.");
            }
        }

        public string PrintStats()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Username).Append(" | EloPoints: ").Append(EloPoints)
                .Append(" | Victories: ").Append(Victories)
                .Append(" | Defeats: ").Append(Defeats)
                .Append(" | Draws: ").Append(Draws);

            return sb.ToString();
        }

        public void SetUsername(string newName)
        {
            Username = newName;
        }

        public void SetBio(string newBio)
        {
            Bio = newBio;
        }

        public void SetImage(string newImage)
        {
            Image = newImage;
        }

        public void WinGame()
        {
            Victories++;
        }

        public void LoseGame()
        {
            Defeats++;
        }

        public void DrawGame()
        {
            Draws++;
        }
    }
}
