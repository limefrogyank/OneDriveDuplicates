using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneDriveDuplicates.ViewModel
{
    public class WelcomePageViewModel : ReactiveObject, IRoutableViewModel
    {
        public string UrlPathSegment => "WelcomePage";

        public IScreen HostScreen { get; }

    }
}
