using System;
using GalaSoft.MvvmLight;
using NLog;
using Popcorn.Utils;

namespace Popcorn.Services.Application
{
    public class ApplicationService : ObservableObject, IApplicationService
    {
        /// <summary>
        /// Logger of the class
        /// </summary>
        private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Power request
        /// </summary>
        private NativeMethods.POWER_REQUEST_CONTEXT _powerRequestContext;

        /// <summary>
        /// Handle
        /// </summary>
        private IntPtr _powerRequest;

        /// <summary>
        /// Specify if a connection error has occured
        /// </summary>
        private bool _isConnectionInError;

        /// <summary>
        /// Indicates if application is fullscreen
        /// </summary>
        private bool _isFullScreen;

        /// <summary>
        /// Indicates if a movie is playing
        /// </summary>
        private bool _isMoviePlaying;

        /// <summary>
        /// Indicates if a movie is playing
        /// </summary>
        public bool IsMediaPlaying
        {
            get => _isMoviePlaying;
            set { Set(() => IsMediaPlaying, ref _isMoviePlaying, value); }
        }

        /// <summary>
        /// Specify if a connection error has occured
        /// </summary>
        public bool IsConnectionInError
        {
            get => _isConnectionInError;
            set { Set(() => IsConnectionInError, ref _isConnectionInError, value); }
        }

        /// <summary>
        /// Indicates if application is fullscreen
        /// </summary>
        public bool IsFullScreen
        {
            get => _isFullScreen;
            set { Set(() => IsFullScreen, ref _isFullScreen, value); }
        }

        private bool _enableConstantDisplayAndPower;

        /// <summary>
        /// Prevent Windows from sleeping
        /// </summary>
        /// <summary>
        /// Prevent screensaver, display dimming and power saving. This function wraps PInvokes on Win32 API. 
        /// </summary>
        /// <param name="enableConstantDisplay"></param>
        public void SwitchConstantDisplayAndPower(bool enableConstantDisplay)
        {
            try
            {
                if (enableConstantDisplay && !_enableConstantDisplayAndPower)
                {
                    // Set up the diagnostic string
                    _powerRequestContext.Version = NativeMethods.POWER_REQUEST_CONTEXT_VERSION;
                    _powerRequestContext.Flags = NativeMethods.POWER_REQUEST_CONTEXT_SIMPLE_STRING;
                    _powerRequestContext.SimpleReasonString = "Popcorn is playing a media.";

                    // Create the request, get a handle
                    _powerRequest = NativeMethods.PowerCreateRequest(ref _powerRequestContext);

                    // Set the request
                    NativeMethods.PowerSetRequest(_powerRequest,
                        NativeMethods.PowerRequestType.PowerRequestSystemRequired);
                    NativeMethods.PowerSetRequest(_powerRequest,
                        NativeMethods.PowerRequestType.PowerRequestDisplayRequired);
                    _enableConstantDisplayAndPower = true;
                }
                else if(!enableConstantDisplay && _enableConstantDisplayAndPower)
                {
                    // Clear the request
                    NativeMethods.PowerClearRequest(_powerRequest,
                        NativeMethods.PowerRequestType.PowerRequestSystemRequired);
                    NativeMethods.PowerClearRequest(_powerRequest,
                        NativeMethods.PowerRequestType.PowerRequestDisplayRequired);

                    NativeMethods.CloseHandle(_powerRequest);
                    _enableConstantDisplayAndPower = false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}