using Microsoft.UI.Xaml.Controls;

namespace ZoDream.Archiver.ViewModels
{
    internal partial class AppViewModel
    {
        private Frame _rootFrame;

        public void Navigate<T>() where T : Page
        {
            _rootFrame.Navigate(typeof(T));
        }

        public void Navigate<T>(object parameter) where T : Page
        {
            _rootFrame.Navigate(typeof(T), parameter);
        }

        public void NavigateBack()
        {
            _rootFrame.GoBack();
        }
    }
}
