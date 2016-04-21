//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
//using System.Windows.Input;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;

using System;
using System.Windows;
using System.IO;
using System.Globalization;
using System.Drawing;
using System.Drawing.Imaging;

namespace GPS_Linker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenImage_Button_Click(object sender, RoutedEventArgs e)
        {
            var file_opening_dialog = new Microsoft.Win32.OpenFileDialog
            {
                //DefaultExt = ".txt",
                Filter = "JPEG file (*.jpg;*.jpeg)|*.jpg;*.jpeg",
                CheckFileExists = true,
                //Multiselect = true
            };



            //http://habrahabr.ru/post/134774/
            if (file_opening_dialog.ShowDialog(this) == true)
            {
                var photoPath = file_opening_dialog.FileName;

                double? latitude = null;
                double? longtitude = null;

                //Image image = new Image();

                try
                {
                    var image = Image.FromFile(photoPath);

                    latitude = GetLatitude(image);
                    longtitude = GetLongitude(image);
                }
                catch
                {
                    PhotoFilename_Label.Content = "";
                    GoogleEarth_TextBox.Text = "";
                    MessageBox.Show("Unable open image!", "Something went wrong...", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                PhotoFilename_Label.Content = file_opening_dialog.SafeFileName.Replace("_", "__");

                //var latitude = GetLatitude(image);
                //var longtitude = GetLongitude(image);

                if(latitude == null || longtitude == null)
                {
                    GoogleEarth_TextBox.Text = "There is no GPS metadata";
                    return;
                }

                //https://www.google.ru/maps/@59.9315899,30.3029563,14z
                var googleMapsLink = String.Format("https://www.google.ru/maps/@{0},{1},20z", latitude.ToString().Replace(',', '.'), longtitude.ToString().Replace(',', '.'));

                GoogleEarth_TextBox.Text = googleMapsLink;

                //FileStream Foto = File.Open(photoPath, FileMode.Open, FileAccess.Read); // открыли файл по адресу s для чтения
                //BitmapDecoder decoder = JpegBitmapDecoder.Create(Foto, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default); //"распаковали" снимок и создали объект decoder
                //BitmapMetadata TmpImgEXIF = (BitmapMetadata)decoder.Frames[0].Metadata.Clone(); //считали и сохранили метаданные
            }
        }



        public static double? GetLatitude(Image targetImg)
        {
            try
            {
                //Property Item 0x0001 - PropertyTagGpsLatitudeRef
                PropertyItem propItemRef = targetImg.GetPropertyItem(1);
                //Property Item 0x0002 - PropertyTagGpsLatitude
                PropertyItem propItemLat = targetImg.GetPropertyItem(2);
                return ExifGpsToDouble(propItemRef, propItemLat);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }


        public static double? GetLongitude(Image targetImg)
        {
            try
            {
                ///Property Item 0x0003 - PropertyTagGpsLongitudeRef
                PropertyItem propItemRef = targetImg.GetPropertyItem(3);
                //Property Item 0x0004 - PropertyTagGpsLongitude
                PropertyItem propItemLong = targetImg.GetPropertyItem(4);
                return ExifGpsToDouble(propItemRef, propItemLong);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }



        private static double ExifGpsToDouble(PropertyItem propItemRef, PropertyItem propItem)
        {
            double degreesNumerator = BitConverter.ToUInt32(propItem.Value, 0);
            double degreesDenominator = BitConverter.ToUInt32(propItem.Value, 4);
            double degrees = degreesNumerator / (double)degreesDenominator;

            double minutesNumerator = BitConverter.ToUInt32(propItem.Value, 8);
            double minutesDenominator = BitConverter.ToUInt32(propItem.Value, 12);
            double minutes = minutesNumerator / (double)minutesDenominator;

            double secondsNumerator = BitConverter.ToUInt32(propItem.Value, 16);
            double secondsDenominator = BitConverter.ToUInt32(propItem.Value, 20);
            double seconds = secondsNumerator / (double)secondsDenominator;


            double coorditate = degrees + (minutes / 60d) + (seconds / 3600d);
            string gpsRef = System.Text.Encoding.ASCII.GetString(new byte[1] { propItemRef.Value[0] }); //N, S, E, or W
            if (gpsRef == "S" || gpsRef == "W")
                coorditate = coorditate * -1;
            return coorditate;
        }
    }
}
