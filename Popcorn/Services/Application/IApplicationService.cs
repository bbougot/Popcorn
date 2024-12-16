namespace Popcorn.Services.Application
{
    public interface IApplicationService
    {
        /// <summary>
        /// Indicates if application is fullscreen
        /// </summary>
        bool IsFullScreen { get; set; }

        /// <summary>
        /// Specify if a connection error has occured
        /// </summary>
        bool IsConnectionInError { get; set; }

        /// <summary>
        /// Specify if a movie is playing
        /// </summary>
        bool IsMediaPlaying { get; set; }

        void SwitchConstantDisplayAndPower(bool enableConstantDisplay);
    }
}