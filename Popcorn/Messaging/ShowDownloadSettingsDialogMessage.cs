using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using Popcorn.Models.Media;
using Popcorn.Models.Movie;

namespace Popcorn.Messaging
{
    public class ShowDownloadSettingsDialogMessage : MessageBase
    {
        public bool Download { get; set; }

        public readonly IMedia Media;

        public ShowDownloadSettingsDialogMessage(IMedia media)
        {
            Media = media;
        }
    }
}
