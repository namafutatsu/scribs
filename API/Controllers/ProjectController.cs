using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scribs.Core;
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
        private readonly Factories factories;
        private readonly PandocService pandoc;

        public ProjectController(AuthService auth, IMapper mapper,  MongoStorage storage, Factories factories, PandocService pandoc) {
            this.auth = auth;
            this.mapper = mapper;
            this.storage = storage;
            this.factories = factories;
            this.pandoc = pandoc;
        }

        [HttpGet]
        public async Task<ActionResult> GetList() {
            var user = await auth.Identify(User);
            var projects = await factories.Get<Document>().GetAsync(o => o.UserName == user.Name);
            var result = projects.Select(o => mapper.Map<DocumentModel>(o));
            return Ok(result);
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

        [HttpPost]
        public async Task<ActionResult> Post(DocumentModel model) {
            var user = await auth.Identify(User);
            Document project;
            if (String.IsNullOrEmpty(model.Id)) {
                string name = model.Name.Trim();
                if (String.IsNullOrWhiteSpace(name))
                    return BadRequest($"A name is required");
                project = await storage.LoadAsyncByName(user, name, false);
                if (project != null)
                    return BadRequest($"A project with the name {project.Name} already exists for user {user.Name}");
                model.Id = Utils.CreateId();
            }
            project = mapper.Map<Document>(model);
            project.UserName = user.Name;
            await storage.SaveAsync(project, false);
            return Ok(project);
        }
    }
}
