using R3;

namespace Course31_trpo.Pages;

public partial class Loaded : ContentPage
{
    public Loaded()
    {
        InitializeComponent();
        MauiProgram.SettingsVM.Subscribe(_ => BindingContext = MauiProgram.LoadedItemsVM.CurrentValue);
    }
}