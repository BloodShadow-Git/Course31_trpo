using R3;
using System.Runtime.CompilerServices;

namespace BloodShadow.Core.Operations
{
    public abstract class Operation : IDisposable, ICloneable
    {
        public abstract ReadOnlyReactiveProperty<float> Progress { get; }
        public abstract ReactiveProperty<bool> AllowSceneActivation { get; }
        public abstract ReadOnlyReactiveProperty<bool> IsDone { get; }
        public abstract ReactiveProperty<int> Priority { get; }
        public abstract ReadOnlyReactiveProperty<string> Description { get; }
        protected OperationAwaiter _awaiter;

        public event Action OnCompleted
        {
            add => OnCompletedAction += value;
            remove => OnCompletedAction -= value;
        }
        protected Action OnCompletedAction;

        public abstract void Dispose();
        public abstract object Clone();
        public virtual void Wait() { while (!IsDone.CurrentValue) { } }
        public virtual void AddCompleted(Action action) { OnCompleted += action; }
        public virtual Operation Start() { return this; }
        public virtual OperationAwaiter GetAwaiter()
        {
            _awaiter ??= new OperationAwaiter(this);
            return _awaiter;
        }

        public class OperationAwaiter(Operation operation) : INotifyCompletion, IDisposable
        {
            protected virtual Operation Operation { get; set; } = operation;

            public virtual bool IsCompleted => Operation?.IsDone.CurrentValue ?? true;
            public virtual void OnCompleted(Action continuation)
            {
                if (IsCompleted) { continuation?.Invoke(); }
                else { Operation?.AddCompleted(continuation); }
            }
            public virtual void GetResult() { Operation.Wait(); }
            public virtual void Dispose() { try { Operation?.Dispose(); } catch { } }
        }

        public static implicit operator Task(Operation operation) => Task.Factory.StartNew(() => { operation.Wait(); });
    }
}
