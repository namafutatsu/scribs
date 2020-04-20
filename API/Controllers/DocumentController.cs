//using System.Collections.Generic;
//using Scribs.Core.Entities;
//using Scribs.Core.Services;

//namespace Scribs.API.Controllers {

//    [Route("api/[controller]/[action]")]
//    [ApiController]
//    public class DocumentController : ControllerBase {

//        private readonly Factory<Document> factory;

//        public DocumentController(Factory<Document> factory) => this.factory = factory;

//        [HttpGet]
//        public List<User> Get() {
//            return factory.Get();
//        }

//        [HttpGet("{id}", Name = "Get")]
//        public User Get(string id) {
//            return factory.Get(id);
//        }

//        [HttpGet("{name}", Name = "GetByName")]
//        public User GetByName(string name) {
//            return factory.GetByName(name);
//        }

//        //[HttpPost]
//        //public void Post([FromBody] string value) {
//        //}

//        //[HttpPut("{id}")]
//        //public void Put(int id, [FromBody] string value) {
//        //}

//        [HttpDelete("{id}")]
//        public void Delete(string id) {
//            factory.Remove(id);
//        }
//    }
//}
