using Octokit;
using Scribs.Core.Entities;
using System;

namespace Scribs.Core.Services {
    public class GitHubService {
        private GitHubClient gitHubClient;
        public GitHubService(IGitHubSettings settings) {
            if (String.IsNullOrEmpty(settings.Username))
                return;
            var basicAuth = new Credentials(settings.Username, settings.Password);
            gitHubClient = new GitHubClient(new ProductHeaderValue("scribs"));
            gitHubClient.Credentials = basicAuth;
        }

        public string GetRepoName(Document project) => $"scribs_{project.User.Name}_{project.Name}";

        public void Create(string repoName) {
            gitHubClient.Repository.Create(new NewRepository(repoName) { Private = true, AutoInit = true }).Wait();
        }

        public void Create(Document project) => Create(GetRepoName(project));

        private void Delete(string repoName) {
            try {
                var repo = gitHubClient.Repository.Get(gitHubClient.Credentials.Login, repoName).Result;
                gitHubClient.Repository.Delete(repo.Id).Wait();
            } catch { }
        }

        public void Delete(Document project) => Delete(GetRepoName(project));

        public bool Exists(string repoName) => gitHubClient.Repository.Get(gitHubClient.Credentials.Login, repoName) != null;

        public bool Exists(Document project) => Exists(GetRepoName(project));
    }

    public class GitHubSettings : IGitHubSettings {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public interface IGitHubSettings {
        string Username { get; set; }
        string Password { get; set; }
    }
}
