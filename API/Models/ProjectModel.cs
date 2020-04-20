using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Scribs.API.Models {
    public class ProjectModel {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        public IList<ProjectModel> Children { get; set; }
    }
}
