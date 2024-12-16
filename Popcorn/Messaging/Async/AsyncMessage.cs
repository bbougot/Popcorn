using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;

namespace Popcorn.Messaging.Async
{
    /// <summary>
    /// awaitable message
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    class AsyncMessage<TMessage> : MessageBase
        where TMessage : MessageBase
    {
        private readonly TaskCompletionSource<object> _source = new TaskCompletionSource<object>();
        public TMessage InnerMessage { get; private set; }
        public Task<object> Task => this._source.Task;
        public AsyncMessage(TMessage innerMessage)
        {
            InnerMessage = innerMessage;
        }
        public void SetResult(object result)
        {
            _source.SetResult(result);
        }
        public void SetException(Exception ex)
        {
            _source.SetException(ex);
        }
    }
}
