using Course31_trpo.Drawables;
using Microsoft.Maui.Graphics.Skia;
using Font = Microsoft.Maui.Graphics.Font;

namespace Course31_trpo
{
    public static class Static
    {
        public static Color[] GenerateColors(int count)
        {
            Color[] result = new Color[count];
            Random random = new();
            for (int i = 0; i < count; i++) { result[i] = new Color(random.Next(128, 256), random.Next(128, 256), random.Next(128, 256)); }
            return result;
        }

        public static Color Lerp(Color a, Color b, float amount) => new(
            float.Lerp(a?.Red ?? 0, b?.Red ?? 0, amount),
            float.Lerp(a?.Green ?? 0, b?.Green ?? 0, amount),
            float.Lerp(a?.Blue ?? 0, b?.Blue ?? 0, amount), float.Lerp(a?.Alpha ?? 0, b?.Alpha ?? 0, amount)
        );

        public static DiogrammaScale<float>[] GenerateScale(uint valuesCount, float min, float max, float fontSize, out SizeF maxSize, float minSize = 16f)
        {
            DiogrammaScale<float>[] scale = new DiogrammaScale<float>[valuesCount + 2];
            float valueStep = (Math.Max(min, max) - Math.Min(min, max)) / (valuesCount + 1);
            float curValue = Math.Min(min, max);
            maxSize = new();
            for (int i = 0; i < scale.Length; i++)
            {
                using BitmapExportContext bmp = new SkiaBitmapExportContext(450, 150, 1f);
                scale[i] = new(bmp.Canvas.GetStringSize(Math.Round(curValue, 1, MidpointRounding.ToEven).ToString(), Font.Default, fontSize), curValue);
                if (maxSize.Width < scale[i].TextSize.Width) { maxSize.Width = scale[i].TextSize.Width; }
                if (maxSize.Height < scale[i].TextSize.Height) { maxSize.Height = scale[i].TextSize.Height; }
                curValue += valueStep;
            }
            maxSize.Width = Math.Max(maxSize.Width, minSize);
            maxSize.Height = Math.Max(maxSize.Height, minSize);
            return scale;
        }
        public static DiogrammaScale<string>[] GenerateScale(IEnumerable<string> values, int itemsCount, float fontSize, out SizeF maxSize, float minSize = 16f)
        {
            int resultCount = values == null ? itemsCount : Math.Max(values.Count(), itemsCount);
            int valuesCount = values?.Count() ?? 0;
            DiogrammaScale<string>[] scale = new DiogrammaScale<string>[resultCount];
            maxSize = new();
            for (int i = 0; i < scale.Length; i++)
            {
                string curValue;
                if (i < valuesCount && values != null) { curValue = values?.ElementAt(i) ?? i.ToString(); }
                else { curValue = i.ToString(); }
                using BitmapExportContext bmp = new SkiaBitmapExportContext(450, 150, 1f);
                scale[i] = new(bmp.Canvas.GetStringSize(curValue, Font.Default, fontSize), curValue);
                if (maxSize.Width < scale[i].TextSize.Width) { maxSize.Width = scale[i].TextSize.Width; }
                if (maxSize.Height < scale[i].TextSize.Height) { maxSize.Height = scale[i].TextSize.Height; }
            }
            maxSize.Width = Math.Max(maxSize.Width, minSize);
            maxSize.Height = Math.Max(maxSize.Height, minSize);
            return scale;
        }
    }

    public static class BloodMath
    {
        public static double GetAngle(this PointF one, PointF two)
        {
            float dX = two.X - one.X;
            float dY = two.Y - one.Y;

            return Math.Atan2(dY, dX) * (180f / Math.PI);
        }

        /// <summary>
        /// Remap value from source to target scales
        /// </summary>
        /// <param name="value">Value in source scale</param>
        /// <param name="min1">Min of source scale</param>
        /// <param name="max1">Max of source scale</param>
        /// <param name="min2">Min of target scale</param>
        /// <param name="max2">Max of target scale</param>
        /// <returns>Remapped value to target scale</returns>
        public static double Remap(this double value, double min1, double max1, double min2, double max2) => min2 + (value - min1) * (max2 - min2) / (max1 - min1);
        public static float Remap(this float value, float min1, float max1, float min2, float max2) => min2 + (value - min1) * (max2 - min2) / (max1 - min1);
    }
}
