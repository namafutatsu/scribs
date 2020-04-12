using MongoDB.Bson;

namespace Scribs.Core {
    public static class Utils {
        public static string CreateGuid() => ObjectId.GenerateNewId().ToString();//Guid.NewGuid().ToString();
        public static string CleanName(string name) => name;
    }
}
