using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
namespace Translator {
    public class TranslateableString {
        public bool NotFound { get; protected set; }
        public string Content { get; protected set; }
        public string HotKey { get; protected set; }

        public bool HasHotKey {
            get {
                return HotKey != null;
            }
        }

        private static Regex subsitution_regex = new Regex(@"%([A-za-z]+)%", RegexOptions.IgnoreCase);

        private static Regex variable_regex = new Regex(@"%([0-9]+)", RegexOptions.IgnoreCase);

        private static Regex conditional_regex = new Regex(@"%([0-9]+){([^}]+)}", RegexOptions.IgnoreCase);

        public TranslateableString(string content, bool not_found)
            : this(content, null) {
            this.NotFound = not_found;
        }
        public TranslateableString(string content) : this(content, null) { }

        public TranslateableString(string content, string hotkey) {
            this.Content = content;
            this.HotKey = hotkey;
        }

        public string interpret(params string[] variables) {
            return TranslateableString.interpret(Content, variables);
        }

        private static string interpret(string content, params string[] variables) {
            StringBuilder builder = new StringBuilder(content);
            Match match;
            int offset;


            match = subsitution_regex.Match(builder.ToString());
            offset = 0;
            while (match.Success) {
                Group g = match.Groups[0];
                Group key_g = match.Groups[1];
                string key = key_g.Value;

                string line = Strings.getString(StringType.Source, key).interpret();
                builder.Remove(g.Index + offset, g.Length);
                builder.Insert(g.Index + offset, line);
                offset += line.Length - g.Length;

                match = match.NextMatch();
            }


            match = conditional_regex.Match(builder.ToString());
            offset = 0;
            while (match.Success) {
                Group g = match.Groups[0];
                Group key_g = match.Groups[1];
                Group options_g = match.Groups[2];
                long key;

                char[] split_char = options_g.Value.Substring(0, 1).ToCharArray();
                string[] options = options_g.Value.Split(split_char, StringSplitOptions.RemoveEmptyEntries);
                Dictionary<string, string> matchers = new Dictionary<string, string>();
                for (int i = 0; i < options.Length; i++) {
                    matchers.Add(options[i], options[++i]);
                }


                if (!Int64.TryParse(key_g.Value, out key)) {
                    Logger.Logger.log("Unable to parse " + content + "  variable name " + key_g.Value + " as Integer");
                } else {
                    string line;
                    if (variables.Length <= key) {
                        if (!matchers.ContainsKey("*NULL")) {
                            Logger.Logger.log("String " + content + " is requesting a " + key + "th variable, but "
                            + variables.Length.ToString() + " are being provided by the program. Please adjust the translation file to accomodate this number of variables, which are as follows:");
                            foreach (string var in variables) {
                                Logger.Logger.log(var);
                            }
                            line = g.Value;
                        } else {
                            line = matchers["*NULL"];
                        }
                    } else {
                        string test = variables[key];
                        if (matchers.ContainsKey(test)) {
                            line = matchers[test];
                        } else if (matchers.ContainsKey("*ELSE")) {
                            line = matchers["*ELSE"];
                        } else {
                            line = g.Value;
                            Logger.Logger.log("This string: " + content + "  has no value for %" + key + " that matches" + test);
                        }
                    }
                    builder.Remove(g.Index + offset, g.Length);
                    builder.Insert(g.Index + offset, line);
                    offset += line.Length - g.Length;
                }
                match = match.NextMatch();
            }


            match = variable_regex.Match(builder.ToString());
            offset = 0;
            while (match.Success) {
                Group g = match.Groups[0];
                Group key_g = match.Groups[1];
                long key;
                if (!Int64.TryParse(key_g.Value, out key)) {
                    Logger.Logger.log("Unable to parse " + content + "  variable name " + key_g.Value + " as Integer");
                } else if (variables.Length <= key) {
                    Logger.Logger.log("String " + content + " is requesting a " + key + "th variable, but "
                        + variables.Length.ToString() + " are being provided by the program. Please adjust the translation file to accomodate this number of variables, which are as follows:");
                    foreach (string var in variables) {
                        Logger.Logger.log(var);
                    }
                } else {
                    string line = variables[key];
                    builder.Remove(g.Index + offset, g.Length);
                    builder.Insert(g.Index + offset, line);
                    offset += line.Length - g.Length;
                }
                match = match.NextMatch();
            }



            string output = builder.ToString();
            if (output.Contains("\\n")) {
                output = output.Replace("\\n", Environment.NewLine);
            }
            return output.ToString();

        }
    }
}
