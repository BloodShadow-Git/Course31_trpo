using ObservableCollections;
using R3;

namespace Course31_trpo.Drawables
{
    public class Diogramma : Graph
    {
        #region Public properties
        public BindableReactiveProperty<uint> LinesCount { get; }
        public BindableReactiveProperty<float> LineThickness { get; }
        public BindableReactiveProperty<float> ValueThickness { get; }
        public BindableReactiveProperty<float> MinValue { get; }
        public BindableReactiveProperty<float> MaxValue { get; }
        public BindableReactiveProperty<DiogrammaAutoValues> AutoValues { get; }
        public BindableReactiveProperty<DiogrammaDrawMode> DrawMode { get; }
        public BindableReactiveProperty<string[]> ScaleNames { get; }
        public BindableReactiveProperty<float> FontSize { get; }
        public BindableReactiveProperty<float> Padding { get; }
        public ObservableList<DiogrammaValue> Values { get; }
        public NotifyCollectionChangedSynchronizedViewList<DiogrammaLegend> Legend => _legend.ToNotifyCollectionChanged();
        #endregion

        #region Private Fields
        private readonly ObservableList<DiogrammaLegend> _legend;
        private readonly ReactiveProperty<Color[]> _scaleColors;
        private readonly IAnimatable _animatable;
        private RectF _lastRect;
        #endregion

        #region Constructors
        public Diogramma(uint linesCount = 1,
            float lineThickness = 1f,
            float valueThickness = 1f,
            float minValue = 0f,
            float maxValue = 1f,
            float fontSize = 16f,
            float padding = 10f,
            DiogrammaAutoValues dav = DiogrammaAutoValues.All,
            DiogrammaDrawMode ddm = DiogrammaDrawMode.Dots,
            params DiogrammaValue[] values) : this([], linesCount, lineThickness, valueThickness, minValue, maxValue, fontSize, padding, dav, ddm, values) { }

        public Diogramma(string[] scaleNames,
            uint linesCount = 1,
            float lineThickness = 1f,
            float valueThickness = 1f,
            float minValue = 0f,
            float maxValue = 1f,
            float fontSize = 16f,
            float padding = 10f,
            DiogrammaAutoValues dav = DiogrammaAutoValues.None,
            DiogrammaDrawMode ddm = DiogrammaDrawMode.Dots,
            params DiogrammaValue[] values) : base()
        {
            LinesCount = new(linesCount);
            LineThickness = new(lineThickness);
            ValueThickness = new(valueThickness);
            MinValue = new(minValue);
            MaxValue = new(maxValue);
            AutoValues = new(dav);
            DrawMode = new(ddm);
            ScaleNames = new(scaleNames);
            FontSize = new(fontSize);
            Padding = new(padding);
            Values = [.. values];

            _legend = [];
            _scaleColors = new(Static.GenerateColors(Values.Count));
            _animatable = new Button();
            _lastRect = default;

            LinesCount.Select(_ => Unit.Default)
                .Merge(LineThickness.Select(_ => Unit.Default))
                .Merge(ValueThickness.Select(_ => Unit.Default))
                .Merge(MinValue.Select(_ => Unit.Default))
                .Merge(MaxValue.Select(_ => Unit.Default))
                .Merge(AutoValues.Select(_ => Unit.Default))
                .Merge(DrawMode.Select(_ => Unit.Default))
                .Merge(FontSize.Select(_ => Unit.Default))
                .Merge(Padding.Select(_ => Unit.Default))
                .Merge(DrawProgress.Select(_ => Unit.Default))
                .Merge(_scaleColors.Select(_ => Unit.Default))
                .Merge(MauiProgram.OnAppThemeChange.Select(_ => Unit.Default))
                .Merge(Values.ObserveChanged().Select(_ => Unit.Default))
                .Subscribe(_ => ShouldUpdate.Value = true);
            Values.ObserveCountChanged().Subscribe(_ => _scaleColors.Value = Static.GenerateColors(Values.Count));
            Values.ObserveChanged().Subscribe(_ => FillLegendForLinesDots());
            Click.Subscribe(Check);
        }
        #endregion

