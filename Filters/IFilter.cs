using System;

namespace ImageFilterPrototyper.Filters
{
    public interface IFilter
    {
        BitmapInfo GetFilteredImage(BitmapInfo bitmapInfoSource);
    }
}
