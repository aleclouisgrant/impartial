using Impartial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpartialUI.Models
{
    public class Judge : UserBase, IJudge
    {
        public Guid JudgeId { get; set; }
        public List<IFinalScore> Scores { get; set; }

        public double Accuracy => Scores == null ? 0 : Math.Round(Scores.Sum(s => s.Accuracy) / Scores.Count, 2);

        public double Top5Accuracy
        {
            get
            {
                if (Scores == null)
                    return 0;

                var scores = Scores.FindAll(s => s.Placement <= 5);
                return Math.Round(scores.Sum(s => s.Accuracy) / scores.Count, 2);
            }
        }

        public Judge(string firstName, string lastName, Guid? id = null)
        {
            JudgeId = id ?? Guid.NewGuid();

            FirstName = firstName;
            LastName = lastName;
        }

        public override string ToString()
        {
            return FullName;
        }
    }
}
