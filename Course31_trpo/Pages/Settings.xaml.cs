using R3;

namespace Course31_trpo.Pages;

public partial class Settings : ContentPage
{
    public Settings()
    {
        InitializeComponent();
        MauiProgram.SettingsVM.Subscribe(_ => BindingContext = MauiProgram.SettingsVM.CurrentValue);
    }
}