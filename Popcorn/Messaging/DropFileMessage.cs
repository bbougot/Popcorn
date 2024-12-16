using GalaSoft.MvvmLight.Messaging;

namespace Popcorn.Messaging
{
    public class DropFileMessage : MessageBase
    {
        public enum DropFileEvent
        {
            Enter,
            Leave
        }

        public DropFileEvent Event { get; }

        public DropFileMessage(DropFileEvent dropFileEvent)
        {
            Event = dropFileEvent;
        }
    }
}