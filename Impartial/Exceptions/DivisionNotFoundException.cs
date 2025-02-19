using System;

namespace Impartial
{
    public class DivisionNotFoundException : Exception
    {
        public Division DivisionNotFound { get; }

        public DivisionNotFoundException(Division divisionNotFound) : base(ConvertToMessage(divisionNotFound))
        {
            DivisionNotFound = divisionNotFound;
        }

        private static string ConvertToMessage(Division divisionNotFound)
        {
            return "Could not find " + divisionNotFound.ToString() + " in scoresheet.";
        }
    }
}
