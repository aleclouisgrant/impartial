using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Impartial
{
    public class Competitor : IPersonModel
    {
        [BsonId]
        public Guid Id { get; set; }

        // personal info
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => LastName == string.Empty ? FirstName : FirstName + " " + LastName;

        public Competitor(string firstName, string lastName)
        {
            this.FirstName = firstName;
            this.LastName = lastName;
        }
    }
}
