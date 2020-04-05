using System;
using System.Linq;
using System.Security;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Octokit;

namespace Scribs.Core {

    public static class Git {
        private static LibGit2Sharp.Signature Signature => new LibGit2Sharp.Signature(new Identity("System", "system@scribs.io"), DateTimeOffset.Now);
        private static UsernamePasswordCredentials credentials;
        private static GitHubClient gitHubClient;
        public static CredentialsHandler CredentialsHandler {
            get {
                if (credentials == null)
                    throw new Exception("No credentials");
                return new CredentialsHandler((url, usernameFromUrl, types) => credentials);
            }
        }

        public static void SetCredentials(string username, string password) {
            var basicAuth = new Octokit.Credentials(username, password);
            gitHubClient = new GitHubClient(new ProductHeaderValue("scribs"));
            gitHubClient.Credentials = basicAuth;
            //user = client.User.Get(username).Result;
            credentials = new UsernamePasswordCredentials() {
                Username = username,
                Password = password
            };
        }

        public static void Create(string repoName) {
            gitHubClient.Repository.Create(new NewRepository(repoName) { Private = true, AutoInit = true }).Wait();
        }

        public static bool IsRepo(string path) {
            try {
                new LibGit2Sharp.Repository(path);
                return true;
            } catch {
                return false;
            }
        }

        public static string Url(string path) {
            using (var repo = new LibGit2Sharp.Repository(path)) {
                return repo.Config.Get<string>("remote", "origin", "url").Value; 
            }
        }

        public static void Commit(string path, string message) {
            using (var repo = new LibGit2Sharp.Repository(path)) {
                var changes = repo.Diff.Compare<TreeChanges>();
                if (changes.Any()) {
                    Commands.Stage(repo, "*");
                    repo.Commit(message, Signature, Signature);
                    repo.Network.Push(repo.Branches["master"], new LibGit2Sharp.PushOptions {
                        CredentialsProvider = CredentialsHandler
                    });
                }
            }
        }

        public static void Clone(string repoName, string path) {
            LibGit2Sharp.Repository.Clone("https://github.com/scribssys/" + repoName, path, new CloneOptions {
                CredentialsProvider = CredentialsHandler
            });
        }

        public static void Pull(string path) {
            using (var repo = new LibGit2Sharp.Repository(path)) {
                Commands.Pull(repo, Signature, new PullOptions {
                    FetchOptions = new FetchOptions {
                        CredentialsProvider = CredentialsHandler
                    }
                });
            }
        }
    }
}