using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Translator
{
    public interface ITranslateableWindow
    {
        void setTranslatedTitle(string name, params string[] variables);
        bool askQuestion(string title, string message);
        bool showError(string title, string message);
        bool showError(string title, string message, Exception exception);
        bool showWarning(string title, string message);
        bool showInfo(string title, string message);
    }
}
