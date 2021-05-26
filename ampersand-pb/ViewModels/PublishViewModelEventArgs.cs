using System;
using ampersand.Core;

namespace ampersand_pb.ViewModels
{
    public class PublishViewModelEventArgs: EventArgs
    {
        public PublishViewModelEventArgs(BaseViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public BaseViewModel ViewModel { get; private set; }
    }
}
