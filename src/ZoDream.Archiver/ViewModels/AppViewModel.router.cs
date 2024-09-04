using Microsoft.UI.Xaml.Controls;
using ZoDream.Archiver.Pages;

namespace ZoDream.Archiver.ViewModels
{
    internal partial class AppViewModel
    {
        private Frame _rootFrame;

        public void Navigate<T>() where T : Page
        {
            _rootFrame.Navigate(typeof(T));
            BackEnabled = typeof(T) != typeof(StartupPage);
        }

        public void Navigate<T>(object parameter) where T : Page
        {
            _rootFrame.Navigate(typeof(T), parameter);
            BackEnabled = typeof(T) != typeof(StartupPage);
        }

        public void NavigateBack()
        {
            _rootFrame.GoBack();
            BackEnabled = false;
        }
    }
}
