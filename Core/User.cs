namespace Scribs.Core {
    public class User {
        public string Name { get; private set; }
        public string Path => System.IO.Path.Join("users", Name);
        public string Key => Name;

        public User(string name) {
            Name = name;
        }
    }
}
