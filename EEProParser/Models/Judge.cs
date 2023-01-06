using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Impartial
{
    public class Judge : PersonModel
    {
        // personal data
        public string FullName => LastName == string.Empty ? FirstName : FirstName + " " + LastName;

        // data
        public List<Score> Scores { get; set; }

        // total accuracy 
        public double Accuracy => Scores == null ? 0 : Math.Round(Scores.Sum(s => s.Accuracy) / Scores.Count, 2);
        
        public double Top5Accuracy
        {
            get
            {
                if (Scores == null)
                    return 0;

                var scores = Scores.FindAll(s => s.ActualPlacement <= 5);
                return Math.Round(scores.Sum(s => s.Accuracy) / scores.Count, 2);
            }
        }

        public Judge(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
