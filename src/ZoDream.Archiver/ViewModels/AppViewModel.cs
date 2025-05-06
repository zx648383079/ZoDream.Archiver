using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using WinRT.Interop;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Logging;
using ZoDream.Shared.ViewModel;

namespace ZoDream.Archiver.ViewModels
{
    internal partial class AppViewModel: IDisposable
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
        private ILogger? _logger;

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

        public ILogger Logger => _logger ??= new EventLogger(CreateFileLog());
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

        public void Dispose()
        {
            Logger.Dispose();
        }

        public static ILogger CreateFileLog()
        {
            var fs = File.OpenWrite(LogFileName);
            fs.Seek(0, SeekOrigin.End);
            return new FileLogger(fs);
        }

        public static string LogFileName =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{DateTime.Now:yyyy-MM-dd}.log");
    }
}
