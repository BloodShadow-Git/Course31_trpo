using System.Windows.Input;

namespace Course31_trpo.Controls
{
    public partial class BindableGraphicsView : GraphicsView
    {
        public static BindableProperty UpdateTriggerProperty = BindableProperty.Create(nameof(UpdateTrigger), typeof(bool), typeof(bool), propertyChanged: UpdateValue);
        public static BindableProperty OnClickProperty = BindableProperty.Create(nameof(OnClick), typeof(ICommand), typeof(ICommand));

        public static void UpdateValue(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(newValue as bool?) ?? false) { return; }
            BindableGraphicsView? ugv = (BindableGraphicsView?)bindable;
            if (ugv == null) { return; }
            ugv.Invalidate();
        }

        public bool UpdateTrigger
        {
            get => (bool)GetValue(UpdateTriggerProperty);
            set => SetValue(UpdateTriggerProperty, value);
        }

        public ICommand OnClick
        {
            get => (ICommand)GetValue(OnClickProperty);
            set => SetValue(OnClickProperty, value);
        }

        public BindableGraphicsView() { StartInteraction += InvokeClick; }

        private void InvokeClick(object? sender, TouchEventArgs e) { try { OnClick?.Execute(e); } catch { } }
    }
}