using System;

namespace Pad4Win.Scintilla
{
    public class Style
    {
        private ScintillaBox _scintilla;

        public Style(ScintillaBox scintilla, int index)
        {
            if (scintilla == null)
                throw new ArgumentNullException("scintilla");

            if (index < 0 || index > StyleCollection.STYLE_MAX)
                throw new ArgumentOutOfRangeException("index");

            _scintilla = scintilla;
            Index = index;
        }

        public int Index { get; private set; }
    }
}
