using ObservableCollections;
using R3;

namespace Course31_trpo.Sources.Structures
{
    public class Selecter<T> : ISelecter<T>
    {
        public ReactiveCommand Prev { get; }
        public IReadOnlyBindableReactiveProperty<T> DisplayValue => _displayValue;
        public BindableReactiveProperty<int> DisplayValueIndex { get; }
        public ReactiveCommand Next { get; }
        public ObservableList<T> Values { get; }

        private readonly BindableReactiveProperty<T> _displayValue;

        public Selecter()
        {
            Prev = new();
            Next = new();
            Values = [];
            DisplayValueIndex = new();
            _displayValue = new();
            Values.ObserveChanged().Subscribe(_ =>
            {
                if (Values.Count == 0)
                {
                    Prev.ChangeCanExecute(false);
                    Next.ChangeCanExecute(false);
                }
                else
                {
                    Prev.ChangeCanExecute(true);
                    Next.ChangeCanExecute(true);
                }
                if (DisplayValueIndex.Value >= Values.Count) { DisplayValueIndex.Value = Values.Count - 1; }
                if (DisplayValueIndex.Value < 0 && Values.Count > 0) { DisplayValueIndex.Value = 0; }
                DisplayValueIndex.OnNext(DisplayValueIndex.Value);
            });
            Prev.Subscribe(_ =>
            {
                int cache = DisplayValueIndex.Value;
                cache--;
                if (cache < 0) { DisplayValueIndex.Value = Values.Count - 1; }
                else { DisplayValueIndex.Value = cache; }
            });
            Next.Subscribe(_ =>
            {
                int cache = DisplayValueIndex.Value;
                cache++;
                if (cache >= Values.Count) { DisplayValueIndex.Value = 0; }
                else { DisplayValueIndex.Value = cache; }
            });
            DisplayValueIndex.Subscribe(_ =>
            {
                if (Values.Count == 0 || DisplayValueIndex.Value < 0 || DisplayValueIndex.Value >= Values.Count) { return; }
                _displayValue.Value = Values[DisplayValueIndex.Value];
            });
        }
    }
}
