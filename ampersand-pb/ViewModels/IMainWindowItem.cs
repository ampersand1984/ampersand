using ampersand.Core.Common;
using System;
using MahApps.Metro.Controls.Dialogs;

namespace ampersand_pb.ViewModels
{
    public interface IMainWindowItem
    {
        string DisplayName { get; }
        RelayCommand CloseCommand { get; }
        IDialogCoordinator DialogCoordinator { get; set; }

        event EventHandler CloseEvent;

        event EventHandler<PublishViewModelEventArgs> PublishViewModelEvent;
    }
}
