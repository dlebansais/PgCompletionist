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
