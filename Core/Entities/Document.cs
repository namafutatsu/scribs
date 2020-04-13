﻿using System.Collections.Generic;
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
        public ObservableCollection<Document> Documents { get; set; }
        [BsonElement("Index")]
        [DataMember]
        public int Index { get; set; }
        [BsonElement("IndexNodes")]
        [DataMember]
        public bool? IndexNodes { get; set; }
        [BsonElement("IndexLeaves")]
        [DataMember]
        public bool? IndexLeaves { get; set; }
        public string Text { get; set; }

        public bool IsLeaf => Documents == null;
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
            if (Documents == null)
                Documents = new ObservableCollection<Document>();
            Documents.Add(document);
            return document;
        }

        public static void BuildProject(Document project, User user) {
            project.Project = project;
            project.User = user;
            foreach (var child in project.Documents) {
                child.SetParent(project, true);
            }
        }

        private void SetParent(Document parent, bool recurrence) {
            if (parent == null) {
                Project = this;
                if (!IndexNodes.HasValue)
                    IndexNodes = false;
                if (!IndexLeaves.HasValue)
                    IndexLeaves = true;
                return;

            }
            Parent = parent;
            if (!IndexNodes.HasValue)
                IndexNodes = parent.IndexNodes;
            if (!IndexLeaves.HasValue)
                IndexLeaves = parent.IndexLeaves;
            Project = parent.Project;
            User = parent.User;
            if (Project.AllDocuments == null)
                Project.AllDocuments = new Dictionary<string, Document> {
                    [Project.Key] = Project
                };
            Project.AllDocuments.Add(Key, this);
            if (recurrence && Documents != null)
                foreach (var child in Documents)
                    child.SetParent(this, true);
        }

        public void OrderDocument(Document document, int index) {
            var oldIndex = Documents.IndexOf(document);
            Documents.Move(oldIndex, index);
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
            if (!NoMetadata && !other.NoMetadata && (Key != other.Key || Index != other.Index))
                return false;
            if (Documents?.Count != other.Documents?.Count)
                return false;
            if (Documents != null) 
                foreach (var document in Documents) {
                    var otherDocument = other.Documents?.SingleOrDefault(o => o.Name == document.Name);
                    if (otherDocument == null || !document.Equals(otherDocument))
                        return false;
                }
            return true;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public IDictionary<string, string> Metadata {
            get {
                var metadata = new Dictionary<string, string> {
                    ["id"] = Key
                };
                if (Repo != null)
                    metadata.Add("repo", Repo);
                if (!IsLeaf) {
                    metadata.Add("index.nodes", IndexNodes.ToString());
                    metadata.Add("index.leaves", IndexLeaves.ToString());
                }
                return metadata;
            }
        }
    }
}
