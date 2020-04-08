using Octokit;

namespace Scribs.Core.Services {
    public class GitHubService {
        private GitHubClient gitHubClient;

        public GitHubService(IGitHubSettings settings) {
            var basicAuth = new Octokit.Credentials(settings.Username, settings.Password);
            gitHubClient = new GitHubClient(new ProductHeaderValue("scribs"));
            gitHubClient.Credentials = basicAuth;
        }

        public void Create(string repoName) {
            gitHubClient.Repository.Create(new NewRepository(repoName) { Private = true, AutoInit = true }).Wait();
        }
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
