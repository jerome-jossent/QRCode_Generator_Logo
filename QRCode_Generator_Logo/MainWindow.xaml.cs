using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;


//Install-Package ZXing.Net
//Install-Package ZXing.Net.Bindings.Windows.Compatibility
using ZXing;

using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static System.Net.Mime.MediaTypeNames;
using ZXing.Common;
using ZXing.Windows.Compatibility;
using System.Collections;
using Microsoft.Win32;
using System.IO;
using System.Formats.Tar;
using ZXing.Datamatrix.Encoder;
using ZXing.Datamatrix;
using OpenCvSharp.Dnn;
using System.Reflection.Metadata;

namespace QRCode_Generator_Logo
{
    public partial class MainWindow : System.Windows.Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        ZXing.Windows.Compatibility.BarcodeWriter encoder;
        ZXing.Windows.Compatibility.BarcodeReader decoder;

        Bitmap qrcode;

        public string text_input
        {
            get => _text_input;
            set
            {
                if (_text_input == value) return;
                _text_input = value;
                Compute();
                OnPropertyChanged();
            }
        }
        string _text_input;

        public string text_output
        {
            get => _text_output;
            set
            {
                if (_text_output == value) return;
                _text_output = value;
                OnPropertyChanged();
            }
        }
        string _text_output;

        public BitmapSource bitmapsource
        {
            get => _bitmapsource;
            set
            {
                if (_bitmapsource == value) return;
                _bitmapsource = value;
                OnPropertyChanged();
            }
        }
        BitmapSource _bitmapsource;

        public BitmapSource bitmapsource2
        {
            get => _bitmapsource2;
            set
            {
                if (_bitmapsource2 == value) return;
                _bitmapsource2 = value;
                OnPropertyChanged();
            }
        }
        BitmapSource _bitmapsource2;


        public System.Windows.Media.Color color_1
        {
            get => _color_1;
            set
            {
                if (_color_1 == value) return;
                _color_1 = value;
                OnPropertyChanged();
                Compute();
            }
        }
        System.Windows.Media.Color _color_1 = Colors.White;

        public System.Windows.Media.Color color_2
        {
            get => _color_2;
            set
            {
                if (_color_2 == value) return;
                _color_2 = value;
                OnPropertyChanged();
                Compute();
            }
        }
        System.Windows.Media.Color _color_2 = Colors.Purple;




        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Initialiser le générateur de QR code
            encoder = new BarcodeWriter
            {
                //Format = BarcodeFormat.DATA_MATRIX,
                Format = BarcodeFormat.QR_CODE,
                Options = new DatamatrixEncodingOptions
                {
                    Width = 500,
                    Height = 500,
                    //SymbolShape = SymbolShapeHint.FORCE_SQUARE,
                    //CompactEncoding = true,
                }
                //Renderer = new BitMatrixRenderer()

            };
            encoder.Options.Hints.Add(EncodeHintType.DISABLE_ECI, true);
            encoder.Options.Hints.Add(EncodeHintType.CHARACTER_SET, "UTF-8");

            // Initialiser le lecteur de QR code
            decoder = new ZXing.Windows.Compatibility.BarcodeReader();
        }

        void Compute()
        {
            text_output = Compute(text_input);
        }

        string Compute(string text_input)
        {
            string text_ouput;


            Gma.QrCodeNet.Encoding.QrEncoder encoder2 = new Gma.QrCodeNet.Encoding.QrEncoder();
            var code = encoder2.Encode(text_input);

            Title = code.Matrix.Width.ToString();
            Gma.QrCodeNet.Encoding.BitMatrix qr_m = code.Matrix;

            int parametre = 13;
            int diametre = parametre * 2 + 1;
            int marge = parametre * 2;

            Scalar couleur_fond = new Scalar(color_1.B, color_1.G, color_1.R, color_1.A);
            Scalar couleur_mark = new Scalar(color_2.B, color_2.G, color_2.R, color_2.A);
            Mat qr_i = new Mat(qr_m.Height * diametre + marge, qr_m.Width * diametre + marge, MatType.CV_8UC4, couleur_fond);

            //Scalar noir = new Scalar(0);

            //Points
            for (int x = 0; x <= qr_m.Width - 1; x++)
                for (int y = 0; y <= qr_m.Height - 1; y++)
                    if (qr_m[x, y])
                    {
                        int X = x * diametre + marge;
                        int Y = y * diametre + marge;
                        //qr_i.Set(Y, X, 0); // noir pour les pixels du QR code

                        Cv2.Rectangle(qr_i, new OpenCvSharp.Rect(X, Y, diametre, diametre), couleur_mark, thickness: -1);

                    }



            Display_QRCode2(encoder.Write(text_input));


            System.Drawing.Bitmap image = qr_i.ToBitmap();
            Display_QRCode(image);

            // Décoder le QR code à partir de l'image bitmap
            Result result = decoder.Decode(image);
            if (result != null)
            {
                text_ouput = result.Text;
                _tbk_output.Background = new SolidColorBrush(Colors.White);
            }
            else
            {
                text_ouput = "";
                _tbk_output.Background = new SolidColorBrush(Colors.Red);
            }
            return text_ouput;
        }

        void Display_QRCode(System.Drawing.Bitmap image)
        {
            qrcode = image;
            bitmapsource = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(image);
        }

        void Display_QRCode2(System.Drawing.Bitmap image)
        {
            bitmapsource2 = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(image);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void Save()
        {
            if (qrcode == null) return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            List<string> filters = new List<string>();
            filters.Add("PNG (*.png)|*.png");
            filters.Add("JPG (*.jpg)|*.jpg");
            filters.Add("BMP (*.bmp)|*.bmp");

            saveFileDialog.Filter = string.Join("|", filters);

            if (saveFileDialog.ShowDialog() == true)
            {
                FileInfo file = new FileInfo(saveFileDialog.FileName);
                System.Drawing.Imaging.ImageFormat format = null;
                switch (file.Extension)
                {
                    case ".png": format = System.Drawing.Imaging.ImageFormat.Png; break;
                    case ".jpg": format = System.Drawing.Imaging.ImageFormat.Jpeg; break;
                    case ".bmp":
                    default: format = System.Drawing.Imaging.ImageFormat.Bmp; break;
                }
                qrcode.Save(saveFileDialog.FileName, format);
            }
        }
    }
}
