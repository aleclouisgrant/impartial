using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Impartial
{
    public class Competitor
    {
        [BsonId]
        public Guid Id { get; set; }

        // personal info
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => FirstName + " " + LastName; 

        public Competitor(string firstName)
        {
            FirstName = firstName;
        }

        public Competitor(string firstName, string lastName)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
        }
    }
}
