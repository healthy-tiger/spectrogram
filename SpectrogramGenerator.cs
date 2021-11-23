using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using csfft;

namespace spectrogram
{
    public class SpectrogramGenerator
    {
        /**
         * <summary>number of fft points</summary>
         */
        public int FftSize { get { return trans.NumOfPoints; } }

        private FFT trans;

        private double[] _real;

        private double[] _imag;

        private List<double[]> _powers;

        /**
         * <summary>The highest level represented in the spectrogram</summary>
         * <remarks>All levels above "gain" are represented by the hue specified by "HueMax".</remarks>
         */
        public int Gain { get; set; }

        /**
         * <summary>Range below "gain" expressed in spectrogram</summary>
         * <remarks>The range from "gain" to "gain"-"range" is expressed by the hue from "HueMax" to "HueMin".</remarks>
         */
        public int Range { get; set; }

        /**
         * <summary>Hue corresponding to the highest level</summary>
         */
        public double HueMax { get; set; }

        /**
         * <summary>Hue corresponding to the lowest level</summary>
         */
        public double HueMin { get; set; }

        public SpectrogramGenerator(int fftsize, WindowFunction windowFunction = null, int Gain = -30, int Range = 80, double HueMax = 0, double HueMin = (double)2 / 3)
        {
            if (windowFunction == null)
            {
                windowFunction = FFT.HannWindow;
            }
            this.trans = new FFT(fftsize, windowFunction, FFT.DivByN);
            this.Gain = Gain;
            this.Range = Range;
            this.HueMax = HueMax;
            this.HueMin = HueMin;
            Reset();
        }

        public void Reset()
        {
            this._real = new double[FftSize];
            this._imag = new double[FftSize];
            this._powers = new List<double[]>();
        }

        public void PutNextFrame(double[] re, int reoffset, double[] im, int imoffset, int length)
        {
            int d = FftSize - length;
            if (d > 0)
            {
                Array.Copy(_real, length, _real, 0, d);
                Array.Copy(_imag, length, _imag, 0, d);
                Array.Copy(re, reoffset, _real, d, length);
                Array.Copy(im, imoffset, _imag, d, length);
            } else {
                Array.Copy(re, reoffset, _real, 0, FftSize);
                Array.Copy(im, imoffset, _imag, 0, FftSize);
            }

            _powers.Add(trans.FwdPower(_real, _imag, db: true));
        }

        public void PutAll(double[] re, double[] im, int length, int slide, int reoffset = 0, int imoffset = 0) {
            if(slide <= 0) {
                throw new ArgumentException("Slide widths less than 0 cannot be accepted.");
            }
            for(int off = 0; off < length; off+=slide) {
                int len = Math.Min(length - off, slide);
                this.PutNextFrame(re, off + reoffset, im, off + imoffset, len);
            }
        }

        public void PutAll(double[] src, int length, int slide, int baseoffset = 0) {
            double[] im = new double[this.FftSize];
            for(int off = 0; off < src.Length; off+=slide) {
                int len = Math.Min(src.Length - off, slide);
                this.PutNextFrame(src, off + baseoffset, im, 0, len);
            }
        }

        private const int PixelWidth = 3;

        public Bitmap GetBitmap(bool clearfft = true)
        {
            double[][] power = _powers.ToArray();
            int width = power.Length;
            int height = power[0].Length / 2;
            double hmin = HueMin;
            double hmax = HueMax;
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            BitmapData bmpdata = bmp.LockBits(new Rectangle(0, 0, width, height),
                          ImageLockMode.WriteOnly,
                          PixelFormat.Format24bppRgb);
            byte[] bytes = new byte[bmpdata.Stride * height];
            for (int col = 0; col < width; col++)
            {
                for (int row = 0; row < height; row++)
                {
                    int off = col * PixelWidth + (height - row - 1) * bmpdata.Stride;
                    double m = power[col][row]; // Since the input is normalized to the range -1.0 to 1.0, the dB of the FFT result will always be 0 or minus.
                    ColorUtils.dbToColor(m, Gain, Range, hmin, hmax, bytes, off);
                }
            }
            Marshal.Copy(bytes, 0, bmpdata.Scan0, bytes.Length);
            bmp.UnlockBits(bmpdata);

            if (clearfft)
            {
                _powers.Clear();
            }

            return bmp;
        }
    }
}