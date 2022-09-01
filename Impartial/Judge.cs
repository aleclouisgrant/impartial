using System.Collections.Generic;
using System.Linq;

namespace Impartial
{    
    public class Judge
    {
        // personal info
        public string FirstName { get; set; }
        public string LastName { get; set; }

        // data
        public List<Score> Scores { get; set; }

        // total accuracy 
        public double Accuracy { get; set; }
        
        public Judge(string firstName)
        {
            FirstName = firstName;
        }
        public Judge(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }
    }

    public class Competitor
    {
        // personal info
        public string FirstName { get; set; }
        public string LastName { get; set; }

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
