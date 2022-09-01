using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace Impartial
{
    public class MongoHelper
    {
        private IMongoDatabase db;

        public MongoHelper(string database)
        {
            var client = new MongoClient();
            db = client.GetDatabase(database);
        }

        public void Insert<T>(string table, T record)
        {
            var collection = db.GetCollection<T>(table);
            collection.InsertOne(record);
        }

        public List<T> LoadAll<T>(string table)
        {
            var collection = db.GetCollection<T>(table);
            return collection.Find(new BsonDocument()).ToList();
        }

        public T LoadById<T>(string table, Guid id)
        {
            var collection = db.GetCollection<T>(table);
            var filter = Builders<T>.Filter.Eq("Id", id);
            return collection.Find(filter).First();
        }

        public void UpsertById<T>(string table, Guid id, T record)
        {
            var collection = db.GetCollection<T>(table);
            var result = collection.ReplaceOne(
                new BsonDocument("_id", id), 
                record,
                new ReplaceOptions { IsUpsert = true });
        }

        public void DeleteById<T>(string table, Guid id)
        {
            var collection = db.GetCollection<T>(table);
            var filter = Builders<T>.Filter.Eq("Id", id);
            collection.DeleteOne(filter);
        }

        public void DeleteAll<T>(string table)
        {
            var collection = db.GetCollection<T>(table);
            var filter = Builders<T>.Filter.Empty;
            collection.DeleteMany(filter);
        }
    }
}
