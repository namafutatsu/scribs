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
    public class ProjectController : ControllerBase {
        private readonly IMapper mapper;
        private readonly AuthService auth;
        private readonly MongoStorage storage;

        public ProjectController(AuthService auth, IMapper mapper,  MongoStorage storage) {
            this.auth = auth;
            this.mapper = mapper;
            this.storage = storage;
        }

        [HttpPost]
        public async Task<ActionResult> Get(DocumentModel model) {
            var user = await auth.Identify(User);
            var project = storage.Load(user.Name, model.Name);
            if (project == null)
                return Problem($"Project {model.Name} not found for user {user.Name}");
            return Ok(mapper.Map<DocumentModel>(project));
        }

        [HttpPost]
        public async Task<ActionResult> Post(DocumentModel model) {
            var user = await auth.Identify(User);
            var project = mapper.Map<Document>(model);
            project.UserName = user.Name;
            await storage.SaveAsync(project, false);
            return Ok(project);
        }
    }
}
