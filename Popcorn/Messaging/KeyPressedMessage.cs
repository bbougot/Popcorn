using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using Popcorn.Utils;

namespace Popcorn.Messaging
{
    public class KeyPressedMessage : MessageBase
    {
        public readonly KeyPressedArgs KeyPressedArgs;

        public KeyPressedMessage(KeyPressedArgs args)
        {
            KeyPressedArgs = args;
        }
    }
}
