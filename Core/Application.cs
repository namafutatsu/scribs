using System;

namespace Scribs.Core {
    class Application : IDisposable {
        public Storage Storage { get; private set; }
        private static Application current;
        public static Application Current {
            get {
                if (current == null)
                    current = new Application(new DiskStorage("C:/Storage/Disk"));
                return current;
            }
        }

        public Application(Storage storage) {
            Storage = storage;
        }

        public void Dispose() {
            throw new NotImplementedException();
        }
    }
}
