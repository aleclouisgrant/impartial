using Impartial;
using System;
using System.Collections.Generic;

namespace ImpartialUI
{
    public class DanceConvention : IDanceConvention
    {
        public Guid Id { get; }
        public string Name { get; }
        public DateTime Date { get; }
        public List<ICompetition> Competitions { get; set; }

        public DanceConvention(string name, DateTime date, Guid? id = null)
        {
            if (id == null)
                Id = Guid.NewGuid();
            else
                Id = (Guid)id;

            Name = name;
            Date = date;

            Competitions = new List<ICompetition>();
        }
    }
}
