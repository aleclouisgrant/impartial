using System;

namespace Impartial
{
    public interface IUser
    {
        Guid UserId { get; set; }

        // personal info
        string FirstName { get; set; }
        string LastName { get; set; }
        string FullName => LastName == string.Empty ? FirstName : FirstName + " " + LastName;
    }

    public abstract class UserBase : IUser 
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => LastName == string.Empty ? FirstName : FirstName + " " + LastName;
    }
}