﻿using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Impartial
{
    public interface IPersonModel
    {
        [BsonId]
        Guid Id { get; set; }

        // personal info
        string FirstName { get; set; }
        string LastName { get; set; }
        string FullName => LastName == string.Empty ? FirstName : FirstName + " " + LastName;
    }
}