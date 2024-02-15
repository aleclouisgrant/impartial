using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Impartial
{
    public class Judge : PersonModel
    {
        public string FullName => LastName == string.Empty ? FirstName : FirstName + " " + LastName;

        public List<FinalScore> Scores { get; set; }

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

        public Judge(string firstName, string lastName, Guid? id = null)
        {
            if (id == null)
                Id = Guid.NewGuid();
            else
                Id = (Guid)id;

            FirstName = firstName;
            LastName = lastName;
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
