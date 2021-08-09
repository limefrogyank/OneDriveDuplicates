using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Graph;
using Newtonsoft.Json;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System.Diagnostics;
using OneDriveDuplicates.Service;
using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;
using System.Net.Http;
using DynamicData;
using System.Reactive.Linq;
using ReactiveUI;
using System.Collections.ObjectModel;
using OneDriveDuplicates.ViewModel;
using OneDriveDuplicates.Model;
using System.Reactive;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace OneDriveDuplicates
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DuplicatesPage : Page, IViewFor<DuplicatesPageViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
               .Register(nameof(ViewModel), typeof(DuplicatesPageViewModel), typeof(DuplicatesPage), null);

        public DuplicatesPage()
        {
            //ViewModel = Bootstrapper.BuildMainViewModel();
            this.InitializeComponent();


        }

        public DuplicatesPageViewModel ViewModel
        {
            get => (DuplicatesPageViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (DuplicatesPageViewModel)value;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            

            base.OnNavigatedTo(e);
        }

        private void ShowNotification(string message)
        {
            // Get the main page that contains the InAppNotification
            var mainPage = (Window.Current.Content as Frame).Content as MainPage;

            // Get the notification control
            var notification = mainPage.FindName("Notification") as InAppNotification;

            notification.Show(message);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            //if (LastSelectedItem != null)
            //{
            //    files.Clear();
            //    var items = scanner.GetFilesInFolder(LastSelectedItem.Id,false);
            //    await foreach (var item in items)
            //    {
            //        files.AddOrUpdate(item);
            //    }
            //}
        }

        private void TreeView_ItemInvoked(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            Debug.WriteLine(args.InvokedItem);

            ViewModel.LastSelectedItem = args.InvokedItem as FolderViewModel;
        }
    }
}
