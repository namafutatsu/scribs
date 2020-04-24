using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Scribs.Core.Models {
    public class DocumentModel {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Index { get; set; }
        public bool IndexNodes { get; set; }
        public bool IndexLeaves { get; set; }
        public ObservableCollection<DocumentModel> Children { get; set; }
        public bool IsLeaf { get; set; }
    }
}
