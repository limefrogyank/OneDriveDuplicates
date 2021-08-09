using DynamicData;
using OneDriveDuplicates.Model;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneDriveDuplicates.ViewModel
{
    public class CommonFolderGroupedDupesViewModel : ReactiveObject
    {
        public string Name { get; set; }
        public ReadOnlyObservableCollection<DupeViewModel> Children { get; }

        public CommonFolderGroupedDupesViewModel(IGroup<DupeViewModel, string, string> group)
        {
            group.Cache.Connect()
                .ToCollection()
                .Subscribe(x =>
                {
                    var first = x.FirstOrDefault();
                    if (first != null)
                    {
                        Name = first.ParentReference.Name;
                        //FirstFileId = first.File.Id;
                        //ParentReference = first.ParentReference;
                    }
                });

            group.Cache.Connect()
                .Bind(out var children)
                .Subscribe();
            Children = children;
        }
    }
}
