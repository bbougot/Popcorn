using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;

namespace Popcorn.Messaging
{
    public class DownloadMagnetLinkMessage : MessageBase
    {
        public readonly string MagnetLink;

        public DownloadMagnetLinkMessage(string magnetLink)
        {
            MagnetLink = magnetLink;
        }
    }
}
