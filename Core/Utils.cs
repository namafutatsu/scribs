using System;

namespace Scribs.Core {
    public static class Utils {
        public static string CreateGuid() => Guid.NewGuid().ToString();

        public static string CleanName(string name) => name;
    }
}
