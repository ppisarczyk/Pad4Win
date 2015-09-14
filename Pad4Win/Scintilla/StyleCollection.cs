using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Pad4Win.Scintilla
{
    public class StyleCollection : ObservableCollection<Style>
    {
        private ScintillaBox _scintilla;

        internal StyleCollection(ScintillaBox scintilla)
        {
            _scintilla = scintilla;
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Style style in e.NewItems)
                    {
                        style._scintilla = _scintilla;
                    }
                    break;
            }

            base.OnCollectionChanged(e);
        }
    }
}
