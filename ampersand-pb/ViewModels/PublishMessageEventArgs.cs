using System;
using ampersand_pb.Models;

namespace ampersand_pb.ViewModels
{
    public class PublishMessageEventArgs : EventArgs
    {
        public PublishMessageEventArgs(MessageParam messageParam)
        {
            MessageParam = messageParam;
        }

        public MessageParam MessageParam { get; }
    }
}
