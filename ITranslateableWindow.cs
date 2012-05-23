using System;

namespace Translator {
    public interface ITranslateableWindow {
        void setTranslatedTitle(string name, params string[] variables);
        bool displayQuestion(string title, string message);
        bool displayError(string title, string message);
        bool displayError(string title, string message, Exception exception);
        bool displayWarning(string title, string message);
        bool displayInfo(string title, string message);
    }
}
