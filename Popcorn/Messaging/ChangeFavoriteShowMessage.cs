using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;

namespace Popcorn.Messaging
{
    /// <summary>
    /// Used to broadcast the show whose favorite value has changed
    /// </summary>
    public class ChangeFavoriteShowMessage : MessageBase
    {
    }
}
