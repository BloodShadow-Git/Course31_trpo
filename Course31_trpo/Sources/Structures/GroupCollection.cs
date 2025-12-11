using ObservableCollections;
using R3;

namespace Course31_trpo.Sources.Structures
{
    public partial class GroupCollection<TKey, TValue>
    {
        public TKey Name { get; }
        public NotifyCollectionChangedSynchronizedViewList<TValue> DisplayList => _displayList.ToNotifyCollectionChanged();
        public ReactiveCommand SwitchCommand { get; }
        public IReadOnlyBindableReactiveProperty<string> Image => _image;

        private readonly ObservableList<TValue> _displayList;
        private readonly BindableReactiveProperty<string> _image;
        private readonly IEnumerable<TValue> _items;
        private bool _expanded;

        private const string EXPAND = "minus_hexagon.png";
        private const string COLLAPSE = "plus_hexagon.png";

        public GroupCollection(TKey name, IEnumerable<TValue> reports)
        {
            Name = name;
            SwitchCommand = new();
            _displayList = [.. reports];
            _image = new();
            _expanded = false;
            _items = reports;
            SwitchCommand.Subscribe(_ =>
            {
                _expanded = !_expanded;
                _displayList.Clear();
                if (_expanded)
                {
                    _image.Value = EXPAND;
                    _displayList.AddRange(_items);
                }
                else { _image.Value = COLLAPSE; }
            });
            SwitchCommand.Execute(Unit.Default);
        }
    }
}
