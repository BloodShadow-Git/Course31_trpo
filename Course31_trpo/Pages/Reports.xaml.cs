using R3;

namespace Course31_trpo.Pages;

public partial class Reports : ContentPage
{
    public Reports()
    {
        InitializeComponent();
        MauiProgram.SettingsVM.Subscribe(_ => BindingContext = MauiProgram.ReportVM.CurrentValue);
    }
}