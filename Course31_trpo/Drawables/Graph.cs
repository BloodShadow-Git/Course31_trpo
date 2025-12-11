using R3;

namespace Course31_trpo.Drawables
{
    public abstract class Graph : IDrawable
    {
        public ReactiveProperty<float> Fill { get; } = new(1f);
        public BindableReactiveProperty<bool> ShouldUpdate { get; } = new();
        public bool Animate { get; set; } = true;
        public ReactiveCommand<TouchEventArgs> Click { get; } = new();
        public Observable<int> OnSelect => _onSelect;
        public BindableReactiveProperty<float> DrawProgress { get; } = new(1f);

        protected Subject<int> _onSelect = new();

        public abstract void Draw(ICanvas canvas, RectF dirtyRect);
    }
}
