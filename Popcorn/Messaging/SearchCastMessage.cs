using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using Popcorn.Models.Cast;

namespace Popcorn.Messaging
{
    public class SearchCastMessage : MessageBase
    {
        public readonly CastJson Cast;

        public SearchCastMessage(CastJson cast)
        {
            Cast = cast;
        }
    }
}
