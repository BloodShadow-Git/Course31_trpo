using Course31_trpo.Pages.BaseClasses;
using Course31_trpo.Pages.Narrow;
using Course31_trpo.Pages.Wide;
using R3;

namespace Course31_trpo.Pages;

public partial class Home : BaseContentPage
{
    protected override PageSize[] Pages => [new(MauiProgram.NARROWWINDOWSIZE, new HomeNarrow()), new(new HomeWide())];
    public Home() { MauiProgram.HomeVM.Subscribe(_ => BindingContext = MauiProgram.HomeVM.CurrentValue); }
}