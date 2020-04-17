using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Scribs.Core.Entities {

    [DataContract]
    public class Document: Entity {
        [BsonElement("Documents")]
        [DataMember(EmitDefaultValue = false)]
        public ObservableCollection<Document> Children { get; set; }
        [BsonElement("Index")]
        [DataMember]
        public int Index { get; set; }
        [BsonElement("IndexNodes")]
        [DataMember]
        public bool IndexNodes { get; set; } = false;
        [BsonElement("IndexLeaves")]
        [DataMember]
        public bool IndexLeaves { get; set; } = false;
        public string Text { get; set; }

        public bool IsLeaf => Children == null;
        public Document Parent { get; protected set; }
        public Document Project { get; protected set; }
        public User User { get; private set; }
        public string Path => System.IO.Path.Join(Parent != null ? Parent.Path : User.Path, Name);
        public bool Disconnect { get; set; } = false;

        // Project
        public IDictionary<string, Document> AllDocuments { get; set; }
        [BsonElement("Repo")]
        [DataMember(EmitDefaultValue = false)]
        public string Repo { get; set; }

        public Document(string name, User user, Document parent = null, string key = null) {
            Key = key ?? Utils.CreateGuid();
            NoMetadata = key == null;
            Name = name;
            User = user;
            SetParent(parent, false);
        }

        public Document CreateDocument(string name, string text = null, int? index = null) {
            var document = new Document(name, User, this);
            if (text != null)
                document.Text = text;
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
                    [Project.Key] = Project
                };
            Project.AllDocuments.Add(Key, this);
            if (recurrence && Children != null)
                foreach (var child in Children)
                    child.SetParent(this, true);
        }

        public void OrderDocument(Document document, int index) {
            var oldIndex = Children.IndexOf(document);
            Children.Move(oldIndex, index);
        }

        public bool NoMetadata { get; set; } = false;

        public override bool Equals(object obj) {
            var other = obj as Document;
            if (Name != other.Name ||
                User.Name != other.User.Name ||
                Text != other.Text ||
                IndexNodes != other.IndexNodes ||
                IndexLeaves != other.IndexLeaves)
                return false;
            if (!NoMetadata &&
                !other.NoMetadata &&
                Metadatas.Count(m => m.Get(this) != null) > 1 &&
                Metadatas.Count(m => m.Get(other) != null) > 1 &&
                (Key != other.Key || Index != other.Index))
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
                    new Metadata("id", Utils.CreateGuid, d => d.Key, (d, m) => d.Key = m),
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
