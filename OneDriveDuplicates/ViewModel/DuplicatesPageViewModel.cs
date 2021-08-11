using Microsoft.Graph;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using OneDriveDuplicates.Service;
using System.Collections.ObjectModel;
using OneDriveDuplicates.Model;
using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;
using Splat;
using ReactiveUI.Fody.Helpers;

namespace OneDriveDuplicates.ViewModel
{
    public class DuplicatesPageViewModel : ReactiveObject,IRoutableViewModel
    {
        private ScannerService scanner;
        private SourceCache<DriveItem, string> files = new SourceCache<DriveItem, string>(x => x.Id);

        private SourceCache<DriveItem, string> folders = new SourceCache<DriveItem, string>(x => x.Id);

        public ReadOnlyObservableCollection<DupeViewModel> Duplicates { get; }
        public ReadOnlyObservableCollection<FolderViewModel> FolderTree { get; }
        public ReadOnlyObservableCollection<CommonFolderGroupedDupesViewModel> GroupedDuplicates { get; }

        public FolderViewModel LastSelectedItem { get; set; }
        [Reactive] public DupeViewModel SelectedDupe { get; set; }
        [Reactive] public DupeViewModel SelectedGroupedDupe { get; set; }
        [Reactive] public bool IncludeSubfolders { get; set;  }

        public bool ReversedIncludeSubfolders => reversedIncludeSubfolders.Value;
        private ObservableAsPropertyHelper<bool> reversedIncludeSubfolders;

        public ReactiveCommand<Unit,Unit> FindDupesCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ConsolidateToShortestNameCommand { get; set; }
        public ReactiveCommand<Unit, Unit> ConsolidateGroupsToShortestNameCommand { get; set; }

        public string UrlPathSegment => "DuplicatesPage";

        public IScreen HostScreen { get; }
        
        public DuplicatesPageViewModel(IScreen screen = null)
        {
            HostScreen = screen ?? Locator.Current.GetService<IScreen>();

            var filteredDupes = files.Connect()
                .Transform(x => new ModdedDriveItem(x))
                .Filter(x => x.Hash != null)
                .Group(x => x.Hash)
                .Transform(x => new ViewModel.DupeViewModel(x))
                .Filter(Observable.Return<Func<DupeViewModel, bool>>(y => y.Count > 1), files.CountChanged.Select(x => Unit.Default))
                .RefCount();

            filteredDupes.ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out var duplicates)
                .Subscribe();
            Duplicates = duplicates;

            var groupedFiltered = files.Connect()
                .Transform(x => new ModdedDriveItem(x))
                .Filter(x => x.Hash != null)
                .Group(x => x.ParentReference.Id)
                .Transform(x => new ViewModel.CommonFolderGroupedDupesViewModel(x, files))
                .Filter(Observable.Return<Func<CommonFolderGroupedDupesViewModel, bool>>(y => y.Children.Count > 0), files.CountChanged.Select(x => Unit.Default))
                .RefCount();

            groupedFiltered.Bind(out var groupedDuplicates)
                .Subscribe();
            GroupedDuplicates = groupedDuplicates;

            reversedIncludeSubfolders = this.WhenAnyValue(x => x.IncludeSubfolders).Select(x => !x).ToProperty(this, x => x.ReversedIncludeSubfolders);

            folders.Connect()
                .Transform(x => new FolderViewModel(x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out var folderTree)
                .Subscribe();
            FolderTree = folderTree;

            FindDupesCommand = ReactiveCommand.CreateFromTask<Unit, Unit>(async x =>
            {
                if (LastSelectedItem != null)
                {
                    files.Clear();
                    var items = scanner.GetFilesInFolder(LastSelectedItem.Id, IncludeSubfolders);
                    await foreach (var item in items)
                    {
                        files.AddOrUpdate(item);
                    }
                }
                return Unit.Default;
            });


            var nongroupedPlan = filteredDupes.ToCollection().Select(x => x.Count > 0 ? true : false).And(FindDupesCommand.CanExecute).Then((x, y) => x && y);

            ConsolidateToShortestNameCommand = ReactiveCommand.CreateFromTask<Unit, Unit>(async x =>
            {
                var listToDelete = new List<string>();
                                
                foreach (var dupe in Duplicates)
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
                files.Clear();
                return Unit.Default;
            }, Observable.When(nongroupedPlan));

            var groupPlan = groupedFiltered.ToCollection().Select(x => x.Count > 0 ? true : false).And(FindDupesCommand.CanExecute).Then((x, y) => x && y);

            ConsolidateGroupsToShortestNameCommand = ReactiveCommand.CreateFromTask<Unit, Unit>(async x =>
            {
                var listToDelete = new List<string>();

                foreach (var group in GroupedDuplicates)
                {
                    foreach (var dupe in group.Children)
                    {
                        var min = dupe.DuplicateItems.Min(x => x.Name.Length);
                        var shortest = dupe.DuplicateItems.FirstOrDefault(x => x.Name.Length == min);
                        foreach (var item in dupe.DuplicateItems.Except(new[] { shortest }))
                        {
                            listToDelete.Add(item.Id);
                        }
                    }
                }
               
                if (listToDelete.Count > 0)
                {
                    await scanner.DeleteBatchFileAsync(listToDelete);
                }
                files.Clear();
                return Unit.Default;
            }, Observable.When(groupPlan));

            

            Observable.FromAsync(async x =>
            {
                //Get the Graph client from the provider
                var graphClient = ProviderManager.Instance.GlobalProvider.GetClient();
                //var graphClient = MainPage.MsalProvider.GetClient();
                scanner = new ScannerService(graphClient);
                try
                {
                    var items = scanner.GetFolders();
                    await foreach (var item in items)
                    {
                        folders.AddOrUpdate(item);
                    }

                    //var items = scanner.GetFiles();
                    //var count = 0;
                    //await foreach (var item in items)
                    //{
                    //    files.AddOrUpdate(item);
                    //}

                    //Debug.WriteLine(count);

                }
                catch (ServiceException ex)
                {
                    //ShowNotification($"Exception getting events: {ex.Message}");
                }
            }).Subscribe();
        }
    }
}
