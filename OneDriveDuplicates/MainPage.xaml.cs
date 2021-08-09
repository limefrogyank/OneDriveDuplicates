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

using CommunityToolkit.Authentication;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using OneDriveDuplicates.ViewModel;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace OneDriveDuplicates
{
    
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MsalProvider MsalProvider { get; set; }

        public MainViewModel ViewModel { get; set; }

        public MainPage()
        {
            this.InitializeComponent();

            ViewModel = new MainViewModel();

            // Load OAuth settings

            var oauthSettings = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView("OAuth");
            var appId = oauthSettings.GetString("AppId");
            var scopes = oauthSettings.GetString("Scopes").Split(' ');

            var webAccountProviderConfig = new WebAccountProviderConfig(WebAccountProviderType.Msa, appId);

            Guid settingsCommandId = Guid.NewGuid();
            void OnSettingsCommandInvoked(IUICommand command)
            {
                System.Diagnostics.Debug.WriteLine("AccountsSettingsPane command invoked: " + command.Id);
            }

            var accountsSettingsPaneConfig = new AccountsSettingsPaneConfig(
            addAccountHeaderText: "Custom header text",
            commands: new List<SettingsCommand>()
            {
                new SettingsCommand(settingsCommandId: settingsCommandId, label: "Click me!", handler: OnSettingsCommandInvoked)
    });

            ProviderManager.Instance.ProviderStateChanged += Instance_ProviderStateChanged;
            ProviderManager.Instance.GlobalProvider = new WindowsProvider(scopes, autoSignIn:true);
            //MainPage.MsalProvider = new MsalProvider(appId, scopes);
            
            // Configure MSAL provider  
            //MsalProvider.ClientId = appId;
            //MsalProvider.Scopes = new ScopeSet(scopes.Split(' '));

            // Handle auth state change
            //ProviderManager.Instance.ProviderUpdated += ProviderUpdated;

            // Navigate to HomePage.xaml
            //RootFrame.Navigate(typeof(HomePage));
            
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //await ProviderManager.Instance.GlobalProvider.SignInAsync();
            //await ProviderManager.Instance.GlobalProvider.TrySilentSignInAsync();
            //try
            //{
            //    await MsalProvider.SignOutAsync();
            //    await MsalProvider.TrySilentSignInAsync();
            //}
            //catch
            //{
            //    await MsalProvider.SignInAsync();
            //}
        }

        private void Instance_ProviderStateChanged(object sender, ProviderStateChangedEventArgs e)
        {
            
            var globalProvider = ProviderManager.Instance.GlobalProvider;
            SetAuthState(globalProvider != null && globalProvider.State == ProviderState.SignedIn);
            ViewModel.Router.Navigate.Execute(new WelcomePageViewModel()).Subscribe();
            //RootFrame.Navigate(typeof(HomePage));
        }

        //private void ProviderUpdated(object sender, ProviderUpdatedEventArgs e)
        //{
        //    var globalProvider = ProviderManager.Instance.GlobalProvider;
        //    SetAuthState(globalProvider != null && globalProvider.State == ProviderState.SignedIn);
        //    RootFrame.Navigate(typeof(HomePage));
        //}

        private void SetAuthState(bool isAuthenticated)
        {
            (Application.Current as App).IsAuthenticated = isAuthenticated;

            // Toggle controls that require auth
            Calendar.IsEnabled = isAuthenticated;
            NewEvent.IsEnabled = isAuthenticated;
        }

        private void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var invokedItem = args.InvokedItem as string;

            switch (invokedItem.ToLower())
            {
                case "new event":
                    throw new NotImplementedException();
                    break;
                case "duplicates":
                    ViewModel.Router.Navigate.Execute(new DuplicatesPageViewModel()).Subscribe();
                    //RootFrame.Navigate(typeof(DuplicatesPage));
                    break;
                case "calendar":
                    //RootFrame.Navigate(typeof(CalendarPage));
                    break;
                case "home":
                default:
                    //RootFrame.Navigate(typeof(HomePage));
                    break;
            }
        }
    }
}
