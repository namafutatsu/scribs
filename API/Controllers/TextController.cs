using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scribs.Core.Entities;
using Scribs.Core.Services;

namespace Scribs.API.Controllers {

    [Route("api/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class TextController : ControllerBase {
        private readonly IMapper mapper;
        private readonly AuthService auth;
        private readonly Factory<Text> factory;

        public TextController(AuthService auth, IMapper mapper, Factory<Text> factory) {
            this.auth = auth;
            this.mapper = mapper;
            this.factory = factory;
        }

        [HttpPost]
        public async Task<ActionResult> Get(TextModel model) {
            var user = await auth.Identify(User);
            var text = await factory.GetAsync(model.Id);
            if (text == null || text.UserId != user.Id)
                return Problem($"Project {model.Id} not found for user {user.Name}");
            return Ok(mapper.Map<TextModel>(text));
        }

        [HttpPost]
        public async Task<ActionResult> Post(TextModel model) {
            var user = await auth.Identify(User);
            var text = mapper.Map<Text>(model);
            await factory.CreateAsync(text);
            return Ok();
        }

        [HttpPost]
        public async Task<ActionResult> Update(TextModel model) {
            var user = await auth.Identify(User);
            var text = mapper.Map<Text>(model);
            await factory.UpdateAsync(text);
            return Ok();
        }
    }
}
