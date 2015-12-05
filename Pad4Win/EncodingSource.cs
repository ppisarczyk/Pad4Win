using System.Collections.Generic;
using System.Text;

namespace Pad4Win
{
    public class EncodingSource
    {
        private static readonly List<Encoding> _all = GetEncodings();

        public static IList<Encoding> All
        {
            get
            {
                return _all;
            }
        }

        private static List<Encoding> GetEncodings()
        {
            var list = new List<Encoding>();
            var infos = Encoding.GetEncodings();

            Encoding win1252 = null;
            Encoding latin1 = null;

            foreach (var info in infos)
            {
                var enc = info.GetEncoding();
                if (enc is UTF8Encoding)
                    continue;

                if (enc.CodePage == 1252)
                {
                    win1252 = enc;
                    continue;
                }

                if (enc.CodePage == 28591)
                {
                    latin1 = enc;
                    continue;
                }

                list.Add(enc);
            }
            list.Sort(new EncodingComparer());

            // put these in front of the others
            if (win1252 != null)
            {
                list.Insert(0, win1252);
            }
            if (latin1 != null)
            {
                list.Insert(0, latin1);
            }
            list.Insert(0, UTF8EncodingNoBom.Value);
            list.Insert(0, Encoding.UTF8);
            return list;
        }

        private class EncodingComparer : IComparer<Encoding>
        {
            public int Compare(Encoding x, Encoding y)
            {
                return x.EncodingName.CompareTo(y.EncodingName);
            }
        }
    }
}
