using System;

namespace Popcorn.Events
{
    /// <summary>
    /// When a show season has been selected, transfer its number
    /// </summary>
    public class SelectedSeasonChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The selected season number
        /// </summary>
        public readonly int SelectedSeasonNumber;

        /// <summary>
        /// Initialize a new instance of SelectedSeasonChangedEventArgs
        /// </summary>
        /// <param name="selectedSeasonNumber">Season number</param>
        public SelectedSeasonChangedEventArgs(int selectedSeasonNumber)
        {
            SelectedSeasonNumber = selectedSeasonNumber;
        }
    }
}
