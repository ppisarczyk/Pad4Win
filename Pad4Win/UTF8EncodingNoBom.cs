using System.Text;

namespace Pad4Win
{
    public class UTF8EncodingNoBom : UTF8Encoding
    {
        public static readonly UTF8EncodingNoBom Value = new UTF8EncodingNoBom();

        public UTF8EncodingNoBom()
            : base(false)
        {
        }

        public override string EncodingName
        {
            get
            {
                return base.EncodingName + ", no Byte Order Mark";
            }
        }
    }
}
