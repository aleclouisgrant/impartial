using System;
using System.Collections.Generic;

namespace Impartial
{
    public interface IDanceConvention
    {
        public Guid Id { get; }
        public string Name { get; }
        public DateTime Date { get; }
        public List<ICompetition> Competitions { get; set; }
    }
}
