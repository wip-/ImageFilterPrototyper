using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Media;
using ImageFilterPrototyper.Filters;
using Microsoft.Win32;


namespace ImageFilterPrototyper
{

    public static class Helper
    {
        static public int GetComponentsNumber(System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    return 1;

                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return 3;

                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return 4;

                default:
                    Debug.Assert(false);
                    return 0;
            }
        }
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        String ImageSourceFileName;
        BitmapInfo bitmapInfoSource;
        BitmapInfo bitmapInfoFiltered;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            String infoMsg = Window_OnDrop_Sub(e);
            if (infoMsg != null)
            {
                MessageBox.Show(infoMsg);
            }
        }

        private String Window_OnDrop_Sub(DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return "Not a file!";

            String[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length > 1)
                return "Too many files!";

            ImageSourceFileName = files[0];

            if (!File.Exists(ImageSourceFileName))
                return "Not a file!";

            FileStream fs = null;
            try
            {
                fs = File.Open(ImageSourceFileName, FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                if (fs != null)
                    fs.Close();
                return "File already in use!";
            }


            Bitmap bitmapSource = null;
            try
            {
                bitmapSource = new Bitmap(fs);
            }
            catch (System.Exception /*ex*/)
            {
                bitmapSource.Dispose();
                return "Not an image!";
            }

            ImageSource.Source =
                Imaging.CreateBitmapSourceFromHBitmap(
                    bitmapSource.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            bitmapInfoSource = new BitmapInfo(bitmapSource);

            UpdateFilteredImage();

            return null;
        }


        private void UpdateFilteredImage()
        {
            //IFilter filter = new IdentityFilter();
            //IFilter filter = new LensFilter();
            IFilter filter = new WarmingFilter85();

            ParameterizedFilter parameterizedFilter = filter as ParameterizedFilter;
            if (parameterizedFilter != null)
                parameterizedFilter.Parameter = SliderDensity.Value;

            bitmapInfoFiltered = filter.GetFilteredImage(bitmapInfoSource);

            ImageFiltered.Source =
                Imaging.CreateBitmapSourceFromHBitmap(
                    bitmapInfoFiltered.ToBitmap().GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }



        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            double vert = ((ScrollViewer)sender).VerticalOffset;
            double hori = ((ScrollViewer)sender).HorizontalOffset;

            ScrollViewer[] scrollViewers = new ScrollViewer[] { ScrollViewerSource, ScrollViewerFiltered};

            foreach (ScrollViewer scrollViewer in scrollViewers)
            {
                scrollViewer.ScrollToVerticalOffset(vert);
                scrollViewer.ScrollToHorizontalOffset(hori);
                scrollViewer.UpdateLayout();
            }
        }

        private void MyCatch(System.Exception ex)
        {
            var st = new StackTrace(ex, true);      // stack trace for the exception with source file information
            var frame = st.GetFrame(0);             // top stack frame
            String sourceMsg = String.Format("{0}({1})", frame.GetFileName(), frame.GetFileLineNumber());
            Console.WriteLine(sourceMsg);
            MessageBox.Show(ex.Message + Environment.NewLine + sourceMsg);
            Debugger.Break();
        }

        private void Zoom(double val)
        {
            try
            {
                ScaleTransform myScaleTransform = new ScaleTransform();
                myScaleTransform.ScaleY = val;
                myScaleTransform.ScaleX = val;
                if (LabelZoom != null)
                    LabelZoom.Content = val;
                TransformGroup myTransformGroup = new TransformGroup();
                myTransformGroup.Children.Add(myScaleTransform);

                System.Windows.Controls.Image[] images =
                    new System.Windows.Controls.Image[] { ImageSource, ImageFiltered};

                foreach (System.Windows.Controls.Image image in images)
                {
                    if (image == null || image.Source == null)
                        continue;
                    //image.RenderTransform = myTransformGroup;
                    image.LayoutTransform = myTransformGroup;
                }
            }
            catch (System.Exception ex)
            {
                MyCatch(ex);
            }
        }

        private void SliderZoomOut_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SliderZoomOut.Value != 1)
                Zoom(SliderZoomOut.Value);
            if (SliderZoomIn != null)
                SliderZoomIn.Value = 1;
        }

        private void SliderZoomIn_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SliderZoomIn.Value != 1)
                Zoom(SliderZoomIn.Value);
            if (SliderZoomOut != null)
                SliderZoomOut.Value = 1;
        }

        private void ButtonResetZoom_Click(object sender, RoutedEventArgs e)
        {
            Zoom(1);
            SliderZoomIn.Value = 1;
            SliderZoomOut.Value = 1;
        }

        private void ButtonSaveResult_Click(object sender, RoutedEventArgs e)
        {
            if (bitmapInfoFiltered == null)
                return;

            SaveFileDialog dialogSaveFile = new SaveFileDialog();
            dialogSaveFile.Filter = "Supported images|*.png";
            dialogSaveFile.InitialDirectory = Path.GetDirectoryName(ImageSourceFileName);
            dialogSaveFile.FileName = AddToFileName(ImageSourceFileName, "-filtered");

            if ((bool)dialogSaveFile.ShowDialog())
            {
                Stream saveStream;
                if ((saveStream = dialogSaveFile.OpenFile()) != null)
                {
                    bitmapInfoFiltered.ToBitmap().Save(saveStream, ImageFormat.Png);
                    saveStream.Close();
                }
            }
        }

        private String AddToFileName(String filename, String addChars)
        {
            return Path.GetFileNameWithoutExtension(filename) + addChars + Path.GetExtension(filename);
        }

        private void ImageSource_MouseMove(object sender, MouseEventArgs e)
        {
            Image_MouseMove(ImageSource, e);
        }

        private void ImageFiltered_MouseMove(object sender, MouseEventArgs e)
        {
            Image_MouseMove(ImageFiltered, e);
        }

        private void Image_MouseMove(System.Windows.Controls.Image clickedImage, MouseEventArgs e)
        {
            int x = (int)(e.GetPosition(clickedImage).X);
            int y = (int)(e.GetPosition(clickedImage).Y);

            BitmapInfo[] bitmapInfos =
                new BitmapInfo[] { bitmapInfoSource, bitmapInfoFiltered};

            System.Windows.Controls.Label[] labels =
                new System.Windows.Controls.Label[] { LabelColorSource, LabelColorFiltered};

            LabelInfo.Content = String.Format("X={0:D4}, Y={1:D4}", x, y);

            for (int i = 0; i < 2; ++i)
            {
                if (bitmapInfos[i] == null) continue;

                System.Drawing.Color color = bitmapInfos[i].GetPixelColor(x, y);
                float hue = color.GetHue();
                labels[i].Content = String.Format("A={0:D3}, R={1:D3}, G={2:D3}, B={3:D3}, H={4:###.##}",
                    color.A, color.R, color.G, color.B, hue);
            }
        }

        private void SliderDensity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LabelParameterValue.Content = String.Format("{0:F5}", SliderDensity.Value);

            if (bitmapInfoSource != null)
                UpdateFilteredImage();
        }

    }
}
