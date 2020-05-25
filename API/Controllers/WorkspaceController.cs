using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scribs.Core.Entities;
using Scribs.Core.Models;
using Scribs.Core.Services;
using Scribs.Core.Storages;

namespace Scribs.API.Controllers {

    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class WorkspaceController : ControllerBase {
        private readonly IMapper mapper;
        private readonly AuthService auth;
        private readonly MongoStorage storage;
        private readonly Factories factories;
        private readonly PandocService pandoc;

        public WorkspaceController(AuthService auth, IMapper mapper,  MongoStorage storage, Factories factories, PandocService pandoc) {
            this.auth = auth;
            this.mapper = mapper;
            this.storage = storage;
            this.factories = factories;
            this.pandoc = pandoc;
        }

        [HttpPost]
        public async Task<ActionResult> Get(DocumentModel model) {
            var user = await auth.Identify(User);
            var project = await storage.LoadAsyncById(user, model.Id, false) ?? await storage.LoadAsyncByName(user, model.Name, false);
            if (project == null)
                return Problem($"Project {model.Name} not found for user {user.Name}");
            var result = new WorkspaceModel {
                Project = mapper.Map<DocumentModel>(project),
                Texts = new Dictionary<string, string>()
            };
            if (project.ProjectDocuments != null) {
                var textsIds = project.ProjectDocuments.Keys;
                var texts = await factories.Get<Text>().GetAsync(textsIds.ToList());
                foreach (var text in texts)
                    result.Texts.Add(text.Id, text.Content);// pandoc.Convert(text.Content, FileType.markdown, FileType.html));
            }
            return Ok(result);
        }
    }
}
