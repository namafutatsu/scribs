using System.ComponentModel.DataAnnotations;

namespace Scribs.API.Models {
    public class UserSignInModel {
        [Required(ErrorMessage = "Login is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
