using Microsoft.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.ApplicationModel;
using WinRT.Interop;
using Microsoft.UI.Windowing;
using Microsoft.UI.Dispatching;
using ZoDream.Shared.ViewModel;
using ZoDream.Shared.Interfaces;
using System.Threading.Tasks;
using ZoDream.Shared.Logging;

namespace ZoDream.Archiver.ViewModels
{
    internal partial class AppViewModel
    {
        public AppViewModel()
        {
            BackCommand = new RelayCommand(_ => {
                NavigateBack();
            });
        }
        private Window _baseWindow;
        private IntPtr _baseWindowHandle;
        private AppWindow _appWindow;

        /// <summary>
        /// UI线程.
        /// </summary>
        public DispatcherQueue DispatcherQueue => _baseWindow!.DispatcherQueue;

        /// <summary>
        /// 获取当前版本号.
        /// </summary>
        /// <returns>版本号.</returns>
        public string Version {
            get {
                var version = Package.Current.Id.Version;
                return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }
        }

        public ILogger Logger { get; private set; } = new EventLogger();
        public PluginViewModel Plugin { get; private set; } = new();

        public ISettingContainer Setting { get; private set; } = new SettingContainer();


        public IEntryService Service 
        {
            get {
                var temporary = Setting.Get<string>(SettingNames.TemporaryPath);
                return new EntryService(Logger, string.IsNullOrWhiteSpace(temporary) ? new TemporaryStorage() : new Shared.IO.TemporaryStorage(temporary));
            }
        }

        public void Binding(Window window, Frame frame)
        {
            _baseWindow = window;
            _baseWindowHandle = WindowNative.GetWindowHandle(_baseWindow);
            var windowId = Win32Interop.GetWindowIdFromWindow(_baseWindowHandle);
            _appWindow = AppWindow.GetFromWindowId(windowId);
            _rootFrame = frame;
            Startup();
        }

        public double GetDpiScaleFactorFromWindow()
        {
            return BaseXamlRoot.RasterizationScale;
        }

        public async Task InitializeAsync()
        {
            await Setting.LoadAsync();
        }
    }
}
