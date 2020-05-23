using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scribs.Core;
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
            if (text == null)
                return NotFound();
            if (text.UserId != user.Id)
                return Unauthorized();
            return Ok(mapper.Map<TextModel>(text).Content);
        }

        [HttpPost]
        public async Task<ActionResult> Post(TextModel model) {
            var user = await auth.Identify(User);
            bool noId = String.IsNullOrEmpty(model.Id);
            var text = mapper.Map<Text>(model);
            text.UserId = user.Id;
            if (noId) {
                model.Id = Utils.CreateId();
                await factory.CreateAsync(text);
            } else {
                var saved = await factory.GetAsync(text.Id);
                if (saved == null) {
                    await factory.CreateAsync(text);
                } else {
                    if (saved.UserId != user.Id)
                        return Unauthorized();
                    await factory.UpdateAsync(text);
                    return Ok(saved);
                }
            }
            return Ok(text.Id);
        }
    }
}
