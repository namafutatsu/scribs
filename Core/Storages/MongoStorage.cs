using System;
using System.Threading.Tasks;
using Scribs.Core.Entities;
using Scribs.Core.Services;

namespace Scribs.Core.Storages {
    public class MongoStorage : IStorage {
        // todo mass loading and saving

        private Factories factories;

        public MongoStorage(Factories factories) {
            this.factories = factories;
        }

        public Document Load(string userName, string name) => LoadAsync(userName, name, true).Result;
        public void Save(Document project) => SaveAsync(project, true).Wait();

        private async Task<User> GetUserAsync(string name) {
            var user = await factories.Get<User>().GetByNameAsync(name);
            if (user == null)
                throw new Exception($"User {name} not found");
            return user;
        }

        private async Task<Document> GetProjectAsync(Func<Factory<Document>, Task<Document>> get, User user) {
            var project = await get(factories.Get<Document>());
            if (project == null)
                return null;
            if (project.UserName != user.Name)
                throw new Exception($"Project {project.Id} named {project.Name} not belonging to user {user.Id}");
            Document.BuildProject(project, user);
            return project;
        }

        private Task<Document> GetProjectByIdAsync(string id, User user) => GetProjectAsync(o => o.GetAsync(id), user);

        private Task<Document> GetProjectByNameAsync(string name, User user) => GetProjectAsync(o => o.GetByNameAsync(name), user);

        private async Task<Text> GetTextAsync(string id, User user, Document project) {
            var text = await factories.Get<Text>().GetAsync(id);
            if (text == null)
                return null;
            if (text.UserId != user.Id)
                throw new Exception($"Text {text.Id} not belonging to user {user.Id}");
            if (text.ProjectId != project.Id)
                throw new Exception($"Text {text.Id} not belonging to project {project.Id}");
            return text;
        }

        public async Task<Document> LoadAsync(string userName, string name, bool content = true) {
            var user = await GetUserAsync(userName);
            return await LoadAsync(user, name, content);
        }

        public async Task<Document> LoadAsync(User user, string name, bool content = true) {
            var project = await GetProjectByNameAsync(name, user);
            if (project == null) {
                throw new Exception($"Project {name} not found");
            }
            if (content) {
                foreach (var kvp in project.ProjectDocuments) {
                    var text = await GetTextAsync(kvp.Key, user, project);
                    kvp.Value.Content = text?.Content;
                };
            }
            return project;
        }

        private Task CreateProjectAsync(Document project) => factories.Get<Document>().CreateAsync(project);

        private Task UpdateProjectAsync(Document project)  => factories.Get<Document>().UpdateAsync(project);

        private Task CreateTextAsync(User user, Document project, Document document) {
            var text = new Text(user.Id, project.Id, document);
            return factories.Get<Text>().CreateAsync(text);
        }

        private Task UpdateTextAsync(Text text, string content) {
            text.Content = content;
            return factories.Get<Text>().UpdateAsync(text);
        }

        public async Task SaveAsync(Document project, bool content) {
            var user = await GetUserAsync(project.UserName);
            var saved = await GetProjectByNameAsync(project.Name, user) ?? await GetProjectByIdAsync(project.Id, user);
            if (saved == null)
                await CreateProjectAsync(project);
            else
                await UpdateProjectAsync(project);
            if (content) {
                foreach (var kvp in project.ProjectDocuments) {
                    var text = await GetTextAsync(kvp.Key, user, project);
                    if (text == null)
                        await CreateTextAsync(user, project, kvp.Value);
                    else
                        await UpdateTextAsync(text, kvp.Value.Content);
                }
            }
        }
    }
}