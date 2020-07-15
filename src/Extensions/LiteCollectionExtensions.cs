using LiteDB;

namespace WebRequest.Elegant.Offline.Extensions
{
    public static class LiteCollectionExtensions
    {
        public static void InsertOrUpdate<T>(this ILiteCollection<T> liteCollection, BsonValue id, T item)
        {
            var existingResponseData = liteCollection.FindById(id);

            if (existingResponseData != null)
            {
                liteCollection.Update(item);
            }
            else
            {
                liteCollection.Insert(item);
            }
        }
    }
}
