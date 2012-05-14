using System;
using System.Collections.Generic;
using System.Xml;
namespace Translator {
    public class StringCollection : Dictionary<StringType, TranslateableString> {
        string name;
        public StringCollection(string name) {
            this.name = name;
        }

        public void addString(XmlNode node) {
            TranslateableString new_string;
            StringType type = StringType.General;
            string content = node.InnerText;

            if (node.Attributes["type"] != null) {
                type = ParseStringType(node.Attributes["type"].InnerText);
            } else {
                Logger.Logger.log(node.OuterXml + " does not have a type attribute.");
            }

            if (node.Attributes["hotkey"] != null) {
                new_string = new TranslateableString(content, node.Attributes["hotkey"].InnerText);
            } else {
                new_string = new TranslateableString(content);
            }

            if (this.ContainsKey(type)) {
                this[type] = new_string;
            } else {
                this.Add(type, new_string);
            }


        }

        public override string ToString() {
            if (this.ContainsKey(StringType.General)) {
                return this[StringType.General].ToString();
            } else {
                throw new Exception("Attempted to perform ToString on a String Collection without a general string: " + name);
            }

        }
        private static StringType ParseStringType(string type) {
            switch (type) {
                case "title":
                    return StringType.Title;
                case "message":
                    return StringType.Message;
                case "general":
                    return StringType.General;
                case "label":
                    return StringType.Label;
                case "source":
                    return StringType.Source;
                case "tooltip":
                    return StringType.ToolTip;
                default:
                    throw new Exception("The string type " + type + " is not known");
            }
        }


    }
}
