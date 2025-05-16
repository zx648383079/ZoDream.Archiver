using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using ZoDream.Archiver.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ZoDream.Archiver.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CompressPage : Page
    {
        public CompressPage()
        {
            this.InitializeComponent();
        }

        public CompressViewModel ViewModel => (CompressViewModel)DataContext;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ViewModel.LoadAsync(e.Parameter);
        }
    }
}
