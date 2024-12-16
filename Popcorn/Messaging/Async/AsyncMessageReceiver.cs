using System;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;

namespace Popcorn.Messaging.Async
{
    /// <summary>
    /// AsyncMessage Receiver
    /// </summary>
    /// <typeparam name="TMessage"></typeparam>
    class AsyncMessageReceiver<TMessage> : IDisposable
        where TMessage : MessageBase
    {
        private bool _disposed;
        private IMessenger Messenger { get; set; }
        private Func<TMessage, Task<object>> Callback { get; set; }
        private object Token { get; set; }
        public AsyncMessageReceiver(IMessenger messenger,
            object token,
            bool receiveDerivedMessagesToo,
            Func<TMessage, Task<object>> callback)
        {
            Messenger = messenger;
            Token = token;
            Callback = callback;
            messenger.Register<AsyncMessage<TMessage>>(
                this,
                token,
                receiveDerivedMessagesToo,
                ReceiveAsyncMessage);
        }

        private async void ReceiveAsyncMessage(AsyncMessage<TMessage> m)
        {
            try
            {
                var result = await Callback(m.InnerMessage);
                m.SetResult(result);
            }
            catch (Exception ex)
            {
                m.SetException(ex);
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (Callback == null)
                {
                    return;
                }

                Messenger.Unregister<AsyncMessage<TMessage>>(this, Token, ReceiveAsyncMessage);
                Callback = null;
                Token = null;
                Messenger = null;
            }

            _disposed = true;
        }
    }
}
