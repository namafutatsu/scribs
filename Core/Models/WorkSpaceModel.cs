using System.Collections.Generic;

namespace Scribs.Core.Models {
    public class WorkspaceModel {
        public DocumentModel Project { get; set; }
        public IDictionary<string, string> Texts { get; set; }
    }
}
