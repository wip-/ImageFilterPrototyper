using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFilterPrototyper.Filters
{
    public class WarmingFilter85 : ParameterizedFilter
    {

        private enum ChartRow
        {
            HueRange,

            RedCoeff3,
            RedCoeff2,
            RedCoeff1,
            RedCoeff0,

            GreenCoeff3,
            GreenCoeff2,
            GreenCoeff1,
            GreenCoeff0,

            BlueCoeff3,
            BlueCoeff2,
            BlueCoeff1,
            BlueCoeff0,
        }


        // Color Component Curves for different values of Hue
        private double[,] WarmingFilter85Curves = new double[,]
        {
            { 0       , 25        , 60          , 180          , 222        , 300           , 360 },   // hue
                                                               
            { 0.000023, 0.000023  , -000.0000725,   0.0000867  ,   0.0000867, -0000.00002279, 0   },   // red 3
            { 0.0046  , 0.0046    ,  000.04775  , - 0.042874   , -00.042874 ,     0.03049165, 0   },   // red 2
            { 0.017   , 0.017     , -008.584    ,   5.85       ,   5.85     , -0013.106     , 0   },   // red 1
            { 190     , 190       ,  576.4      , -15          , -15        ,  2019.8       , 0   },   // red 0
                                             
            { 0       ,   0.002816,    0.0000106, -0000.0010831, 0          , 0             , 0   },   // green 3
            { 0       , - 0.40435 , -000.00588  ,     0.628986 , 0          , 0             , 0   },   // green 2
            { 0       ,  22.12    ,    0.96     , -0124.5      , 0          , 0             , 0   },   // green 1
            { 0       , -336      ,  105.7      ,  8498        , 0          , 0             , 0   },   // green 0

            { 0       , 0         , 0           , 0            , 0          , 0             , 0   },   // blue 3
            { 0       , 0         , 0           , 0            , 0          , 0             , 0   },   // blue 2
            { 0       , 0         , 0           , 0            , 0          , 0             , 0   },   // blue 1
            { 0       , 0         , 0           , 0            , 0          , 0             , 0   },   // blue 0
        };
        

        protected override void FillPixelData(BitmapInfo bitmapInfoSource, BitmapInfo bitmapInfoDest)
        {
            for (int y = 0; y < bitmapInfoSource.Height; y++)
            {
                for (int x = 0; x < bitmapInfoSource.Width; x++)
                {
                    Color sourceColor = bitmapInfoSource.GetPixelColor(x, y);

                    float hue = sourceColor.GetHue();
                    int hueRangerIndex = 0;
                    while (WarmingFilter85Curves[(int)ChartRow.HueRange, hueRangerIndex + 1] <= hue)
                        ++hueRangerIndex;

                    double r =
                          WarmingFilter85Curves[(int)ChartRow.RedCoeff3, hueRangerIndex] * hue * hue * hue
                        + WarmingFilter85Curves[(int)ChartRow.RedCoeff2, hueRangerIndex] * hue * hue
                        + WarmingFilter85Curves[(int)ChartRow.RedCoeff1, hueRangerIndex] * hue
                        + WarmingFilter85Curves[(int)ChartRow.RedCoeff0, hueRangerIndex];

                    double g =
                          WarmingFilter85Curves[(int)ChartRow.GreenCoeff3, hueRangerIndex] * hue * hue * hue
                        + WarmingFilter85Curves[(int)ChartRow.GreenCoeff2, hueRangerIndex] * hue * hue
                        + WarmingFilter85Curves[(int)ChartRow.GreenCoeff1, hueRangerIndex] * hue
                        + WarmingFilter85Curves[(int)ChartRow.GreenCoeff0, hueRangerIndex];

                    double b =
                          WarmingFilter85Curves[(int)ChartRow.BlueCoeff3, hueRangerIndex] * hue * hue * hue
                        + WarmingFilter85Curves[(int)ChartRow.BlueCoeff2, hueRangerIndex] * hue * hue
                        + WarmingFilter85Curves[(int)ChartRow.BlueCoeff1, hueRangerIndex] * hue
                        + WarmingFilter85Curves[(int)ChartRow.BlueCoeff0, hueRangerIndex];
                    
                    r = Parameter * r + (1 - Parameter) * sourceColor.R;
                    g = Parameter * g + (1 - Parameter) * sourceColor.G;
                    b = Parameter * b + (1 - Parameter) * sourceColor.B;

                    Color filteredColor = Color.FromArgb(
                        sourceColor.A,
                        Convert.ToByte(r.Clamp(0, 255)),
                        Convert.ToByte(g.Clamp(0, 255)),
                        Convert.ToByte(b.Clamp(0, 255)));

                    bitmapInfoDest.SetPixelColor(x, y, filteredColor);
                }
            }
        }

    }
}