        public override void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.StrokeSize = LineThickness.CurrentValue;
            canvas.StrokeColor = Application.Current?.UserAppTheme == AppTheme.Dark ? new(1f, 1f, 1f, 1f) : new(0f, 0f, 0f, 1f);
            canvas.FontColor = Application.Current?.UserAppTheme == AppTheme.Dark ? new(1f, 1f, 1f, 1f) : new(0f, 0f, 0f, 1f);
            canvas.FontSize = FontSize.CurrentValue;
            if (LinesCount.CurrentValue < 2) { LinesCount.Value = 2; }
            if (AutoValues.CurrentValue.HasFlag(DiogrammaAutoValues.Max)) { AutoValue(MaxValue, x => x.Any() ? x.Max() : 1f); }
            if (AutoValues.CurrentValue.HasFlag(DiogrammaAutoValues.Min)) { AutoValue(MinValue, x => x.Any() ? x.Min() : 0f); }
            DiogrammaScale<float>[] scale =
                Static.GenerateScale(LinesCount.CurrentValue, MinValue.CurrentValue, MaxValue.CurrentValue, FontSize.CurrentValue, out SizeF maxSize);
            DiogrammaScale<string>[] scaleNames =
                Static.GenerateScale(ScaleNames.CurrentValue, Values.Count == 0 ? 0 : Values.Select(x => x.Values.Length).Max(), FontSize.CurrentValue, out SizeF namesMax);

            float scaleStep = 1f / (LinesCount.CurrentValue + 1);
            float valueStep = 1f / (Values.Count + 1);
            RectF gioRect = new(maxSize.Width, Padding.CurrentValue, dirtyRect.Width - maxSize.Width - Padding.CurrentValue,
                dirtyRect.Height - namesMax.Height - Padding.CurrentValue * 2);

            for (int i = 0; i < scale.Length; i++)
            {
                float lerpedY = float.Lerp(gioRect.Bottom, gioRect.Top, scaleStep * i);
                canvas.DrawLine(gioRect.Left, lerpedY, gioRect.Right, lerpedY);
                DrawScale(canvas, gioRect, scale[i], lerpedY);
            }

            _lastRect = gioRect;
            PointF[][] points = DrawValue(canvas, gioRect, maxSize, scaleNames);
            if (points.Length >= 1 && points[0].Length >= 1)
            {
                PointF startPoint = points[0][0];
                for (int value = 0; value < points.Length; value++)
                {
                    PathF background = new(new PointF(startPoint.X, gioRect.Y + gioRect.Height));
                    for (int point = 0; point < points[value].Length; point++) { background.LineTo(points[value][point]); }
                    background.LineTo(new PointF(points[^1][^1].X, gioRect.Y + gioRect.Height));
                    canvas.FillColor = new(_scaleColors.CurrentValue[value].Red, _scaleColors.CurrentValue[value].Green, _scaleColors.CurrentValue[value].Blue, .25f);
                    canvas.FillPath(background);
                }
            }

            ShouldUpdate.Value = false;
        }

        #region DrawValues
        private PointF[][] DrawValue(ICanvas canvas, RectF gioRect, SizeF maxSize,
            DiogrammaScale<string>[] valueNames)
        {
            DrawValueNamesHor(canvas, gioRect, valueNames, maxSize, out float[] positions);
            PointF[][] points = CalculatePoints(positions, gioRect);
            switch (DrawMode.CurrentValue)
            {
                case DiogrammaDrawMode.Dots: DrawDots(canvas, gioRect, points); break;
                case DiogrammaDrawMode.Lines: DrawLines(canvas, points); break;
                case DiogrammaDrawMode.DotsLines:
                    DrawDots(canvas, gioRect, points);
                    DrawLines(canvas, points);
                    break;
            }
            return points;
        }

        private void DrawDots(ICanvas canvas, RectF gioRect, PointF[][] points)
        {
            float radius = Math.Min(gioRect.Height, gioRect.Width) * .01f;
            for (int dvIndex = 0; dvIndex < points.Length; dvIndex++)
            {
                canvas.FillColor = _scaleColors.CurrentValue[dvIndex];
                for (int value = 0; value < points[dvIndex].Length; value++) { canvas.FillCircle(points[dvIndex][value], radius); }
            }
        }

        private void DrawLines(ICanvas canvas, PointF[][] points)
        {
            for (int dvIndex = 0; dvIndex < points.Length; dvIndex++)
            {
                canvas.StrokeColor = _scaleColors.CurrentValue[dvIndex];
                canvas.StrokeSize = 3f;
                for (int value = 1; value < points[dvIndex].Length; value++) { canvas.DrawLine(points[dvIndex][value - 1], points[dvIndex][value]); }
            }
        }

        private PointF[][] CalculatePoints(float[] positions, RectF gioRect)
        {
            PointF[][] result = new PointF[Values.Count][];
            for (int dvIndex = 0; dvIndex < Values.Count; dvIndex++)
            {
                DiogrammaValue dv = Values[dvIndex];
                result[dvIndex] = new PointF[dv.Values.Length];
                for (int value = 0; value < dv.Values.Length; value++)
                {
                    result[dvIndex][value] = new(positions[value],
                        float.Lerp(gioRect.Y + gioRect.Height, dv.Values[value]
                        .Remap(MinValue.CurrentValue, MaxValue.CurrentValue, gioRect.Y + gioRect.Height, gioRect.Y), DrawProgress.CurrentValue));
                }
            }
            return result;
        }

