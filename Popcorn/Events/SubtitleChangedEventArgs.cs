using System;
using OSDB.Models;

namespace Popcorn.Events
{
    public class SubtitleChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The subtitle
        /// </summary>
        public readonly Subtitle Subtitle;

        /// <summary>
        /// Initialize a new instance of SubtitleChangedEventArgs
        /// </summary>
        /// <param name="subtitle">The subtitle</param>
        public SubtitleChangedEventArgs(Subtitle subtitle)
        {
            Subtitle = subtitle;
        }
    }
}
