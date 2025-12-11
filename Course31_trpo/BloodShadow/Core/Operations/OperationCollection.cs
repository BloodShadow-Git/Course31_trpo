namespace BloodShadow.Core.Operations
{
    using ObservableCollections;
    using R3;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class OperationCollection : Operation
    {
        public override ReadOnlyReactiveProperty<float> Progress => _progress;
        public override ReactiveProperty<bool> AllowSceneActivation => _allowSceneActivation;
        public override ReadOnlyReactiveProperty<bool> IsDone => _isDone;
        public override ReactiveProperty<int> Priority => _priority;
        public override ReadOnlyReactiveProperty<string> Description => _description;

        private readonly ReactiveProperty<float> _progress;
        private readonly ReactiveProperty<bool> _allowSceneActivation;
        private readonly ReactiveProperty<bool> _isDone;
        private readonly ReactiveProperty<int> _priority;
        private readonly ReactiveProperty<string> _description;

        public IObservableCollection<Operation> Operations => _operations;

        private readonly ObservableList<Operation> _operations;
        private readonly Dictionary<Operation, IDisposable> _disposeables;
        private Task _waitTask;
        private CancellationTokenSource _waitTaskSource;

        public OperationCollection() : base()
        {
            _operations = [];
            _disposeables = [];
            _awaiter = new OperationAwaiter(this);

            _progress = new ReactiveProperty<float>();
            _allowSceneActivation = new ReactiveProperty<bool>();
            _isDone = new ReactiveProperty<bool>();
            _priority = new ReactiveProperty<int>();
            _description = new ReactiveProperty<string>();

            _allowSceneActivation.Subscribe(_ => _operations.Where((op) =>
            { return op != null; }).ToList().ForEach((op) => { op.AllowSceneActivation.Value = AllowSceneActivation.Value; }));

            _priority.Subscribe(_ => _operations.Where((op) => { return op != null; }).ToList().ForEach((op) => { op.Priority.Value = _priority.Value; }));

            _operations.ObserveAdd().Subscribe(operation =>
            {
                if (_disposeables.ContainsKey(operation.Value)) { return; }
                CompositeDisposable disposables =
                [
                    operation.Value.Progress.Subscribe(_ => UpdateOperation()),
                    operation.Value.AllowSceneActivation.Subscribe(_ => UpdateOperation()),
                    operation.Value.IsDone.Subscribe(_ => UpdateOperation()),
                    operation.Value.Priority.Subscribe(_ => UpdateOperation())
                ];
                _disposeables.Add(operation.Value, disposables);
                UpdateOperation();
            });
            _operations.ObserveRemove().Subscribe(operation =>
            {
                if (_disposeables.TryGetValue(operation.Value, out IDisposable disposable))
                {
                    disposable?.Dispose();
                    _disposeables.Remove(operation.Value);
                    UpdateOperation();
                }
            });
        }

        private void UpdateOperation()
        {
            _progress.Value = _operations.Where((op) => { return op != null; }).Average((op) => { return op.Progress.CurrentValue; });
            _allowSceneActivation.Value = _operations.Where((op) => { return op != null; }).All((op) => { return op.AllowSceneActivation.Value; });
            _isDone.Value = _operations.Where((op) => { return op != null; }).All((op) => { return op.IsDone.CurrentValue; });
            _priority.Value = (int)_operations.Where((op) => { return op != null; }).Average((op) => { return op.Priority.Value; });
            _description.Value = $"{_operations.Count(input => input.IsDone.CurrentValue)}/{_operations.Count}";
        }

        public OperationCollection(Operation operation) : this() { Add(operation); }
        public OperationCollection(IEnumerable<Operation> operations) : this() { Add(operations); }
        public OperationCollection(params Operation[] operations) : this() { Add(operations); }
        public OperationCollection(Operation operation, IEnumerable<Operation> operations) : this(operations.ToArray()) { Add(operation); }

        public OperationCollection Merge(OperationCollection operation) { return new OperationCollection(_operations.Concat(operation._operations)); }
        public OperationCollection Merge(IEnumerable<OperationCollection> operations)
        {
            OperationCollection result = new(_operations);
            foreach (OperationCollection operation in operations) { result = result.Merge(operation); }
            return result;
        }

        public async void UpdateWait()
        {
            _waitTaskSource?.Cancel();
            if (_waitTask != null) { await _waitTask; }
            _waitTaskSource?.Dispose();

            _waitTaskSource = new CancellationTokenSource();
            _waitTask = Await();
        }

        private async Task Await()
        {
            try
            {
                List<Operation> ops = [.. _operations.Where((op) => { return op != null; })];
                foreach (Operation operation in ops)
                {
                    if (!_waitTaskSource.IsCancellationRequested) { await operation; }
                    else { break; }
                }
                _isDone.Value = true;
                OnCompletedAction?.Invoke();
                return;
            }
            catch { }
        }

        public void Add(Operation operation)
        {
            if (operation == null) { return; }
            _operations.Add(operation);
        }

        public void Add(IEnumerable<Operation> operations) { foreach (Operation operation in operations) { Add(operation); } }

        public override async void Dispose()
        {
            foreach (Operation operation in _operations) { if (operation is IDisposable disposable) { disposable?.Dispose(); } }
            _waitTaskSource?.Cancel();
            if (_waitTask != null) { await _waitTask; }
            _waitTask?.Dispose();
            _waitTaskSource?.Dispose();
        }
        public override OperationAwaiter GetAwaiter()
        {
            UpdateWait();
            return base.GetAwaiter();
        }
        public override object Clone() => new OperationCollection(_operations);
    }
}
