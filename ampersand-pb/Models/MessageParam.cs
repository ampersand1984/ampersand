using MahApps.Metro.Controls.Dialogs;

namespace ampersand_pb.Models
{
    public class MessageParam
    {
        public MessageParam(string title, string message)
            :this(title, message, MessageDialogStyle.Affirmative, null)
        {

        }

        public MessageParam(string title, string message, MessageDialogStyle style, MetroDialogSettings settings)
        {
            Title = title;
            Message = message;
            Style = style;
            Settings = settings;
        }

        public string Title { get; }
        public string Message { get; }
        public MessageDialogStyle Style { get; }
        public MetroDialogSettings Settings { get; }
    }
}
