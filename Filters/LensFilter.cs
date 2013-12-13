using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ImageFilterPrototyper.Filters
{
    public class LensFilter : BaseFilter
    {
        
        /// <summary>
        /// Transmittance curves according to wavelength
        /// based on values available at http://www.karmalimbo.com/aro/pics/filters/transmision%20of%20wratten%20filters.pdf
        /// </summary>
        private SortedDictionary<double, double> WarmingFilter85Transmittance;

        public LensFilter()
        {
            WarmingFilter85Transmittance = new SortedDictionary<double, double>();
            WarmingFilter85Transmittance[400] = 0.060;
            WarmingFilter85Transmittance[410] = 0.180;
            WarmingFilter85Transmittance[420] = 0.284;
            WarmingFilter85Transmittance[430] = 0.334;
            WarmingFilter85Transmittance[440] = 0.362;
            WarmingFilter85Transmittance[450] = 0.381;
            WarmingFilter85Transmittance[460] = 0.404;
            WarmingFilter85Transmittance[470] = 0.430;
            WarmingFilter85Transmittance[480] = 0.453;
            WarmingFilter85Transmittance[490] = 0.472;
            WarmingFilter85Transmittance[500] = 0.489;
            WarmingFilter85Transmittance[510] = 0.492;
            WarmingFilter85Transmittance[520] = 0.482;
            WarmingFilter85Transmittance[530] = 0.483;
            WarmingFilter85Transmittance[540] = 0.492;
            WarmingFilter85Transmittance[550] = 0.510;
            WarmingFilter85Transmittance[560] = 0.558;
            WarmingFilter85Transmittance[570] = 0.645;
            WarmingFilter85Transmittance[580] = 0.750;
            WarmingFilter85Transmittance[590] = 0.830;
            WarmingFilter85Transmittance[600] = 0.872;
            WarmingFilter85Transmittance[610] = 0.889;
            WarmingFilter85Transmittance[620] = 0.900;
            WarmingFilter85Transmittance[630] = 0.905;
            WarmingFilter85Transmittance[640] = 0.907;
            WarmingFilter85Transmittance[650] = 0.909;
            WarmingFilter85Transmittance[660] = 0.910;
            WarmingFilter85Transmittance[670] = 0.910;
            WarmingFilter85Transmittance[680] = 0.910;
            WarmingFilter85Transmittance[690] = 0.910;
            WarmingFilter85Transmittance[700] = 0.910;
        }


        protected override void FillPixelData(BitmapInfo bitmapInfoSource, BitmapInfo bitmapInfoDest)
        {
            for (int y = 0; y < bitmapInfoSource.Height; y++)
            {
                for (int x = 0; x < bitmapInfoSource.Width; x++)
                {
                    Color sourceColor = bitmapInfoSource.GetPixelColor(x, y);

                    //float hue = sourceColor.GetHue();
                    //float saturation = sourceColor.GetSaturation();
                    //float brightness = sourceColor.GetBrightness();

                    //double hue, saturation, value;
                    //ColorToHSV(sourceColor, out hue, out saturation, out value);

                    double hue, saturation, lightness;
                    RGBtoHSL(sourceColor, out hue, out saturation, out lightness);

                    double transmittance = GetTransmittance(hue);

                    double newLightness = transmittance * lightness;    // we do not preserve lightness

                    Color filteredColor = HSLtoRGB(hue, saturation, newLightness);
                    bitmapInfoDest.SetPixelColor(x, y, filteredColor);
                }
            }
        }


        private double GetTransmittance(double hue)
        {
            // We compute an approximate value of the wavelength according to color Hue.
            // It is a linear approximation (wrong because visible spectrum hue presents 
            // some discontinuities around green wavelengths)
            // TODO: a better approximation

            double clampedHue = hue.Clamp(0, 280);  // hue above 280 (magenta) cannot be linked to a single wavelength
            double waveLength = 640 - 6 * clampedHue / 7;

            SortedDictionary<double, double> dict = WarmingFilter85Transmittance;
            if(dict.ContainsKey(waveLength))
                return dict[waveLength];

            // find closest keys
            double closestSmallerKey = dict.Keys.ElementAt(0);
            double closestBiggerKey = dict.Keys.ElementAt(1);
            for (int i = 1; i < dict.Keys.Count; ++i )
            {
                double smallKey = dict.Keys.ElementAt(i-1);
                double bigKey = dict.Keys.ElementAt(i);
                if (bigKey >= waveLength && 
                    smallKey <= waveLength )
                {
                    closestSmallerKey = smallKey;
                    closestBiggerKey = bigKey;
                    break;
                }
            }

            double smallKeyTransmit = dict[closestSmallerKey];
            double bigKeyTransmit = dict[closestBiggerKey];
            
            double ratio = (waveLength - closestSmallerKey) / (closestBiggerKey - closestSmallerKey);
            double value = smallKeyTransmit + ratio * (bigKeyTransmit - smallKeyTransmit);

            return value;
        }






        // conversion functions from http://www.codeproject.com/Articles/19045/Manipulating-colors-in-NET-Part-1
        /// <summary>
        /// Converts HSL to RGB.
        /// </summary>
        /// <param name="h">Hue, must be in [0, 360].</param>
        /// <param name="s">Saturation, must be in [0, 1].</param>
        /// <param name="l">Luminance, must be in [0, 1].</param>
        public static Color HSLtoRGB(double h, double s, double l)
        {
            if (s == 0)
            {
                // achromatic color (gray scale)
                return Color.FromArgb(255,
                    Convert.ToByte(l * 255.0),
                    Convert.ToByte(l * 255.0),
                    Convert.ToByte(l * 255.0));
            }
            else
            {
                double q = (l < 0.5) ? (l * (1.0 + s)) : (l + s - (l * s));
                double p = (2.0 * l) - q;

                double Hk = h / 360.0;
                double[] T = new double[3];
                T[0] = Hk + (1.0 / 3.0);	// Tr
                T[1] = Hk;				// Tb
                T[2] = Hk - (1.0 / 3.0);	// Tg

                for (int i = 0; i < 3; i++)
                {
                    if (T[i] < 0) T[i] += 1.0;
                    if (T[i] > 1) T[i] -= 1.0;

                    if ((T[i] * 6) < 1)
                    {
                        T[i] = p + ((q - p) * 6.0 * T[i]);
                    }
                    else if ((T[i] * 2.0) < 1) //(1.0/6.0)<=T[i] && T[i]<0.5
                    {
                        T[i] = q;
                    }
                    else if ((T[i] * 3.0) < 2) // 0.5<=T[i] && T[i]<(2.0/3.0)
                    {
                        T[i] = p + (q - p) * ((2.0 / 3.0) - T[i]) * 6.0;
                    }
                    else T[i] = p;
                }

                return Color.FromArgb(255,
                    Convert.ToByte(T[0] * 255.0),
                    Convert.ToByte(T[1] * 255.0),
                    Convert.ToByte(T[2] * 255.0));
            }
        }

        /// <summary> 
        /// Converts RGB to HSL.
        /// </summary>
        public static void RGBtoHSL(Color rgb, out double h, out double s, out double l)
        {
            // normalizes red-green-blue values
            double nRed = (double)rgb.R / 255.0;
            double nGreen = (double)rgb.G / 255.0;
            double nBlue = (double)rgb.B / 255.0;

            double max = Math.Max(nRed, Math.Max(nGreen, nBlue));
            double min = Math.Min(nRed, Math.Min(nGreen, nBlue));

            // hue
            h = 0; // undefined
            if (max == min)
            {
                h = 0; // undefined (grayscale)
            }
            else if (max == nRed && nGreen >= nBlue)
            {
                h = 60.0 * (nGreen - nBlue) / (max - min);
            }
            else if (max == nRed && nGreen < nBlue)
            {
                h = 60.0 * (nGreen - nBlue) / (max - min) + 360.0;
            }
            else if (max == nGreen)
            {
                h = 60.0 * (nBlue - nRed) / (max - min) + 120.0;
            }
            else if (max == nBlue)
            {
                h = 60.0 * (nRed - nGreen) / (max - min) + 240.0;
            }

            // luminance
            l = (max + min) / 2.0;

            // saturation
            s = 0;
            if (l == 0 || max == min)
            {
                s = 0;
            }
            else if (0 < l && l <= 0.5)
            {
                s = (max - min) / (max + min);
            }
            else if (l > 0.5)
            {
                s = (max - min) / (2 - (max + min)); //(max-min > 0)?
            }
        } 









        ////http://stackoverflow.com/a/1626175/758666
        //private static void ColorToHSV(Color color, out double hue, out double saturation, out double value)
        //{
        //    int max = Math.Max(color.R, Math.Max(color.G, color.B));
        //    int min = Math.Min(color.R, Math.Min(color.G, color.B));

        //    hue = color.GetHue();
        //    saturation = (max == 0) ? 0 : 1d - (1d * min / max);
        //    value = max / 255d;
        //}


        ////http://stackoverflow.com/a/1626175/758666
        //private static Color ColorFromHSV(double hue, double saturation, double value)
        //{
        //    int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        //    double f = hue / 60 - Math.Floor(hue / 60);

        //    value = value * 255;
        //    int v = Convert.ToInt32(value);
        //    int p = Convert.ToInt32(value * (1 - saturation));
        //    int q = Convert.ToInt32(value * (1 - f * saturation));
        //    int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

        //    if (hi == 0)
        //        return Color.FromArgb(255, v, t, p);
        //    else if (hi == 1)
        //        return Color.FromArgb(255, q, v, p);
        //    else if (hi == 2)
        //        return Color.FromArgb(255, p, v, t);
        //    else if (hi == 3)
        //        return Color.FromArgb(255, p, q, v);
        //    else if (hi == 4)
        //        return Color.FromArgb(255, t, p, v);
        //    else
        //        return Color.FromArgb(255, v, p, q);
        //}














    }
}
