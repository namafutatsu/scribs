using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scribs.API.Models;
using Scribs.Core.Entities;
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
            if (!ModelState.IsValid) {
                return Problem("Model state issue"); ;
            }
            string name = userModel.Name;
            if (await factory.GetByNameAsync(name) != null) {
                return Problem("User already exists");
            }
            var user = mapper.Map<User>(userModel);
            try {
                await factory.CreateAsync(user);
            } catch {
                return Problem("A problem occured, please retry later");
            }
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(UserSignInModel userModel) {
            if (!ModelState.IsValid) {
                return Problem("Model state issue");
            }
            var user = await factory.GetByNameAsync(userModel.Name);
            if (user == null) {
                return Problem("User not found");
            }
            if (userModel.Password != user.Password) {
                return Problem("Incorrect password");
            }
            var token = JwtManager.GenerateToken(user.Id);
            return Ok(token);
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
        public User Get(string id) {
            return factory.Get(id);
        }

        [HttpGet("{name}", Name = "GetByName")]
        public User GetByName(string name) {
            return factory.GetByName(name);
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

        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value) {
        //}

        //[HttpDelete("{id}")]
        //public void Delete(string id) {
        //    factory.Remove(id);
        //}
    }
}
