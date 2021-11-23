using System;

namespace spectrogram
{
    public class ColorUtils
    {
        /**
         * <summary>Convert from hsv color space to RGB</summary>
         * <param name="h">Specify Hue in the range of 0.0 to 1.0</param>
         * <param name="s">Specify Saturation in the range of 0.0 to 1.0</param>
         * <param name="v">Specify Value in the range of 0.0 to 1.0</param>
         * <param name="buf">Buffer to write each converted RGB value</param>
         * <param name="offset">Position to write each converted RGB value</param>
         * <remarks>
         * By multiplying the value of h by 6 and discarding the fractions,
         * it is possible to find out which of the 6-divided circular HSV color spaces it applies to.
         * </remarks>
        */
        public static void Hsv2Rgb(double h, double s, double v, byte[] buf, int offset)
        {
            double r = v;
            double g = v;
            double b = v;
            if (s > 0.0)
            {
                h = h * 6;
                int i = (int)h;
                double f = h - (double)i;
                switch (i)
                {
                    default:
                    case 0:
                        g *= 1 - s * (1 - f);
                        b *= 1 - s;
                        break;
                    case 1:
                        r *= 1 - s * f;
                        b *= 1 - s;
                        break;
                    case 2:
                        r *= 1 - s;
                        b *= 1 - s * (1 - f);
                        break;
                    case 3:
                        r *= 1 - s;
                        g *= 1 - s * f;
                        break;
                    case 4:
                        r *= 1 - s * (1 - f);
                        g *= 1 - s;
                        break;
                    case 5:
                        g *= 1 - s;
                        b *= 1 - s * f;
                        break;
                }
            }
            buf[offset] = (byte)(Byte.MaxValue * r);
            buf[offset + 1] = (byte)(Byte.MaxValue * g);
            buf[offset + 2] = (byte)(Byte.MaxValue * b);
        }

        /**
         * <summary>Convert from dB to pixel value.</summary>
         * <param name="v">dB value</param>
         * <param name="gain">Gain for dB value</param>
         * <param name="range">Effective range of dB value</param>
         * <param name="hmin">The minimum value of Hue in the HSV color space that corresponds to the minimum value of v</param>
         * <param name="hman">Maximum value of Hue in HSV color space corresponding to the minimum value of v</param>
         * <param name="bmpdata">Buffer to store the conversion result</param>
         * <param name="bmpoff">The position of the buffer that stores the conversion result</param>
         * <remarks>
         * All values above "gain" are converted to hmax, and the range from "gain" to values below "range" is 
         * projected into the color space in the range hmax to hmin.
         * </remarks>
        */
        public static void dbToColor(double v, int gain, int range, double hmin, double hmax, byte[] bmpdata, int bmpoff)
        {
            double h = -(v - gain) / range;
            if (h < 0)
            {
                h = 0;
            }
            else if (h > 1)
            {
                h = 1;
            }
            h = h * (hmax - hmin) + hmin;
            Hsv2Rgb(h, 1.0, 1.0, bmpdata, bmpoff);
        }
    }
}