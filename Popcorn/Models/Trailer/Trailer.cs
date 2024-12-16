using System;
using GalaSoft.MvvmLight;

namespace Popcorn.Models.Trailer
{
    public class Trailer : ObservableObject
    {
        private Uri _uri;

        /// <summary>
        /// Initialize a new instance of Trailer class
        /// </summary>
        /// <param name="uri">Trailer's uri</param>
        public Trailer(Uri uri)
        {
            Uri = uri;
        }

        /// <summary>
        /// Uri to the decrypted Youtube URL
        /// </summary>
        public Uri Uri
        {
            get => _uri;
            private set { Set(() => Uri, ref _uri, value); }
        }
    }
}