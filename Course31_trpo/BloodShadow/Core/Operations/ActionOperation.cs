using R3;
using System.Runtime.CompilerServices;

namespace BloodShadow.Core.Operations
{
    public class ActionOperation : Operation
    {
        public override ReadOnlyReactiveProperty<float> Progress => _progress;
        public override ReactiveProperty<bool> AllowSceneActivation => _allowSceneActivation;
        public override ReadOnlyReactiveProperty<bool> IsDone => _isDone;
        public override ReactiveProperty<int> Priority => _priority;
        public override ReadOnlyReactiveProperty<string> Description => _description;

        protected readonly ReactiveProperty<float> _progress;
        protected readonly ReactiveProperty<bool> _allowSceneActivation;
        protected readonly ReactiveProperty<bool> _isDone;
        protected readonly ReactiveProperty<int> _priority;
        protected readonly ReactiveProperty<string> _description;
        protected readonly CancellationTokenSource _tokenSource;
        protected ActionOperationProgress _aop;

        protected readonly CompositeDisposable _compositeDisposable;

        private readonly Task _task;

        protected ActionOperation()
        {
            _progress = new ReactiveProperty<float>();
            _allowSceneActivation = new ReactiveProperty<bool>();
            _isDone = new ReactiveProperty<bool>();
            _priority = new ReactiveProperty<int>();
            _description = new ReactiveProperty<string>();
            _tokenSource = new CancellationTokenSource();
            _compositeDisposable = new();
        }
        public ActionOperation(Func<ActionOperationProgress, Task> func) : this()
        {
            _aop = new ActionOperationProgress();
            SetupAOP();
            _task = func.Invoke(_aop);

            SetupAwaiter();
        }
        public ActionOperation(Action<ActionOperationProgress> action) : this()
        {
            _aop = new ActionOperationProgress();
            SetupAOP();
            _task = new Task(() => action?.Invoke(_aop), _tokenSource.Token);
            SetupAwaiter();
        }
        public ActionOperation(Func<Task> func) : this()
        {
            _task = func.Invoke();
            SetupAwaiter();
        }
        public ActionOperation(Action action) : this()
        {
            _task = new Task(action, _tokenSource.Token);
            SetupAwaiter();
        }
        public ActionOperation(Task task) : this()
        {
            _task = task;
            SetupAwaiter();
        }
        private ActionOperation(ActionOperation ao)
        {
            _task = ao._task;
            if (ao._aop != null)
            {
                _aop = ao._aop;
                SetupAOP();
            }
            SetupAwaiter();
        }

        protected void SetupAOP()
        {
            _compositeDisposable.Add(_aop.Progress.Subscribe(_ => _progress.Value = _aop.Progress.CurrentValue));
            _compositeDisposable.Add(_allowSceneActivation.Subscribe(_ => _aop.AllowSceneActivation.Value = _allowSceneActivation.CurrentValue));
            _compositeDisposable.Add(_aop.IsDone.Subscribe(_ => _isDone.Value = _aop.IsDone.CurrentValue));
            _compositeDisposable.Add(_priority.Subscribe(_ => _aop.Priority.Value = _priority.CurrentValue));
            _compositeDisposable.Add(_aop.Description.Subscribe(_ => _description.Value = _aop.Description.CurrentValue));
        }

        private void SetupAwaiter()
        {
            _awaiter = new OperationAwaiter(this);
            _progress.Value = 0f;

            Task.Run(() =>
            {
                while (!_tokenSource.IsCancellationRequested && !(_task?.IsCompleted ?? false)) { _isDone.Value = false; }
                Dispose();
            });
        }

        public override Operation Start()
        {
            if (_task.Status == TaskStatus.Created) { _task.Start(); }
            return this;
        }
        public override object Clone() => new ActionOperation(this);
        public override void Dispose()
        {
            _tokenSource?.Cancel();
            _task?.Dispose();
            _compositeDisposable?.Dispose();

            _progress.Value = 1f;
            _isDone.Value = true;
            OnCompletedAction?.Invoke();
        }

