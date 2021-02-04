using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// La plantilla de elemento Página en blanco está documentada en https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0xc0a

namespace CleanMyWindows
{
    /// <summary>
    /// Página vacía que se puede usar de forma independiente o a la que se puede navegar dentro de un objeto Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.SearchDirectory();
            this.LoadWhitelist();
        }
        private readonly UserDataPaths UserPaths = UserDataPaths.GetDefault();
        private readonly List<string> WhiteList = new List<string>();
        private async void SearchDirectory()
        {
            try
            {
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(UserPaths.RoamingAppData);
                var dirs = await folder.GetFoldersAsync();
                foreach (var d in dirs)
                {
                    RoamingDirList.Items.Add(d.DisplayName);
                    if (!WhiteList.Contains(d.ToString())) RoamingDirList.SelectedItems.Add(d);

                }
                folder = await StorageFolder.GetFolderFromPathAsync(UserPaths.LocalAppData);
                dirs = await folder.GetFoldersAsync();
                foreach (var d in dirs)
                {
                    LocalDirList.Items.Add(d.DisplayName);
                    if (!WhiteList.Contains(d.ToString())) LocalDirList.SelectedItems.Add(d);
                }
                folder = await StorageFolder.GetFolderFromPathAsync(UserPaths.LocalAppDataLow);
                dirs = await folder.GetFoldersAsync();
                foreach (var d in dirs)
                {
                    LocalLowDirList.Items.Add(d.DisplayName);
                    if (!WhiteList.Contains(d.ToString())) LocalLowDirList.SelectedItems.Add(d);
                }
            }
            catch
            {
                MessageDialog dlg = new MessageDialog("This application needs your permission to access the AppData folder");
                dlg.Commands.Add(new UICommand("Yes", new UICommandInvokedHandler(AskForFileSysPermission), 0));
                dlg.Commands.Add(new UICommand("No", new UICommandInvokedHandler(AskForFileSysPermission), 1));
                await dlg.ShowAsync();
            }
        }
        private async void AskForFileSysPermission(IUICommand command)
        {
            if ((int)command.Id == 0)
                await Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-broadfilesystemaccess"));
        }

        private async void LoadWhitelist()
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            try
            {
                StorageFile w = await folder.GetFileAsync("whitelist.txt");
                var text = await FileIO.ReadTextAsync(w);
                WhitelistREB.TextDocument.SetText(Windows.UI.Text.TextSetOptions.None, text);
                WhiteList.Clear();
                foreach (var i in text.Split('\n'))
                {
                    WhiteList.Add(i);
                }
                await Task.Delay(5000);
                await this.ApplyWhitelistAsync();
            }
            catch
            {
                WhitelistREB.TextDocument.SetText(Windows.UI.Text.TextSetOptions.None, "");
            }
        }

        private async void DeleteSelectedBtn_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(UserPaths.RoamingAppData);
            foreach (var i in RoamingDirList.SelectedItems)
            {
                try
                {
                    var a = await folder.GetFolderAsync(i.ToString());
                    await a.DeleteAsync();
                }
                catch (Exception err)
                {
                    MessageDialog m = new MessageDialog(err.Message);
                    await m.ShowAsync();
                }
                RoamingDirList.Items.Remove(i);
            }
            folder = await StorageFolder.GetFolderFromPathAsync(UserPaths.LocalAppData);
            foreach (var i in LocalDirList.SelectedItems)
            {
                var a = await folder.GetFolderAsync(i.ToString());
                await a.DeleteAsync();
                LocalDirList.Items.Remove(i);
            }
            folder = await StorageFolder.GetFolderFromPathAsync(UserPaths.LocalAppDataLow);
            foreach (var i in LocalLowDirList.SelectedItems)
            {
                var a = await folder.GetFolderAsync(i.ToString());
                await a.DeleteAsync();
                LocalLowDirList.Items.Remove(i);
            }
            MessageDialog dlg = new MessageDialog("Folders deleted :)");
            await dlg.ShowAsync();
        }

        private async void WhitelistBtn_Click(object sender, RoutedEventArgs e)
        {
            WhitelistREB.Document.GetText(Windows.UI.Text.TextGetOptions.None, out string text);
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile save = await folder.CreateFileAsync("whitelist.txt", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(save, text);
            WhiteList.Clear();
            foreach (var i in text.Split('\n'))
            {
                WhiteList.Add(i);
            }
            await this.ApplyWhitelistAsync();
        }

        private async Task ApplyWhitelistAsync()
        {
            RoamingDirList.SelectedItems.Clear();
            foreach (var i in RoamingDirList.Items)
            {
                var matching = WhiteList.Where(str => str.Contains(i.ToString()));
                if (matching.Count() == 0) RoamingDirList.SelectedItems.Add(i);
            }
            await Task.Delay(100);
            LocalDirList.SelectedItems.Clear();
            foreach (var i in LocalDirList.Items)
            {
                var matching = WhiteList.Where(str => str.Contains(i.ToString()));
                if (matching.Count() == 0) LocalDirList.SelectedItems.Add(i);
            }
            await Task.Delay(100);
            LocalLowDirList.SelectedItems.Clear();
            foreach (var i in LocalLowDirList.Items)
            {
                var matching = WhiteList.Where(str => str.Contains(i.ToString()));
                if (matching.Count() == 0) LocalLowDirList.SelectedItems.Add(i);
            }
        }
    }
}
