﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using log4net;

namespace TwainDotNet.Win32
{
    public static class BitmapRenderer
    {
        static ILog log = LogManager.GetLogger(typeof(BitmapRenderer));

        private static float PpmToDpi(double pixelsPerMeter)
        {
            double pixelsPerMillimeter = (double)pixelsPerMeter / 1000.0;
            double dotsPerInch = pixelsPerMillimeter * 25.4;
            return (float)Math.Round(dotsPerInch, 2);
        }

        
        public static Bitmap NewBitmapFromHBitmap(IntPtr dibHandle) {

            IntPtr _bitmapPointer;
            IntPtr _pixelInfoPointer;
            Rectangle _rectangle;
            BitmapInfoHeader _bitmapInfo;
            Bitmap bitmap;

            _bitmapPointer = Kernel32Native.GlobalLock(dibHandle);
            try {
                _bitmapInfo = new BitmapInfoHeader();
                Marshal.PtrToStructure(_bitmapPointer, _bitmapInfo);
                log.Debug(_bitmapInfo.ToString());

                _rectangle = new Rectangle();
                _rectangle.X = _rectangle.Y = 0;
                _rectangle.Width = _bitmapInfo.Width;
                _rectangle.Height = _bitmapInfo.Height;

                if (_bitmapInfo.SizeImage == 0) {
                    _bitmapInfo.SizeImage = ((((_bitmapInfo.Width * _bitmapInfo.BitCount) + 31) & ~31) >> 3) * _bitmapInfo.Height;
                }


                // compute the offset to the pixel info, which follows the bitmap info header
                { 
                    // The following code only works on x86
                    Debug.Assert(Marshal.SizeOf(typeof(IntPtr)) == 4);                
                    int pixelInfoPointer = _bitmapInfo.ClrUsed;
                    if ((pixelInfoPointer == 0) && (_bitmapInfo.BitCount <= 8)) {
                        pixelInfoPointer = 1 << _bitmapInfo.BitCount;
                    }
                    pixelInfoPointer = (pixelInfoPointer * 4) + _bitmapInfo.Size + _bitmapPointer.ToInt32();
                    _pixelInfoPointer = new IntPtr(pixelInfoPointer);
                }

                // render to bitmap
                bitmap = new Bitmap(_rectangle.Width, _rectangle.Height);

                using (Graphics graphics = Graphics.FromImage(bitmap)) {
                    IntPtr hdc = graphics.GetHdc();

                    try {
                        Gdi32Native.SetDIBitsToDevice(hdc, 0, 0, _rectangle.Width, _rectangle.Height,
                            0, 0, 0, _rectangle.Height, _pixelInfoPointer, _bitmapPointer, 0);
                    }
                    finally {
                        graphics.ReleaseHdc(hdc);
                    }
                }

                bitmap.SetResolution(PpmToDpi(_bitmapInfo.XPelsPerMeter), PpmToDpi(_bitmapInfo.YPelsPerMeter));
            } finally {
                Kernel32Native.GlobalUnlock(dibHandle);
            }
            return bitmap;
        }

        public static Bitmap NewBitmapForImageInfo(TwainDotNet.TwainNative.ImageInfo imageInfo) 
        {
            var bitmap = new Bitmap(imageInfo.ImageWidth,imageInfo.ImageLength,System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (Graphics graphics = Graphics.FromImage(bitmap)) {
                graphics.Clear(Color.White);
            }
            return bitmap;
        }

        public static unsafe void TransferPixels(Bitmap bitmap_dest, 
            TwainDotNet.TwainNative.ImageInfo imageInfo, TwainDotNet.TwainNative.ImageMemXfer memxfer_src) {
            BitmapInfoHeaderStruct bitmapInfo = new BitmapInfoHeaderStruct();
            bitmapInfo.Width = (int) memxfer_src.Columns;
            bitmapInfo.Height = - (int) memxfer_src.Rows;
            bitmapInfo.Size = sizeof(BitmapInfoHeaderStruct);            
            bitmapInfo.Planes = 1;
            bitmapInfo.SizeImage = 0;
            bitmapInfo.BitCount = imageInfo.BitsPerPixel;   // this might not work in all cases            

            using (Graphics graphics = Graphics.FromImage(bitmap_dest)) {
                

                IntPtr hdc = graphics.GetHdc();
                try {
                    Gdi32Native.SetDIBitsToDevice(
                        hdc, 
                        (int)memxfer_src.XOffset, 
                        (int)memxfer_src.YOffset, 
                        (int)memxfer_src.Columns, 
                        (int)memxfer_src.Rows,
                        0, 0, 0, 
                        (int)memxfer_src.Rows,
                        memxfer_src.Memory.TheMem, 
                        new IntPtr(&bitmapInfo), 
                        0);
                }
                finally {
                    graphics.ReleaseHdc(hdc);
                }
            }

        }
    }
}
