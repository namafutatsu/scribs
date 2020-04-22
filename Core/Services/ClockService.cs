using System;

namespace Scribs.Core.Services {

    public class ClockService {
        public virtual DateTime GetNow() => DateTime.Now.ToUniversalTime();
    }
}