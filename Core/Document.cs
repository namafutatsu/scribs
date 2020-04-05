﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Scribs.Core {

    [DataContract]
    public class Document {
        [DataMember] public string Key { get; set; }
        [DataMember] public string Name { get; set; }
        [DataMember] public ObservableCollection<Document> Documents { get; set; }
        [DataMember] public int Index { get; set; }
        //[DataMember]
        public string Text { get; set; }

        public bool IsLeaf => Documents == null;
        public Document Parent { get; protected set; }
        public Document Project { get; protected set; }
        public User User { get; private set; }
        public string Path => System.IO.Path.Join(Parent != null ? Parent.Path : User.Path, Name);

        // Project
        public IDictionary<string, Document> AllDocuments { get; set; }
        [DataMember] public string Repo { get; set; }

        public Document(string key, string name, User user, Document parent) {
            Key = key;
            Name = name;
            User = user;
            SetParent(parent, false);
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
                return;
            }
            Parent = parent;
            Project = parent.Project;
            User = parent.User;
            if (Project.AllDocuments == null)
                Project.AllDocuments = new Dictionary<string, Document>();
            Project.AllDocuments.Add(Key, this);
            if (recurrence && Documents != null)
                foreach (var child in Documents)
                    child.SetParent(this, true);
        }

        public void OrderDocument(Document document, int index) {
            var oldIndex = Documents.IndexOf(document);
            Documents.Move(oldIndex, index);
        }

        public IDictionary<string, string> Metadata {
            get {
                var metadata = new Dictionary<string, string> {
                    ["id"] = Key
                };
                if (Repo != null)
                    metadata.Add("repo", Repo);
                return metadata;
            }
        }
    }
}
