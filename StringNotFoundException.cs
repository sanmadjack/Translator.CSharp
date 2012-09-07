using System;

namespace Translator {
    public class StringNotFoundException : Exception {
        public StringType type { get; protected set; }
        public StringNotFoundException(string name, StringType type)
            : base(name) {
            this.type = type;
        }
        public StringNotFoundException(string name)
            : base(name) {
        }
    }
}
