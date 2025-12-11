using ObservableCollections;
using R3;
using Font = Microsoft.Maui.Graphics.Font;

namespace Course31_trpo.Drawables
{
    public class PieChart : Graph
    {
        public ObservableList<PieChartData> Values { get; }

        private List<ValueData> _valuesOld;
        private List<ValueData> _valuesNew;
        private readonly IAnimatable _animatable;
        private RectF _lastRect;
        private readonly ReactiveProperty<Color[]> _scaleColors;

        public PieChart(params PieChartData[] values) : base()
        {
            Values = [.. values];

            _valuesOld = [];
            _valuesNew = [];
            _animatable = new Button();
            _onSelect = new();
            _scaleColors = new(Static.GenerateColors(Values.Count));

            Fill.Select(_ => Unit.Default)
                .Merge(DrawProgress.Select(_ => Unit.Default))
                .Subscribe(_ => ShouldUpdate.Value = true);
            Values.ObserveCountChanged().Subscribe(_ => _scaleColors.Value = Static.GenerateColors(Values.Count));
            _scaleColors.Subscribe(_ => UpdateValues());
            Click.Subscribe(Check);

            UpdateValues(false);
        }

        public override void Draw(ICanvas canvas, RectF dirtyRect)
        {
            ShouldUpdate.Value = false;
            float sum = Values.Sum(x => x.Value);
            float step = 360f / (sum == 0 ? 1 : sum);

            float angle = 0f;
            for (int i = 0; i < _valuesNew.Count; i++)
            {
                ValueData vd = _valuesNew[i];
                vd.Angle = step * _valuesNew[i].Value.Value;
                _valuesNew[i] = vd;
                float endAngle = (angle + float.Lerp(_valuesOld[i].Angle, _valuesNew[i].Angle, DrawProgress.CurrentValue)) * Fill.CurrentValue;

                DrawPie(canvas, dirtyRect, angle, endAngle, Static.Lerp(_valuesOld[i].Color, _valuesNew[i].Color, DrawProgress.CurrentValue), _valuesNew[i].Value.Name);
                angle = endAngle;

                if (angle >= 360f) { break; }
            }

            _lastRect = dirtyRect;
        }

        private void Check(TouchEventArgs arg)
        {
            float radius = Math.Min(_lastRect.Height, _lastRect.Width) / 2f;
            if (arg.Touches.Length < 1 && radius <= 0) { return; }
            PointF clickPos = arg.Touches[0];
            if (clickPos.Distance(_lastRect.Center) > radius) { return; }
            double angle = 180f - clickPos.GetAngle(_lastRect.Center);
            double prevAngle = 0;
            for (int i = 0; i < _valuesNew.Count; i++)
            {
                prevAngle += _valuesNew[i].Angle;
                if (prevAngle > angle)
                {
                    _onSelect.OnNext(i);
                    break;
                }
            }
        }

        private static void DrawPie(ICanvas canvas, RectF dirtyRect, float startAngle, float endAngle, Color color, string text = "", float fontSize = 16f)
        {
            canvas.FillColor = color;
            canvas.FontSize = fontSize;
            canvas.Font = Font.Default;
            endAngle = Math.Clamp(endAngle, startAngle, 360f);
            if ((endAngle - startAngle) % 360 == 0)
            {
                canvas.FillEllipse(dirtyRect);
                canvas.DrawString(text, dirtyRect, HorizontalAlignment.Center, VerticalAlignment.Center);
            }
            else
            {
                PathF path = new();
                path.MoveTo(dirtyRect.Center);
                path.AddArc(PointF.Zero, new PointF(dirtyRect.Width, dirtyRect.Height), startAngle, endAngle, false);
                canvas.FillPath(path);

                float angleRad = -((endAngle - startAngle) / 2f + startAngle) * (float)Math.PI / 180f;
                float radius = Math.Min(dirtyRect.Height, dirtyRect.Width) / 2f * .6f;
                canvas.DrawString(text, new RectF(new((float)Math.Cos(angleRad) * radius + dirtyRect.Center.X,
                    (float)Math.Sin(angleRad) * radius * (dirtyRect.Height / dirtyRect.Width) + dirtyRect.Center.Y),
                    canvas.GetStringSize(text, Font.Default, fontSize * 1.25f)), HorizontalAlignment.Center, VerticalAlignment.Center);
            }
        }

        private void UpdateValues(bool animate = true)
        {
            int count = Math.Max(Values.Count, _valuesNew.Count);
            _valuesNew.AddRange(Enumerable.Repeat(new ValueData(new(0), new(), true), count - _valuesNew.Count));
            _valuesOld.AddRange(Enumerable.Repeat(new ValueData(new(0), new(), true), count - _valuesOld.Count));
            for (int i = 0; i < _scaleColors.CurrentValue.Length; i++) { _valuesNew[i] = new(Values[i], _scaleColors.CurrentValue[i], false); }

            _animatable.AbortAnimation(nameof(UpdateValues));
            if (animate && Animate)
            {
                Animation anim = new(v => { DrawProgress.Value = (float)v; }, 0, 1);
                anim.Commit(_animatable, nameof(UpdateValues), length: 2000, easing: Easing.Linear, finished: (_, canceled) =>
                {
                    if (canceled) { return; }
                    _valuesNew = [.. _valuesNew.Where(value => value.Delete)];
                    _valuesOld = [.. _valuesNew];
                });
            }
            else { DrawProgress.Value = 1f; }

            ShouldUpdate.Value = true;
        }

        private struct ValueData(PieChartData value, Color color, bool delete)
        {
            public PieChartData Value = value;
            public float Angle = 0;
            public Color Color = color;
            public bool Delete = delete;

            public ValueData() : this(new(0), new(0f, 0f, 0f, 1f), true) { }
        }
    }

    public struct PieChartData(float value, object? name = null)
    {
        public float Value { get; set; } = value;
        public string Name { get; set; } = name?.ToString() ?? value.ToString();
    }
}
