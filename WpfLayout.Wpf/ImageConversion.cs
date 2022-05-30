namespace WpfLayout;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public static class ImageConversion
{
    static ImageConversion()
    {
        using Bitmap Bitmap = new Bitmap(1, 1);
        DefaultImage = IconBitmapToImageSource(Bitmap);
    }

    public static ImageSource DefaultImage { get; }

    private static Bitmap FileToBitmap(string fileName)
    {
        if (fileName == null || !File.Exists(fileName))
            return new Bitmap(1, 1);

        return new Bitmap(fileName);
    }

    public static Icon IconFileToIcon(string iconFile)
    {
        using Bitmap Bitmap = FileToBitmap(iconFile);
        IntPtr Handle = Bitmap.GetHicon();
        Icon TemporaryIcon = Icon.FromHandle(Handle);
        Icon Result = (Icon)TemporaryIcon.Clone();
        TemporaryIcon.Dispose();

        return Result;
    }

    public static ImageSource IconFileToImageSource(string iconFile)
    {
        using Bitmap Bitmap = FileToBitmap(iconFile);
        return IconBitmapToImageSource(Bitmap);
    }

    public static ImageSource IconStreamToImageSource(Stream stream)
    {
        using Bitmap Bitmap = new Bitmap(stream);
        return IconBitmapToImageSource(Bitmap);
    }

    private static ImageSource IconBitmapToImageSource(Bitmap bitmap)
    {
        IntPtr Handle = bitmap.GetHbitmap();
        try
        {
            return Imaging.CreateBitmapSourceFromHBitmap(Handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }
        finally
        {
            NativeMethods.DeleteObject(Handle);
        }
    }

    public static ImageSource EmptyBitmapToImageSource(string iconFile, int x, int y, int width, int height)
    {
        using Bitmap Bitmap = FileToBitmap(iconFile);
        return EmptyBitmapToImageSource(Bitmap, x, y, width, height);
    }

    private static ImageSource EmptyBitmapToImageSource(Bitmap bitmap, int x, int y, int width, int height)
    {
        Rectangle Rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        BitmapData Data = bitmap.LockBits(Rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
        IntPtr DataBytes = Data.Scan0;

        int ByteCount = Data.Stride * bitmap.Height;
        byte[] BitmapBytes = new byte[ByteCount];

        Marshal.Copy(DataBytes, BitmapBytes, 0, ByteCount);

        if (width < 0)
            width = bitmap.Width - (x * 2);
        if (height < 0)
            height = bitmap.Height - (y * 2);

        int ColorByteCount = Data.Stride / Data.Width;
        for (int i = y; i < y + height && i < bitmap.Height; i++)
            for (int j = x; j < x + width && j < bitmap.Width; j++)
                for (int k = 0; k < ColorByteCount; k++)
                {
                    float Start = BitmapBytes[(i * Data.Stride) + (x * ColorByteCount) + k];
                    float End = BitmapBytes[(i * Data.Stride) + ((x + width) * ColorByteCount) + k];
                    byte Color = (byte)((((End - Start) * j) / width) + Start);

                    BitmapBytes[(i * Data.Stride) + (j * ColorByteCount) + k] = Color;
                }

        Marshal.Copy(BitmapBytes, 0, DataBytes, ByteCount);

        bitmap.UnlockBits(Data);

        IntPtr Handle = bitmap.GetHbitmap();
        try
        {
            ImageSource Source = Imaging.CreateBitmapSourceFromHBitmap(Handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return Source;
        }
        finally
        {
            NativeMethods.DeleteObject(Handle);
        }
    }

    public static void SaveImageToClipboard(List<FrameworkElement> elementList)
    {
        BitmapSource Image = RenderToBitmap(elementList, 1.0, System.Windows.Media.Brushes.Black);
        Clipboard.SetImage(Image);
    }

    private static BitmapSource RenderToBitmap(List<FrameworkElement> elementList, double scale, System.Windows.Media.Brush background)
    {
        int LengthSquared = (int)Math.Sqrt(elementList.Count);
        int ColumnMax = elementList.Count <= 4 ? elementList.Count : elementList.Count / LengthSquared;
        int RowMax = (elementList.Count + ColumnMax - 1) / ColumnMax;

        double[,] ArrayWidth = new double[RowMax, ColumnMax];
        double[,] ArrayHeight = new double[RowMax, ColumnMax];
        double[] ArrayRowHeight = new double[RowMax];
        int i, j;

        i = 0;
        j = 0;
        for (int Index = 0; Index < elementList.Count; Index++)
        {
            UIElement Item = elementList[Index];
            ArrayWidth[i, j] = Item.RenderSize.Width;
            ArrayHeight[i, j] = Item.RenderSize.Height;

            j++;
            if (j >= ColumnMax)
            {
                i++;
                j = 0;
            }
        }

        double TotalWidth = 0;
        double TotalHeight = 0;

        for (i = 0; i < RowMax; i++)
        {
            double RowWidth = 0;
            double RowHeight = 0;

            for (j = 0; j < ColumnMax; j++)
            {
                RowWidth += ArrayWidth[i, j];

                if (RowHeight < ArrayHeight[i, j])
                    RowHeight = ArrayHeight[i, j];
            }

            if (TotalWidth < RowWidth)
                TotalWidth = RowWidth;

            ArrayRowHeight[i] = RowHeight;
            TotalHeight += RowHeight;
        }

        RenderTargetBitmap RenderTarget = new RenderTargetBitmap((int)(TotalWidth * scale), (int)(TotalHeight * scale), 96, 96, PixelFormats.Default);
        DrawingVisual DrawingVisual = new DrawingVisual();
        DrawingContext DrawingContext = DrawingVisual.RenderOpen();

        using (DrawingContext)
        {
            DrawingContext.PushTransform(new ScaleTransform(scale, scale));
            Rect FullRect = new Rect(0, 0, TotalWidth, TotalHeight);
            DrawingContext.DrawRectangle(background, null, FullRect);

            i = 0;
            j = 0;
            double OffsetX = 0;
            double OffsetY = 0;

            for (int Index = 0; Index < elementList.Count; Index++)
            {
                UIElement Item = (UIElement)elementList[Index];
                VisualBrush SourceBrush = new VisualBrush(Item);
                Rect Rect = new Rect(OffsetX, OffsetY, Item.RenderSize.Width, Item.RenderSize.Height);
                DrawingContext.DrawRectangle(SourceBrush, null, Rect);

                OffsetX += ArrayWidth[i, j];
                j++;

                if (j >= ColumnMax)
                {
                    OffsetX = 0;
                    OffsetY += ArrayRowHeight[i];

                    i++;
                    j = 0;
                }
            }
        }

        RenderTarget.Render(DrawingVisual);

        return RenderTarget;
    }
}
