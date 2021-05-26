using System;
using ampersand.Core;

namespace ampersand_pb.ViewModels
{
    public class EditViewModelEventArgs : EventArgs
    {
        public EditViewModelEventArgs(BaseViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public BaseViewModel ViewModel { get; private set; }
    }
}