        private void FillLegendForLinesDots()
        {
            _legend.Clear();
            if (_scaleColors.CurrentValue.Length != Values.Count) { _scaleColors.Value = Static.GenerateColors(Values.Count); }
            for (int dvIndex = 0; dvIndex < Values.Count; dvIndex++)
            {
                DiogrammaValue dv = Values[dvIndex];
                _legend.Add(new(_scaleColors.CurrentValue[dvIndex], dv.Name));
            }
        }
        #endregion

        #region Support
        private static void DrawValueNamesHor(ICanvas canvas, RectF gioRect, DiogrammaScale<string>[] valueNames, SizeF maxSize, out float[] positions)
        {
            float step = gioRect.Width / (valueNames.Length + 1);
            positions = new float[valueNames.Length];
            for (int i = 0; i < valueNames.Length; i++)
            {
                positions[i] = gioRect.X + step * (i + 1);
                canvas.DrawString(valueNames.ElementAt(i).Value, positions[i], gioRect.Y + gioRect.Height + maxSize.Height,
                    HorizontalAlignment.Center);
            }
        }

        private void AutoValue(ReactiveProperty<float> value, Func<IEnumerable<float>, float> converter)
        {
            if (converter == null || value == null) { return; }
            value.Value = converter.Invoke(Values.SelectMany(x => x.Values));
        }
        private static void DrawScale(ICanvas canvas, RectF gioRect, DiogrammaScale<float> scale, float pos)
        {
            canvas.DrawString(Math.Round(scale.Value, 1, MidpointRounding.ToEven).ToString(), gioRect.X - (scale.TextSize.Width / 2f),
                pos + scale.TextSize.Height / 2f, HorizontalAlignment.Center);
        }
        #endregion

        #region ClickHandler
        private void Check(TouchEventArgs args)
        {
            if (_lastRect == default || args.Touches.Length < 1 || !_lastRect.Contains(args.Touches[0])) { return; }
            RectF[] rects = GetClickRect();
            for (int i = 0; i < rects.Length; i++)
            {
                if (rects[i].Contains(args.Touches[0]))
                {
                    _onSelect.OnNext(i);
                    break;
                }
            }
        }

        private RectF[] GetClickRect()
        {
            DiogrammaScale<string>[] scaleNames =
                Static.GenerateScale(ScaleNames.CurrentValue, Values.Select(x => x.Values.Length).Max(), FontSize.CurrentValue, out SizeF namesMax);
            RectF[] result = new RectF[scaleNames.Length];
            float step;
            step = _lastRect.Width / (scaleNames.Length + 1);
            for (int i = 0; i < scaleNames.Length; i++)
            { result[i] = new(_lastRect.X + step * (i + 1) - namesMax.Width, _lastRect.Y, namesMax.Width * 2, _lastRect.Height); }
            return result;
        }
        #endregion
    }

    #region Structures
    public readonly struct DiogrammaLegend
    {
        public DrawableFigure Figure { get; }
        public string FigureName { get; }

        public DiogrammaLegend(Color color, string name)
        {
            Figure = new Rectangle(color);
            FigureName = name;
        }

        public DiogrammaLegend(DrawableFigure df, string name)
        {
            Figure = df;
            FigureName = name;
        }
    }

    public readonly struct DiogrammaScale<T>(SizeF textSize, T value)
    {
        public SizeF TextSize { get; } = textSize;
        public T Value { get; } = value;
    }

    public readonly struct DiogrammaValue(string name, params float[] value)
    {
        public string Name { get; } = name;
        public float[] Values { get; } = value;
    }

    [Flags]
    public enum DiogrammaAutoValues : byte
    {
        None = 0,
        Max = 1 << 0,
        Min = 1 << 1,
        All = Min | Max
    }

    public enum DiogrammaDrawMode : byte
    {
        Dots = 0,
        Lines = 1,
        DotsLines = 2
    }
    #endregion

    #region Figures
    public abstract class DrawableFigure(Color color) : IDrawable
    {
        public Color Color { get; private set; } = color;
        public abstract void Draw(ICanvas canvas, RectF dirtyRect);
    }

    public class Rectangle(Color color) : DrawableFigure(color)
    {
        public override void Draw(ICanvas canvas, RectF dirtyRect)
        {
            canvas.FillColor = Color;
            canvas.FillRectangle(dirtyRect);
        }
    }
    #endregion
}
