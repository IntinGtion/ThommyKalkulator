using System.Windows.Controls;
using ThommyKalkulator.WPF.ViewModels.Pages;

namespace ThommyKalkulator.WPF.Views.Pages;

public partial class MaterialsPage : UserControl
{
    public MaterialsPage()
    {
        InitializeComponent();
        DataContext = new MaterialsViewModel(App.AppState);
    }
}
