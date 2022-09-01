using System;
using System.Collections.Generic;

namespace Impartial
{
    public class SdcEvent
    {
        public Guid Id { get; }
        public string Name { get; }
        public DateTime Date { get; }
        public List<Competition> Competitions { get; set; }

        public SdcEvent(string name, DateTime date)
        {
            Name = name; Date = date;
            Competitions = new List<Competition>();
        }
    }
}
