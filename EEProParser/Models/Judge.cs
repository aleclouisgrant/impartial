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
        public double Accuracy { get; set; }

        public double Top5Accuracy { get; set; }

        public Judge(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }
    }
}
