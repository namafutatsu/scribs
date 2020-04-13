using System;
using System.Linq;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace Scribs.Core.Services {

    public class RepositoryService {
        private Signature signature;
        private UsernamePasswordCredentials credentials;
        private string ownerUrl;
        public CredentialsHandler CredentialsHandler => new CredentialsHandler((url, usernameFromUrl, types) => credentials);

        public RepositoryService(IRepositorySettings settings) {
            if (String.IsNullOrEmpty(settings.Username))
                return;
            signature = new Signature(new Identity(settings.Username, settings.Mail), DateTimeOffset.Now);
            credentials = new UsernamePasswordCredentials() {
                Username = settings.Owner,
                Password = settings.Password
            };
            ownerUrl = settings.OwnerUrl;
        }        

        public bool IsRepo(string path) {
            return Repository.IsValid(path);
        }

        public string Url(string path) {
            using (var repo = new Repository(path)) {
                return repo.Config.Get<string>("remote", "origin", "url").Value; 
            }
        }

        public void Commit(string path, string message) {
            using (var repo = new Repository(path)) {
                var changes = repo.Diff.Compare<TreeChanges>();
                if (changes.Any()) {
                    Commands.Stage(repo, "*");
                    repo.Commit(message, signature, signature);
                    repo.Network.Push(repo.Branches["master"], new PushOptions {
                        CredentialsProvider = CredentialsHandler
                    });
                }
            }
        }

        public void Clone(string repoName, string path) {
            Repository.Clone(ownerUrl + repoName, path, new CloneOptions {
                CredentialsProvider = CredentialsHandler
            });
        }

        public void Pull(string path) {
            using (var repo = new Repository(path)) {
                Commands.Pull(repo, signature, new PullOptions {
                    FetchOptions = new FetchOptions {
                        CredentialsProvider = CredentialsHandler
                    }
                });
            }
        }
    }

    public class RepositorySettings : IRepositorySettings {
        public string Owner { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public string Mail { get; set; }
        public string OwnerUrl { get; set; }
    }

    public interface IRepositorySettings {
        string Owner { get; set; }
        string Password { get; set; }
        public string Username { get; set; }
        public string Mail { get; set; }
        public string OwnerUrl { get; set; }
    }
}