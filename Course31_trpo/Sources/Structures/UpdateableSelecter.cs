using ObservableCollections;
using R3;

namespace Course31_trpo.Sources.Structures
{
    public class UpdateableSelecter<T> : ISelecter<UpdateableSelecterItem<T>>
    {
        public ReactiveCommand Prev { get; }
        public BindableReactiveProperty<int> DisplayValueIndex { get; }
        public IReadOnlyBindableReactiveProperty<UpdateableSelecterItem<T>> DisplayValue => _displayValue;
        public ReactiveCommand Next { get; }
        public ObservableList<UpdateableSelecterItem<T>> Values { get; }

        private readonly BindableReactiveProperty<UpdateableSelecterItem<T>> _displayValue;
        private CompositeDisposable _cd;

        public UpdateableSelecter()
        {
            Prev = new();
            Next = new();
            Values = [];
            DisplayValueIndex = new();
            _displayValue = new();
            _cd = [];

            Values.ObserveChanged().Subscribe(_ =>
            {
                if (DisplayValueIndex.Value >= Values.Count) { DisplayValueIndex.Value = Values.Count - 1; }
                _cd.Dispose();
                _cd = [];
                foreach (var value in Values) { _cd.Add(value.Avaiable.Subscribe(_ => { UpdateSelected(); })); }
                DisplayValueIndex.OnNext(DisplayValueIndex.Value);
            });
            Prev.Subscribe(_ =>
            {
                int cache = DisplayValueIndex.Value;
                cache--;
                if (cache < 0) { SetMin(cache); }
                else { DisplayValueIndex.Value = cache; }
            });
            Next.Subscribe(_ =>
            {
                int cache = DisplayValueIndex.Value;
                cache++;
                if (cache >= Values.Count) { SetMax(cache); }
                else { DisplayValueIndex.Value = cache; }
            });
            DisplayValueIndex.Subscribe(_ =>
            {
                if (Values.Count == 0 || DisplayValueIndex.Value < 0 || DisplayValueIndex.Value >= Values.Count) { return; }
                _displayValue.Value = Values[DisplayValueIndex.Value];
            });
        }

        public void SetMin(int value) { for (int i = value; i >= 0; i--) { if (Values[i].Avaiable.Value) { DisplayValueIndex.Value = i; return; } } }
        public void SetMax(int value) { for (int i = 0; i < value; i++) { if (Values[i].Avaiable.Value) { DisplayValueIndex.Value = i; return; } } }
        public void SetMin() => SetMin(DisplayValueIndex.Value);
        public void SetMax() => SetMax(DisplayValueIndex.Value);
        private void UpdateSelected()
        {
            Prev.ChangeCanExecute(true);
            Next.ChangeCanExecute(true);
            if (Values.Where(x => x.Avaiable.Value).Count() <= 1)
            {
                Prev.ChangeCanExecute(false);
                Next.ChangeCanExecute(false);
            }
            if (DisplayValueIndex.Value == -1) { return; }
            if (Values[DisplayValueIndex.Value].Avaiable.Value) { return; }
            for (int i = DisplayValueIndex.Value; i >= 0; i--) { if (Values[i].Avaiable.Value) { DisplayValueIndex.Value = i; return; } }
            for (int i = DisplayValueIndex.Value; i < Values.Count; i++) { if (Values[i].Avaiable.Value) { DisplayValueIndex.Value = i; return; } }
            DisplayValueIndex.Value = -1;
            Prev.ChangeCanExecute(false);
            Next.ChangeCanExecute(false);
        }
    }
    public struct UpdateableSelecterItem<T>(T item)
    {
        public T Item { get; set; } = item;
        public BindableReactiveProperty<bool> Avaiable { get; } = new(true);
    }
}
