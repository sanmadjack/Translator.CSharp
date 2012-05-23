
namespace Translator {
    public class StringRequest {
        public string name;
        public string[] variables;
        public StringRequest(string name, params string[] variables) {
            this.name = name;
            this.variables = variables;
        }
    }
}
