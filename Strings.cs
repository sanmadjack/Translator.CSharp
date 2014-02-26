using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Schema;

namespace Translator {
    public class Strings {
        private static string language = "en";
        private static string region = "US";

        private static string language_override = null;

        private static Dictionary<string, StringCollection> strings =
            new Dictionary<string, StringCollection>();

        private static XmlReaderSettings xml_settings;

        public static List<string> available_languages {
            get {
                List<String> files = new List<string>();
                foreach (FileInfo file in new DirectoryInfo("Strings").GetFiles("*.xml")) {
                    files.Add(file.Name.Remove(-4));
                }
                return files;
            }
        }

        static Strings() {
            // Checks if the command line indicates we should be running in translation mode
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++) {
                switch (args[i]) {
                    case "-language_file":
                        if (args.Length > i + 1 && !args[i + 1].StartsWith("-")) {
                            i++;
                            language_override = args[i];
                        } else {
                            Logger.Logger.log("No language file specified!");
                        }
                        break;
                }
            }


            xml_settings = new XmlReaderSettings();
            xml_settings.ConformanceLevel = ConformanceLevel.Auto;
            xml_settings.IgnoreWhitespace = true;
            xml_settings.IgnoreComments = true;
            xml_settings.IgnoreProcessingInstructions = false;
            xml_settings.DtdProcessing = DtdProcessing.Parse;
            //xml_settings.ProhibitDtd = false;
            xml_settings.ValidationType = ValidationType.Schema;
            xml_settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessIdentityConstraints;
            xml_settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            xml_settings.ValidationEventHandler += new ValidationEventHandler(validationHandler);

            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            RegionInfo currentRegion = new RegionInfo(currentCulture.LCID);

            language = currentCulture.TwoLetterISOLanguageName;
            region = currentRegion.TwoLetterISORegionName;

            loadRegion();
        }

        public static void overrideRegion(string new_language, string new_region) {
            language = new_language;
            region = new_region;
        }

        private static void loadRegion() {
            if (!String.IsNullOrEmpty(language_override)) {
                if (File.Exists(Path.Combine("Strings", language_override))) {
                    loadFile(Path.Combine("Strings", language_override));
                } else {
                    Logger.Logger.log("ERROR: CHOSEN STRINGS FILE " + language_override + " IS NOT PRESENT IN Strings FOLDER");
                }
                return;
            }


            // Load the English file first, so that if there are any strings missing from a translation,
            // at least the user will still see something they can punch in to babelfish
            // If we're in translate mode, this is skipped, so that untranslated strings will
            // Show up as the string name rather than the translated string itself
            if (File.Exists(Path.Combine("Strings", "en.xml"))) {
                loadFile(Path.Combine("Strings", "en.xml"));
            } else {
                Logger.Logger.log("ERROR: en.xml cannot be found! You probably need to re-install!");
            }

            // We start by checking for (and loading) a general string file for the current language
            if (File.Exists(Path.Combine("Strings", language + ".xml"))) {
                loadFile(Path.Combine("Strings", language + ".xml"));

            }

            // We then load a region-specific string file, so that if several regions use the same translation
            // for a string, we just put them in the common language file, then put only the region-specific
            // strings in this file
            if (File.Exists(Path.Combine("Strings", language + "-" + region + ".xml"))) {
                loadFile(Path.Combine("Strings", language + "-" + region + ".xml"));
            }

        }




