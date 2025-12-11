namespace Course31_trpo.Controls
{
    public partial class Square : ContentView
    {
        public Square() { SizeChanged += UpdateSize; }

        private void UpdateSize(object? obj, EventArgs args)
        {
            double minSize = Math.Min(Frame.Width, Frame.Height);
            if (minSize < 0) { return; }
            WidthRequest = minSize;
            HeightRequest = minSize;
        }
    }
}
