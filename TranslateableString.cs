using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Translator {
    public class TranslateableString {
        public bool NotFound { get; protected set; }
        public string content { get; protected set; }
        public string hotkey { get; protected set; }

        public bool HasHotKey {
            get {
                return hotkey != null;
            }
        }

        private static Regex subsitution_regex = new Regex(@"%[A-za-z]*%", RegexOptions.IgnoreCase);

        private static Regex variable_regex = new Regex(@"%[0-9]*", RegexOptions.IgnoreCase);

        public TranslateableString(string content, bool not_found)
            : this(content, null) {
            this.NotFound = not_found;
        }
        public TranslateableString(string content) : this(content, null) { }

        public TranslateableString(string content, string hotkey) {
            this.content = content;
            this.hotkey = hotkey;
        }

        public string interpret(params string[] variables) {
            return TranslateableString.interpret(content, variables);
        }

        private static string interpret(string content, params string[] variables) {
            StringBuilder builder = new StringBuilder(content);

            Match m = subsitution_regex.Match(builder.ToString());
            int offset = 0;
            while (m.Success) {
                foreach (Group g in m.Groups) {
                    foreach (Capture c in g.Captures) {
                        string key = c.Value.Trim('%');
                        string line = Strings.getGeneralString(key);
                        builder.Remove(c.Index + offset, c.Length);
                        builder.Insert(c.Index + offset, line);
                        offset += line.Length - c.Length;
                    }
                }
                m = m.NextMatch();
            }

            m = variable_regex.Match(builder.ToString());
            offset = 0;
            while (m.Success) {
                foreach (Group g in m.Groups) {
                    foreach (Capture c in g.Captures) {
                        Int64 key = Int64.Parse(c.Value.TrimStart('%'));
                        if (variables.Length > key) {
                            string line = variables[key];
                            builder.Remove(c.Index + offset, c.Length);
                            builder.Insert(c.Index + offset, line);
                            offset += line.Length - c.Length;
                        } else {
                            Logger.Logger.log("String " + content + " has  " + m.Groups.Count.ToString() + " variable slots, but "
                                + variables.Length.ToString() + " are being provided. Please adjust the translation file to accomodate this number of variables, which are as follows:");
                            foreach (string var in variables) {
                                Logger.Logger.log(var);
                            }
                        }
                    }
                }
                m = m.NextMatch();

            }
            string output = builder.ToString();
            if (output.Contains("\\n")) {
                output = output.Replace("\\n", Environment.NewLine);
            }
            return output.ToString();

        }
    }
}
