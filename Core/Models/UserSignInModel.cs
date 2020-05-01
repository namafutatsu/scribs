using System.ComponentModel.DataAnnotations;

namespace Scribs.Core.Models {
    public class UserSignInModel {
        [Required(ErrorMessage = "Login is required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string Token { get; set; }
    }
}