        private static void loadFile(string file) {
            XmlDocument strings_xml = new XmlDocument();
            XmlReader parse_me = XmlReader.Create(file, xml_settings);

            try {
                strings_xml.Load(parse_me);

                XmlNodeList list = strings_xml.GetElementsByTagName("strings");
                XmlNode nodes = list[0];
                foreach (XmlNode node in nodes.ChildNodes) {
                    if (node.Name == "string") {
                        string name = node.Attributes["name"].InnerText;
                        StringCollection col;
                        // If the string is already present, then we assume that the new string supercedes the previous one
                        if (strings.ContainsKey(name)) {
                            col = strings[name];
                        } else {
                            col = new StringCollection(name);
                            strings.Add(name, col);
                        }
                        col.addString(node);
                    }
                }
            } catch (XmlException ex) {
                IXmlLineInfo info = parse_me as IXmlLineInfo;
                Logger.Logger.log("The file " + file + " has produced this error:" + Environment.NewLine + ex.Message + Environment.NewLine + "The error is on or near line " + info.LineNumber + ", possibly at column " + info.LinePosition + "." + Environment.NewLine + "Go fix it.");
            } catch (Exception ex) {
                Logger.Logger.log("The file " + file + " has produced this error:" + Environment.NewLine + ex.Message);
                Logger.Logger.log(ex);
            } finally {
                parse_me.Close();
            }



        }


        public static StringCollection getInterfaceString(string name) {
            StringCollection str;
            // So, all interface translation strings are going to start with a dollar sign.
            // That way we can leave some interface elements alone
            if (!String.IsNullOrEmpty(name) && name.StartsWith("$")) {
                string real_name = name.TrimStart('$');
                if (real_name.Contains("#")) {
                    str = getStrings(real_name.Split('#')[0]);
                } else {
                    str = getStrings(real_name);
                }
                if (str.ContainsKey(StringType.Label)) {
                    return str;
                }
            }
            str = new StringCollection(name);
            str.Add(StringType.Label, new TranslateableString(name));
            return str;
        }

        public static string GetTitleString(string name, params string[] variables) {
            TranslateableString str = getString(StringType.Title, name);
            return str.interpret(variables);
        }
        public static string GetLabelString(string name, params string[] variables) {
            TranslateableString str = getString(StringType.Label, name);
            return str.interpret(variables);
        }
        public static string GetSourceString(string name, params string[] variables) {
            TranslateableString str = getString(StringType.Source, name);
            return str.interpret(variables);
        }
        public static string GetToolTipString(string name, params string[] variables) {
            TranslateableString str = getString(StringType.ToolTip, name);
            return str.interpret(variables);
        }
        public static string GetMessageString(string name, params string[] variables) {
            TranslateableString str = getString(StringType.Message, name);
            return str.interpret(variables);
        }
        public static TranslateableString getString(StringType type, string name) {
            StringCollection strings = getStrings(name);
            if (strings.ContainsKey(type)) {
                return strings[type];
            } else {
                Logger.Logger.log("STRING " + name + " OF TYPE " + type.ToString() + " NOT FOUND");
            }
            return new TranslateableString(name);
        }

        public static string getString(StringType type, string name, params string[] variables) {
            StringCollection strings = getStrings(name);
            if (strings.ContainsKey(type)) {
                return strings[type].interpret(variables);
            } else {
                Logger.Logger.log("STRING " + name + " OF TYPE " + type.ToString() + " NOT FOUND");
            }
            return name;
        }

        public static StringCollection getStrings(string name) {
            StringCollection error = new StringCollection(name);
            if (String.IsNullOrEmpty(name)) {
                error.Add(StringType.General, new TranslateableString("NULL NAME"));
                return error;
            }


            if (!strings.ContainsKey(name)) {
                // If running in translate mode, then we'll throw an exception when a string is missing.
                Logger.Logger.log("STRING " + name + " NOT FOUND");
                if (!String.IsNullOrEmpty(language_override)) {
                    // This behavior will probably not stick, as most of the time this code occurs during GUI drawing,
                    // So Windows wraps this exception in a WPF exception, which effectively hides this info
                    // from the average user. When breaking into debug in Visual Studio though, this allows us
                    // to see exactly which string is missing.
                    error.Add(StringType.General, new TranslateableString("STRING " + name + " NOT FOUND"));
                } else {
                    // This will eventually become the only behavior when a string isn't found,
                    // so that the main interface will just display the name of a string
                    error.Add(StringType.General, new TranslateableString(name));
                }
                return error;
            }
            return strings[name];

        }

        // Event handler to take care of XML errors while reading game configs
        private static void validationHandler(object sender, ValidationEventArgs args) {
            throw new XmlException(args.Message);
        }


    }
}