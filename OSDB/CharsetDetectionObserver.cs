using NChardet;

namespace OSDB
{
    public class CharsetDetectionObserver :
        ICharsetDetectionObserver
    {
        public string Charset = null;

        public void Notify(string charset)
        {
            Charset = charset;
        }
    }
}