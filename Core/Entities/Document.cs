using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Scribs.Core.Entities {

    [DataContract, BsonIgnoreExtraElements]
    public class Document: Entity {
        [BsonElement, DataMember(EmitDefaultValue = false)]
        public ObservableCollection<Document> Children { get; set; }
        [BsonElement, DataMember]
        public int Index { get; set; }
        [BsonElement, DataMember]
        public bool IndexNodes { get; set; } = false;
        [BsonElement, DataMember]
        public bool IndexLeaves { get; set; } = false;
        [BsonIgnore]
        public string Content { get; set; }
        [BsonIgnore]
        public Document Parent { get; protected set; }
        [BsonIgnore]
        public Document Project { get; protected set; }
        [BsonIgnore]
        public User User { get; private set; }
        [BsonIgnore]
        public bool Disconnect { get; set; } = false;
        [BsonIgnore]
        public bool NoMetadata { get; set; } = false;
        public string Path => System.IO.Path.Join(Parent != null ? Parent.Path : User.Path, Name);
        public bool IsLeaf => Children == null;

        // Project
        [BsonIgnore]
        public IDictionary<string, Document> AllDocuments { get; set; }
        [BsonElement, DataMember]
        public string UserName { get; set; }
        [BsonElement, DataMember(EmitDefaultValue = false)]
        public string Repo { get; set; }

        public Document(string name, User user, Document parent = null, string key = null) {
            Id = key ?? Utils.CreateGuid();
            NoMetadata = key == null;
            Name = name;
            UserName = user?.Name;
            User = user;
            SetParent(parent, false);
        }

        public Document CreateDocument(string name, string text = null, int? index = null) {
            var document = new Document(name, User, this);
            if (text != null)
                document.Content = text;
            if (index.HasValue)
                document.Index = index.Value;
            if (Children == null)
                Children = new ObservableCollection<Document>();
            Children.Add(document);
            return document;
        }

        public static void BuildProject(Document project, User user) {
            project.Project = project;
            project.User = user;
            foreach (var child in project.Children) {
                child.SetParent(project, true);
            }
        }

        private void SetParent(Document parent, bool recurrence) {
            if (parent == null) {
                Project = this;
                return;
            }
            Parent = parent;
            Project = parent.Project;
            User = parent.User;
            if (Project.AllDocuments == null)
                Project.AllDocuments = new Dictionary<string, Document> {
                    [Project.Id] = Project
                };
            Project.AllDocuments.Add(Id, this);
            if (recurrence && Children != null)
                foreach (var child in Children)
                    child.SetParent(this, true);
        }

        public void OrderDocument(Document document, int index) {
            var oldIndex = Children.IndexOf(document);
            Children.Move(oldIndex, index);
        }

        public override bool Equals(object obj) {
            var other = obj as Document;
            if (Name != other.Name ||
                User.Name != other.User.Name ||
                Content != other.Content ||
                IndexNodes != other.IndexNodes ||
                IndexLeaves != other.IndexLeaves ||
                UserName != other.UserName)
                return false;
            if (!NoMetadata &&
                !other.NoMetadata &&
                Metadatas.Count(m => m.Get(this) != null) > 1 &&
                Metadatas.Count(m => m.Get(other) != null) > 1 &&
                (Id != other.Id || Index != other.Index))
                return false;
            if (Children?.Count != other.Children?.Count)
                return false;
            if (Children != null) 
                foreach (var document in Children) {
                    var otherDocument = other.Children?.SingleOrDefault(o => o.Name == document.Name);
                    if (otherDocument == null || !document.Equals(otherDocument))
                        return false;
                }
            return true;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        private static List<Metadata> metadatas;
        public static List<Metadata> Metadatas {
            get {
                if (metadatas == null)
                    metadatas = new List<Metadata> {
                    new Metadata("id", Utils.CreateGuid, d => d.Id, (d, m) => d.Id = m),
                    new Metadata("repo", () => null, d => String.IsNullOrEmpty(d.Repo) ? null : d.Repo, (d, m) => d.Repo = m),
                    new Metadata("index.nodes", () => false.ToString(),
                        d => d.IndexNodes ? true.ToString() : null,
                        (d, m) => {
                            if (m != null)
                                d.IndexNodes = bool.Parse(m);
                        }
                    ),
                    new Metadata("index.leaves", () => false.ToString(),
                        d => d.IndexLeaves ? true.ToString() : null,
                        (d, m) => {
                            if (m != null)
                                d.IndexLeaves = bool.Parse(m);
                        }
                    )
                };
                return metadatas;
            }
        }
    }

    public class Metadata {
        public string Id { get; }
        public Func<string> Default { get; }
        public Func<Document, string> Get { get; }
        public Action<Document, string> Set { get; }

        public Metadata(string id, Func<string> @default, Func<Document, string> get, Action<Document, string> set) {
            Id = id;
            Default = @default;
            Get = get;
            Set = set;
        }
    }
}
