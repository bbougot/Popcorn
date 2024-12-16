using System;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using Popcorn.Messaging.Async;

namespace Popcorn.Extensions
{
    /// <summary>
    /// IMessenger extension methods.
    /// </summary>
    public static class MessengerExtensions
    {
        /// <summary>
        /// Send AsyncMessage.<br/>
        /// Important!!: If not found receiver then freeze.
        /// </summary>
        /// <typeparam name="TMessage">wrapped message type</typeparam>
        /// <param name="self"></param>
        /// <param name="message">wrapped message</param>
        /// <returns></returns>
        public static Task SendAsync<TMessage>(this IMessenger self, TMessage message)
            where TMessage : MessageBase
        {
            var asyncMessage = new AsyncMessage<TMessage>(message);
            self.Send(asyncMessage);
            return asyncMessage.Task;
        }

        /// <summary>
        /// Send AsyncMessage.<br/>
        /// Important!!: If not found receiver then freeze.
        /// </summary>
        /// <typeparam name="TMessage">wrapped message type</typeparam>
        /// <typeparam name="TResult">return type</typeparam>
        /// <param name="self"></param>
        /// <param name="message">wrapped message</param>
        /// <returns></returns>
        public static async Task<TResult> SendAsync<TMessage, TResult>(this IMessenger self, TMessage message)
            where TMessage : MessageBase
        {
            var asyncMessage = new AsyncMessage<TMessage>(message);
            self.Send(asyncMessage);
            return (TResult)await asyncMessage.Task;
        }

        /// <summary>
        /// register message receive callback method. you must have return value reference. if don't have reference then unregist callback, mvvm light WeakReference.
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="self"></param>
        /// <param name="token"></param>
        /// <param name="callback"></param>
        /// <returns>when call dispose method, unregist.</returns>
        public static IDisposable RegisterAsyncMessage<TMessage>(this IMessenger self,
            object token,
            Func<TMessage, Task> callback)
            where TMessage : MessageBase
        {
            return new AsyncMessageReceiver<TMessage>(
                self,
                token,
                false,
                async m =>
                {
                    await callback(m);
                    return null;
                });
        }

        /// <summary>
        /// register message receive callback method. you must have return value reference. if don't have reference then unregist callback, mvvm light WeakReference.
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="self"></param>
        /// <param name="callback"></param>
        /// <returns>when call dispose method, unregist.</returns>
        public static IDisposable RegisterAsyncMessage<TMessage>(this IMessenger self,
            Func<TMessage, Task> callback)
            where TMessage : MessageBase
        {
            return new AsyncMessageReceiver<TMessage>(
                self,
                null,
                false,
                async m =>
                {
                    await callback(m);
                    return null;
                });
        }

        /// <summary>
        /// register message receive callback method. you must have return value reference. if don't have reference then unregist callback, mvvm light WeakReference.
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="self"></param>
        /// <param name="token"></param>
        /// <param name="receiveDerivedMessagesToo"></param>
        /// <param name="callback"></param>
        /// <returns>when call dispose method, unregist.</returns>
        public static IDisposable RegisterAsyncMessage<TMessage>(this IMessenger self,
            object token,
            bool receiveDerivedMessagesToo,
            Func<TMessage, Task> callback)
            where TMessage : MessageBase
        {
            return new AsyncMessageReceiver<TMessage>(
                self,
                token,
                receiveDerivedMessagesToo,
                async m =>
                {
                    await callback(m);
                    return null;
                });
        }

        /// <summary>
        /// register message receive callback method. you must have return value reference. if don't have reference then unregist callback, mvvm light WeakReference.
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="self"></param>
        /// <param name="token"></param>
        /// <param name="callback"></param>
        /// <returns>when call dispose method, unregist.</returns>
        public static IDisposable RegisterAsyncMessage<TMessage, TResult>(this IMessenger self,
            object token,
            Func<TMessage, Task<TResult>> callback)
            where TMessage : MessageBase
        {
            return new AsyncMessageReceiver<TMessage>(
                self,
                token,
                false,
                async m => await callback(m));
        }

        /// <summary>
        /// register message receive callback method. you must have return value reference. if don't have reference then unregist callback, mvvm light WeakReference.
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="self"></param>
        /// <param name="callback"></param>
        /// <returns>when call dispose method, unregist.</returns>
        public static IDisposable RegisterAsyncMessage<TMessage, TResult>(this IMessenger self,
            Func<TMessage, Task<TResult>> callback)
            where TMessage : MessageBase
        {
            return new AsyncMessageReceiver<TMessage>(
                self,
                null,
                false,
                async m => await callback(m));
        }

        /// <summary>
        /// register message receive callback method. you must have return value reference. if don't have reference then unregist callback, mvvm light WeakReference.
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="self"></param>
        /// <param name="token"></param>
        /// <param name="receiveDerivedMessagesToo"></param>
        /// <param name="callback"></param>
        /// <returns>when call dispose method, unregist.</returns>
        public static IDisposable RegisterAsyncMessage<TMessage, TResult>(this IMessenger self,
            object token,
            bool receiveDerivedMessagesToo,
            Func<TMessage, Task<TResult>> callback)
            where TMessage : MessageBase
        {
            return new AsyncMessageReceiver<TMessage>(
                self,
                token,
                receiveDerivedMessagesToo,
                async m => await callback(m));
        }
    }
}
