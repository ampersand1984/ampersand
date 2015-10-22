using ampersand.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
