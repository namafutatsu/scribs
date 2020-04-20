using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scribs.API.Models;
using Scribs.Core.Entities;
using Scribs.Core.Services;
using Scribs.Core.Storages;

namespace Scribs.API.Controllers {

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProjectController : ControllerBase {
        private readonly IMapper mapper;
        private readonly Factories factories;
        private readonly MongoStorage storage;
        private Factory<Document> Factory => factories.Get<Document>();

        public ProjectController(IMapper mapper, Factories factories, MongoStorage storage) {
            this.mapper = mapper;
            this.factories = factories;
            this.storage = storage;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Get(ProjectModel projectModel) {
            var user = await User.Identify(factories);
            var project = storage.Load(user.Name, projectModel.Name);
            if (project == null)
                return Problem($"Project {projectModel.Name} not found for user {user.Name}");
            var model = mapper.Map<ProjectModel>(project);
            return Ok(model);
        }
    }
}
