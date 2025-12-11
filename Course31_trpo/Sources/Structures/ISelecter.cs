using ObservableCollections;
using R3;

namespace Course31_trpo.Sources.Structures
{
    public interface ISelecter<T>
    {
        public ReactiveCommand Prev { get; }
        public IReadOnlyBindableReactiveProperty<T> DisplayValue { get; }
        public BindableReactiveProperty<int> DisplayValueIndex { get; }
        public ReactiveCommand Next { get; }
        public ObservableList<T> Values { get; }
    }
}
