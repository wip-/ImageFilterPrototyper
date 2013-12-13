using System;
using ImageFilterPrototyper;
using ImageFilterPrototyper.Filters;

namespace ImageFilterPrototyper.Filters
{
    public abstract class BaseFilter : IFilter
    {
        public BitmapInfo GetFilteredImage(BitmapInfo bitmapInfoSource)
        {
            BitmapInfo result = new BitmapInfo(
                bitmapInfoSource.Width, bitmapInfoSource.Height, bitmapInfoSource.PixelFormat);
            FillPixelData(bitmapInfoSource, result);
            return result;
        }

        protected abstract void FillPixelData(BitmapInfo bitmapInfoSource, BitmapInfo bitmapInfoDest);
    }
}