        public override OperationAwaiter GetAwaiter()
        {
            Start();
            return base.GetAwaiter();
        }
    }

    public class ActionOperation<T> : ActionOperation
    {
        public T Result { get; private set; }
        new protected OperationAwaiter _awaiter;

        private readonly Task<T> _task;

        public ActionOperation(Func<ActionOperationProgress, Task<T>> func) : base()
        {
            _aop = new ActionOperationProgress();
            SetupAOP();
            _task = func.Invoke(_aop);
            SetupAwaiter();
        }
        public ActionOperation(Func<ActionOperationProgress, T> action) : base()
        {
            _aop = new ActionOperationProgress();
            SetupAOP();
            _task = new(() => action.Invoke(_aop), _tokenSource.Token);
            SetupAwaiter();
        }
        public ActionOperation(Func<Task<T>> func) : base()
        {
            _task = func.Invoke();
            SetupAwaiter();
        }
        public ActionOperation(Func<T> func) : base()
        {
            _task = new(func, _tokenSource.Token);
            SetupAwaiter();
        }
        public ActionOperation(Task<T> task) : base()
        {
            _task = task;
            SetupAwaiter();
        }
        private ActionOperation(ActionOperation<T> ao)
        {
            _task = ao._task;
            if (ao._aop != null)
            {
                _aop = ao._aop;
                SetupAOP();
            }
            SetupAwaiter();
        }
        private void SetupAwaiter()
        {
            _awaiter = new OperationAwaiter(this);
            _progress.Value = 0f;

            Task.Run(() =>
            {
                while (!_tokenSource.IsCancellationRequested && !(_task?.IsCompleted ?? false)) { _isDone.Value = false; }
                Dispose();
            });
        }
        new public virtual OperationAwaiter GetAwaiter()
        {
            Start();
            _awaiter ??= new OperationAwaiter(this);
            return _awaiter;
        }

        public override ActionOperation Start()
        {
            if (_task.Status == TaskStatus.Created) { _task.Start(); }
            return this;
        }
        public override object Clone() => new ActionOperation<T>(this);
        public override void Dispose()
        {
            _tokenSource?.Cancel();
            _task?.Dispose();
            _compositeDisposable?.Dispose();

            _progress.Value = 1f;
            Result = _task.Result;
            _isDone.Value = true;
            OnCompletedAction?.Invoke();
        }

        new public class OperationAwaiter : INotifyCompletion, IDisposable
        {
            protected virtual ActionOperation<T> Operation { get; set; }
            public OperationAwaiter(ActionOperation<T> operation) => Operation = operation;
            public virtual bool IsCompleted => Operation?.IsDone.CurrentValue ?? false;
            public virtual void OnCompleted(Action continuation)
            {
                if (IsCompleted) { continuation?.Invoke(); }
                else { Operation?.AddCompleted(continuation); }
            }
            public virtual T GetResult()
            {
                Operation.Wait();
                return Operation.Result;
            }
            public virtual void Dispose() { try { Operation?.Dispose(); } catch { } }
        }
    }

    public class ActionOperationProgress : IDisposable
    {
        public readonly ReactiveProperty<float> Progress;
        public readonly ReactiveProperty<bool> AllowSceneActivation;
        public readonly ReactiveProperty<bool> IsDone;
        public readonly ReactiveProperty<int> Priority;
        public readonly ReactiveProperty<string> Description;

        public ActionOperationProgress()
        {
            Progress = new ReactiveProperty<float>();
            AllowSceneActivation = new ReactiveProperty<bool>();
            IsDone = new ReactiveProperty<bool>();
            Priority = new ReactiveProperty<int>();
            Description = new ReactiveProperty<string>();
        }

        public void Dispose()
        {
            Progress.Dispose();
            AllowSceneActivation.Dispose();
            IsDone.Dispose();
            Priority.Dispose();
            Description.Dispose();
        }
    }
}
