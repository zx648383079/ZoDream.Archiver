using Microsoft.UI.Xaml.Controls;
using ZoDream.Archiver.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ZoDream.Archiver.Dialogs
{
    public sealed partial class BundlePropertyDialog : ContentDialog
    {
        public BundlePropertyDialog()
        {
            this.InitializeComponent();
        }

        public BundlePropertyDialogViewModel ViewModel => (BundlePropertyDialogViewModel)DataContext;
    }
}
