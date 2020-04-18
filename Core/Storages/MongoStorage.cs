using System;
using Scribs.Core.Entities;
using Scribs.Core.Services;

namespace Scribs.Core.Storages {
    public class MongoStorage : IStorage {
        private Factories factories;

        public MongoStorage(Factories factories) {
            this.factories = factories;
        }

        public Document Load(string userName, string name) => Load(userName, name, true);
        public void Save(Document project) => Save(project, true);

        private User GetUser(string name) {
            var user = factories.Get<User>().GetByName(name);
            if (user == null)
                throw new Exception($"User {name} not found");
            return user;
        }

        private Document GetProject(string name, User user) {
            var project = factories.Get<Document>().GetByName(name);
            if (project == null)
                return null;
            if (project.UserName != user.Name)
                throw new Exception($"Project {project.Id} not belonging to user {user.Id}");
            Document.BuildProject(project, user);
            return project;
        }

        private Text GetText(string id, User user, Document project) {
            var text = factories.Get<Text>().Get(id);
            if (text == null)
                return null;
            if (text.UserId != user.Id)
                throw new Exception($"Text {text.Id} not belonging to user {user.Id}");
            if (text.ProjectId != project.Id)
                throw new Exception($"Text {text.Id} not belonging to project {project.Id}");
            return text;
        }


        public Document Load(string userName, string name, bool content = true) {
            var user = GetUser(userName);
            var project = GetProject(name, user);
            if (project == null) {
                throw new Exception($"Project {name} not found");
            }
            if (content) {
                foreach (var kvp in project.AllDocuments)
                    kvp.Value.Content = GetText(kvp.Key, user, project).Content;
            }
            return project;
        }

        private void CreateProject(Document project) {
            factories.Get<Document>().Create(project);
        }

        private void UpdateProject(Document project) {
            factories.Get<Document>().Update(project.Id, project); // todo virer id
        }

        private void CreateText(User user, Document project, string content) {
            var text = new Text(user.Id, project.Id, content);
            factories.Get<Text>().Create(text);
        }

        private void UpdateText(Text text, string content) {
            text.Content = content;
            factories.Get<Text>().Update(text.Id, text); // todo virer id
        }

        public void Save(Document project, bool content) {
            var user = GetUser(project.UserName);
            var saved = GetProject(project.Name, user);
            if (saved == null)
                CreateProject(project);
            else
                UpdateProject(project);
            if (content) {
                foreach (var kvp in project.AllDocuments) {
                    var text = GetText(kvp.Key, user, project);
                    if (text == null)
                        CreateText(user, project, kvp.Value.Content);
                    else
                        UpdateText(text, kvp.Value.Content);
                }
            }
        }
    }
}