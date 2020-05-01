using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scribs.Core.Entities;
using Scribs.Core.Models;
using Scribs.Core.Services;

namespace Scribs.API.Controllers {

    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase {
        private readonly IMapper mapper;
        private readonly Factory<User> factory;

        public UserController(IMapper mapper, Factory<User> factory) {
            this.mapper = mapper;
            this.factory = factory;
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserRegistrationModel userModel) {
            if (!ModelState.IsValid)
                return BadRequest("Model state issue");
            string name = userModel.Name;
            if (await factory.GetByNameAsync(name) != null)
                return Problem("User already exists");
            var user = mapper.Map<User>(userModel);
            try {
                await factory.CreateAsync(user);
            } catch {
                return BadRequest("A problem occured, please retry later");
            }
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Login(UserSignInModel userModel) {
            if (!ModelState.IsValid)
                return BadRequest("Model state issue");
            var user = await factory.GetByNameAsync(userModel.Name);
            if (user == null)
                return BadRequest("User not found");
            if (userModel.Password != user.Password)
                return BadRequest("Incorrect password");
            var token = AuthService.GenerateToken(user.Id);
            userModel.Token = token;
            return Ok(userModel);
        }

        private async Task<User> Identify(ClaimsPrincipal principal) {
            var user = await factory.GetAsync(User.Identity.Name);
            if (user == null)
                throw new System.Exception("User not found");
            return user;
        }

        [HttpGet]
        [Authorize]
        public async Task<string> Mail() {
            var user = await Identify(User);
            return user.Mail;
        }

        [HttpGet("{id}", Name = "Get")]
        public Task<User> Get(string id) {
            return factory.GetAsync(id);
        }

        [HttpGet("{name}", Name = "GetByName")]
        public Task<User> GetByName(string name) {
            return factory.GetByNameAsync(name);
        }

        public bool IsPasswordValid(string password) {
            if (password.Length < 4)
                return false;
            return true;
        }

        public bool IsUsernameValid(string username) {
            if (username.Length < 4)
                return false;
            var regex = new Regex("^[a-zA-Z0-9 ]*$");
            if (regex.IsMatch(username))
                return false;
            return true;
        }
    }
}
