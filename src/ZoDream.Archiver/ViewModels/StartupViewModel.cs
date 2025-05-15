using System;
using System.Windows.Input;
using Windows.Storage.Pickers;
using ZoDream.Archiver.Pages;
using ZoDream.Shared.ViewModel;

namespace ZoDream.Archiver.ViewModels
{
    public class StartupViewModel : BindableBase
    {
        public StartupViewModel()
        {
            OpenCommand = new RelayCommand(TapOpen);
            DownloadCommand = new RelayCommand(TapDownload);
            OpenBundleCommand = new RelayCommand(TapOpenBundle);
            CreateCommand = new RelayCommand(TapCreate);
            ExplorerCommand = new RelayCommand(TapExplorer);
            version = App.ViewModel.Version;
        }

        private string version;

        public string Version {
            get => version;
            set => Set(ref version, value);
        }

        public ICommand OpenCommand { get; private set; }
        public ICommand OpenBundleCommand { get; private set; }
        public ICommand DownloadCommand { get; private set; }
        public ICommand ExplorerCommand { get; private set; }
        public ICommand CreateCommand { get; private set; }

        private void TapExplorer(object? _)
        {
            App.ViewModel.Navigate<ExplorerPage>();
        }

        private async void TapOpen(object? _)
        {
            var picker = new FileOpenPicker();
            //foreach (var ext in ReaderFactory.FileFilterItems)
            //{
            //    picker.FileTypeFilter.Add(ext);
            //}
            picker.FileTypeFilter.Add("*");
            App.ViewModel.InitializePicker(picker);
            var items = await picker.PickSingleFileAsync();
            if (items is null)
            {
                return;
            }
            App.ViewModel.Navigate<WorkspacePage>(items);
        }

        private void TapOpenFolder(object? _)
        {
            //var picker = new FolderPicker();
            //picker.FileTypeFilter.Add("*");
            //App.ViewModel.InitializePicker(picker);
            //picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            //var folder = await picker.PickSingleFolderAsync();
            //if (folder is null)
            //{
            //    return;
            //}
            //var checkFile = await folder.GetFileAsync(AppConstants.DatabaseFileName);
            //if (checkFile is null)
            //{
            //    // 不存在
            //    return;
            //}
            //StorageApplicationPermissions.FutureAccessList.AddOrReplace(AppConstants.WorkspaceToken, folder);
            //await App.GetService<AppViewModel>().InitializeWorkspaceAsync(folder);
            //App.GetService<IRouter>().GoToAsync(Router.HomeRoute);
        }
        private void TapOpenBundle(object? _)
        {
            App.ViewModel.Navigate<BundlePage>();
        }
        private void TapDownload(object? _)
        {
            App.ViewModel.Navigate<DownloadPage>();
        }
        private void TapCreate(object? _)
        {
            App.ViewModel.Navigate<CompressPage>();
        }
    }
}
