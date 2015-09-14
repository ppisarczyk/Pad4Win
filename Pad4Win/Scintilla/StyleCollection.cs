using System.Collections;
using System.Collections.Generic;

namespace Pad4Win.Scintilla
{
    public class StyleCollection : IReadOnlyList<Style>
    {
        internal const int STYLE_MAX = 255;
        private ScintillaBox _scintilla;

        internal StyleCollection(ScintillaBox scintilla)
        {
            _scintilla = scintilla;
        }

        public Style this[int index]
        {
            get
            {
                return new Style(_scintilla, index);
            }
        }

        public int Count
        {
            get
            {
                return STYLE_MAX + 1;
            }
        }

        public IEnumerator<Style> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
