﻿using System.Text.Json.Serialization;

namespace MTCG
{
    public class User
    {
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Bio { get; private set; } = "Basic biographie";
        public string Image { get; private set; } = ":^(";
        public int Id { get; init; } = -1;

        // Währung für Cards
        public int RitoPoints { get; private set; }
        public int EloPoints { get; private set; }
        public int Victories { get; private set; } = 0;
        public int Defeats { get; private set; } = 0;
        public int Draws { get; private set; } = 0;

        // Stack: alle verfügbaren Karten
        public List<Card>? Stack { get; private set; }

        // Deck: 4 ausgewählte Karten für den Kampf
        public List<Card>? Deck { get; private set; }

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
                
        }

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

        public void EditUsername(string newName)
        {
            Username = newName;
        }

        public void EditBio(string newBio)
        {
            Bio = newBio;
        }

        public void EditImage(string newImage)
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
