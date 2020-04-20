using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
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
            string name = userModel.Name;
            if (await factory.GetByNameAsync(name) != null) {
                return Problem("User already exists");
            }
            var user = mapper.Map<User>(userModel);
            try {
                await factory.CreateAsync(user);
            } catch {
                return Problem("A problem occured. Please retry later.");
            }
            return Ok();
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
