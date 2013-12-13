using System;
using System.Drawing;

namespace ImageFilterPrototyper.Filters
{
    public class IdentityFilter : BaseFilter
    {
        protected override void FillPixelData(BitmapInfo bitmapInfoSource, BitmapInfo bitmapInfoDest)
        {
            for (int y = 0; y < bitmapInfoSource.Height; y++)
            {
                for (int x = 0; x < bitmapInfoSource.Width; x++)
                {
                    Color sourceColor = bitmapInfoSource.GetPixelColor(x, y);

                    Color filteredColor = Color.FromArgb(
                        sourceColor.A,
                        sourceColor.R,
                        sourceColor.G,
                        sourceColor.B);
                    bitmapInfoDest.SetPixelColor(x, y, filteredColor);
                }
            }
        }
    }
}
