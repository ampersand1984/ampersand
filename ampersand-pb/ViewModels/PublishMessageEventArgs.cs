using ampersand_pb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
