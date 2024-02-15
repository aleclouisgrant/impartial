using System;
using System.Collections.Generic;

namespace Impartial
{
    public class DanceConvention
    {
        public Guid Id { get; }
        public string Name { get; }
        public DateTime Date { get; }
        public List<Competition> Competitions { get; set; }

        public DanceConvention(string name, DateTime date, Guid? id = null)
        {
            if (id == null)
                Id = Guid.NewGuid();
            else
                Id = (Guid)id;

            Name = name; 
            Date = date;

            Competitions = new List<Competition>();
        }
    }
}
