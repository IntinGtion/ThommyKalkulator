using System.Windows;

namespace ThommyKalkulator.WPF.Views;

public partial class FloatingPreviewWindow : Window
{
    public FloatingPreviewWindow()
    {
        InitializeComponent();
    }

    public void UpdateText(string text)
    {
        PreviewTextBlock.Text = text ?? string.Empty;
    }
}
