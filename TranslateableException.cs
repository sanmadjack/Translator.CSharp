﻿using System;

namespace Translator {
    public class TranslateableException : Exception {
        public readonly string[] variables;
        public TranslateableException(string name, Exception inner, params string[] variables) :
            base(name, inner) {
            this.variables = variables;
        }
        public TranslateableException(string name, params string[] variables) :
            base(name) {
            this.variables = variables;
        }
    }
}
