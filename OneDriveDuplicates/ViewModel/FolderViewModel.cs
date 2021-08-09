using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;
using Microsoft.Graph;
using OneDriveDuplicates.Service;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneDriveDuplicates.ViewModel
{
    public class FolderViewModel : ReactiveObject
    {
        private DriveItem item;

        private ObservableCollection<FolderViewModel> children;// = new List<FolderViewModel>();
        public ObservableCollection<FolderViewModel> Children
        {
            get
            {
                if (children == null)
                {
                    children = new ObservableCollection<FolderViewModel>();
                    GetSubFolders(item.Id);
                }
                return children;
            }
        }

        public string Name => item.Name;
        public string Id => item.Id;

        public FolderViewModel(DriveItem item)
        {
            this.item = item;

        }

        private async void GetSubFolders(string id)
        {
            var graphClient = ProviderManager.Instance.GlobalProvider.GetClient();
            var scannerService = new ScannerService(graphClient);
            var items = scannerService.GetFolders(id);
            await foreach (var item in items)
            {
                children.Add(new FolderViewModel(item));
                //this.RaisePropertyChanged("Children");
            }

        }
    }
}
