using ampersand.Core.Common;
using System;

namespace ampersand_pb.ViewModels
{
    public interface IMainWindowItem
    {
        string DisplayName { get; }
        RelayCommand CloseCommand { get; }

        event EventHandler CloseEvent;

        event EventHandler<PublishViewModelEventArgs> PublishViewModelEvent;
    }
}
