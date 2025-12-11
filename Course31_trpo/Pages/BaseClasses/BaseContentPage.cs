
namespace Course31_trpo.Pages.BaseClasses
{
    public abstract class BaseContentPage : ContentPage
    {
        protected abstract PageSize[] Pages { get; }

        protected BaseContentPage() { SizeChanged += UpdateContent; }

        private void UpdateContent(object? sender, EventArgs e)
        {
            if (Pages == null || Pages.Length < 1) { throw new Exception("No pages to update"); }
            if (Application.Current == null || Application.Current.Windows.Count < 1) { return; }
            IOrderedEnumerable<PageSize> sortedPages = Pages.OrderBy(x => x.MaxWidth);
            foreach (PageSize page in sortedPages) { if (Application.Current.Windows[0].Width < page.MaxWidth) { Content = page.View; return; } }
            Content = sortedPages.First().View;
        }

        protected struct PageSize(double maxWidth, ContentView view)
        {
            public double MaxWidth { get; set; } = maxWidth;
            public ContentView View { get; set; } = view;
            public PageSize(ContentView view) : this(double.MaxValue, view) { }
        }
    }
}
