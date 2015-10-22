using ampersand.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
