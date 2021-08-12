using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;
using DynamicData;
using Microsoft.Graph;
using OneDriveDuplicates.Model;
using OneDriveDuplicates.Service;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace OneDriveDuplicates.ViewModel
{
    public class CommonFolderGroupedDupesViewModel : ReactiveObject
    {
        private ScannerService scanner;

        [Reactive] public string Name { get; set; }
        [Reactive] public Uri Uri { get; set; }
        public ReadOnlyObservableCollection<DupeViewModel> Children { get; }
        public ReactiveCommand<Unit, Unit> ConsolidateToShortestNameCommand { get; set; }

        public CommonFolderGroupedDupesViewModel(IGroup<ModdedDriveItem, string, string> group, ISourceCache<DriveItem,string> source)
        {
            var graphClient = ProviderManager.Instance.GlobalProvider.GetClient();
            //var graphClient = MainPage.MsalProvider.GetClient();
            scanner = new ScannerService(graphClient);

            var grouped = group.Cache.Connect()
                .Group(x => x.Hash)
                .Transform(x => new DupeViewModel(x))
                .Filter(Observable.Return<Func<DupeViewModel, bool>>(y => y.Count > 1), group.Cache.CountChanged.Select(x => Unit.Default))
                .RefCount();

            grouped.ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out var children)
                .Subscribe();
            Children = children;

            grouped.ToCollection().Take(1)
                .Subscribe(async x =>
                {
                    var first = x.FirstOrDefault();
                    if (first != null)
                    {
                        var decoded = HttpUtility.UrlDecode(first.ParentReference.Path);
                        Name = decoded.Substring(decoded.IndexOf(':') + 1) + "/" + first.ParentReference.Name;

                        var folder = await scanner.GetFolderAsync(first.ParentReference.Id);
                        if (folder != null)
                        {
                            Uri = new Uri(folder.WebUrl);
                        }
                    }
                });

            ConsolidateToShortestNameCommand = ReactiveCommand.CreateFromTask<Unit, Unit>(async x =>
            {
                var listToDelete = new List<string>();
                var children = Children.ToList();  //make a copy in case the list is still populating
                foreach (var dupe in children)
                {
                    var min = dupe.DuplicateItems.Min(x => x.Name.Length);
                    var shortest = dupe.DuplicateItems.FirstOrDefault(x => x.Name.Length == min);
                    foreach (var item in dupe.DuplicateItems.Except(new[] { shortest }))
                    {
                        listToDelete.Add(item.Id);
                    }
                }
                if (listToDelete.Count > 0)
                {
                    await scanner.DeleteBatchFileAsync(listToDelete);
                }
                source.RemoveKeys(listToDelete);
             
                return Unit.Default;
            }, grouped.ToCollection().Select(x => x.Count > 0 ? true : false));

            


        }
    }
}
