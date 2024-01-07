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

        // RitoPoints = Währung
        public int RitoPoints { get; set; } = 20;
        public int EloPoints { get; set; } = 100;
        public int Victories { get; set; } = 0;
        public int Defeats { get; set; } = 0;
        public int Draws { get; set; } = 0;

        // Spieldeck für den Kampf (4 Karten)
        public List<Card>? Deck { get; set; }

        public void DecreaseRitoPoints(int amount)
        {
            if (amount >= 0 && RitoPoints - amount >= 0)
            {
                RitoPoints -= amount;
            }
        }

        public string PrintStats()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Username).Append(" | EloPoints: ").Append(EloPoints)
                .Append(" | Victories: ").Append(Victories)
                .Append(" | Defeats: ").Append(Defeats)
                .Append(" | Draws: ").Append(Draws)
                .Append(" | Count of Games: ").Append(Victories + Defeats + Draws);

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
