using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;
using DynamicData;
using Microsoft.Graph;
using OneDriveDuplicates.Model;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace OneDriveDuplicates.ViewModel
{
    public class DupeViewModel : ReactiveObject
    {
        [Reactive] public string FirstFileId { get; set; }
        [Reactive] public string Name { get; set; }
        [Reactive] public int Count { get; set;  }
        [Reactive] public IReadOnlyCollection<ModdedDriveItem> DuplicateItems { get; set; }

        [Reactive] public ItemReference ParentReference { get; set;  }

        private string imageUri;
        public string ImageUri
        {
            get
            {
                if (imageUri == null)
                {
                    if (FirstFileId != null)
                    {
                        LoadImage(FirstFileId);
                    }
                }
                return imageUri;
            }
        }

        public DupeViewModel(IGroup<ModdedDriveItem,string,string> group)
        {
            group.Cache.Connect()
                .ToCollection()
                .Subscribe(x=>
                {
                    DuplicateItems = x;
                    Count = x.Count;
                    var first = x.FirstOrDefault();
                    if (first != null)
                    {
                        Name = first.File.Name;
                        FirstFileId = first.File.Id;
                        ParentReference = first.ParentReference;
                    }
                });
        }


        private async void LoadImage(string id)
        {
            var graphClient = ProviderManager.Instance.GlobalProvider.GetClient();
            var thumbnails = await graphClient.Me.Drive.Items[id].Thumbnails.Request().GetAsync();
            var thumbnail = thumbnails.FirstOrDefault();
            if (thumbnail != null)
            {
                imageUri = thumbnail.Medium.Url;
                this.RaisePropertyChanged("ImageUri");
            }
        }

    }
}
