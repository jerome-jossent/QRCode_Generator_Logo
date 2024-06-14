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
using System.Security.Policy;
using QRCode;
using OpenCvSharp.WpfExtensions;

namespace QRCode_Generator_Logo
{
    public partial class MainWindow : System.Windows.Window, INotifyPropertyChanged
    {
        Gma.QrCodeNet.Encoding.QrEncoder encoder;
        ZXing.Windows.Compatibility.BarcodeReader decoder;
        Bitmap qrcode_image;
        Bitmap logo;
        List<Test> tests;

        public enum TypePoint { Carrée, Disque, Arrondi }

        #region DATABINDING
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

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

        public BitmapSource bitmapsource_logo
        {
            get => _bitmapsource_logo;
            set
            {
                if (_bitmapsource_logo == value) return;
                _bitmapsource_logo = value;
                OnPropertyChanged();
            }
        }
        BitmapSource _bitmapsource_logo;

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

        public int? size
        {
            get => _size;
            set
            {
                if (_size == value) return;
                _size = value;
                OnPropertyChanged();
                Compute();
            }
        }
        int? _size = 13;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Init_IHM();
            Init_QRCodeEngines();
        }

        void Init_IHM()
        {
            cbx_point.ItemsSource = Enum.GetValues(typeof(TypePoint)).Cast<TypePoint>();
            cbx_point.SelectedIndex = 0;
        }

        void Init_QRCodeEngines()
        {
            // Initialiser le lecteur de QR code
            encoder = new Gma.QrCodeNet.Encoding.QrEncoder(Gma.QrCodeNet.Encoding.ErrorCorrectionLevel.H);
            decoder = new ZXing.Windows.Compatibility.BarcodeReader();
        }

        private void cbx_point_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            Compute();
        }

        void Compute()
        {
            text_output = Compute(text_input);

            //colore en rouge le texte de vérification si il est différent du texte entrée
            _tbk_output.Background = (text_output == text_input) ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Red);
        }

        string Compute(string text_input)
        {
            if (encoder == null) return "";

            Gma.QrCodeNet.Encoding.QrCode code = encoder.Encode(text_input);
            //récupération de la matrice générée
            Gma.QrCodeNet.Encoding.BitMatrix qr_m = code.Matrix;

            int taille_px = (int)size;
            int diametre = taille_px * 2 + 1;
            int marge = taille_px * 2;

            Scalar couleur_fond = new Scalar(color_1.B, color_1.G, color_1.R, color_1.A);
            Scalar couleur_mark = new Scalar(color_2.B, color_2.G, color_2.R, color_2.A);
            //Mat qr_i = new Mat(qr_m.Height * diametre + marge, qr_m.Width * diametre + marge, MatType.CV_8UC4, couleur_fond);
            Mat qr_i = new Mat(qr_m.Height * diametre, qr_m.Width * diametre, MatType.CV_8UC4, couleur_fond);

            TypePoint typpt = (TypePoint)cbx_point.SelectedItem;

            //Points
            for (int x = 0; x < qr_m.Width; x++)
                for (int y = 0; y < qr_m.Height; y++)
                    if (qr_m[x, y])
                    {
                        int X = x * diametre;//+ marge;
                        int Y = y * diametre;// + marge;
                        DessinePoint(qr_i, typpt, X, Y, diametre, couleur_mark, taille_px);
                    }

            double logo_facteur_de_surface = 0.1;
            double val;
            tests = new List<Test>();
            Bitmap img;
            bool logo_facteur_de_surface_auto = true;
            bool onreboucle;
            double intervale_de_satisfaction = 0.01;

            //Ajout d'un logo
            if (logo != null)
            {
                do
                {
                    img = qr_i.ToBitmap();

                    val = nextValueToTest(logo_facteur_de_surface);
                    if (val == -1)
                    {
                        //tb.AppendText("Echec");
                        //messages.Add("Echec");
                        break;
                    }
                    Test t = new Test(val);

                    Bitmap_Tools.BitmapAddBitmap(img, logo, val);

                    bool QRCode_OK = QRCode_Check(text_input, img, out string commentaires, "f=" + val);
                    t.tested = true;
                    t.resultat = QRCode_OK;
                    tests.Add(t);

                    //Disp(commentaires);
                    //dessine(img);
                    Display_QRCode(img);

                    if (!logo_facteur_de_surface_auto)
                        onreboucle = false;
                    else
                        onreboucle = needToTestMore(intervale_de_satisfaction);
                } while (onreboucle);

                if (tests[tests.Count - 1].resultat != true)
                {
                    img = qr_i.ToBitmap();
                    val = getStrongerOK();
                    if (val > 0)
                    {
                        Bitmap_Tools.BitmapAddBitmap(img, logo, val);
                        bool QRCode_OK = QRCode_Check(text_input, img, out string commentaires, "f=" + val);
                        //Disp(commentaires);
                    }
                    else
                    {
                        img = qr_i.ToBitmap();
                        Display_QRCode(img);
                    }
                }
            }
            else
            {
                img = qr_i.ToBitmap();
                Display_QRCode(img);
            }


            string text_ouput;
            // Décoder le QR code à partir de l'image bitmap
            Result result = decoder.Decode(img);
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

        bool QRCode_Check(string TXT, Bitmap image, out string message, string param = "")
        {
            string message_décodé = "";
            bool res;
            try
            {
                Result result = decoder.Decode(image);

                if (result != null) message_décodé = result.Text;
                if (message_décodé == TXT)
                {
                    message = "QRCODE OK (" + param + ") : " + message_décodé + "\n\n";
                    res = true;
                }
                else
                {
                    message = "QRCODE altéré ! (" + param + ") " + message_décodé + "\n\n";
                    res = false;
                }
            }
            catch (Exception ex)
            {
                message = "QRCODE Echec : (" + param + ") " + ex.Message + "\n\n";
                res = false;
            }
            return res;
        }

        double nextValueToTest(double valInit)
        {
            double strongerOK = getStrongerOK();
            double lowerFailed = getlowerFailed();

            if (strongerOK != -1 && lowerFailed != -1)
                //alors on veut essayer entre les deux (dichotomie)
                return (strongerOK + lowerFailed) / 2;
            else if (strongerOK != -1)
            {
                //alors on veut essayer +
                double val = strongerOK + 0.05;
                return val;
            }
            else if (lowerFailed != -1)
            {   //alors on veut essayer -
                double val = lowerFailed - 0.05;
                if (val < 0.0001)
                {
                    bool dejatester = false;
                    foreach (Test item in tests)
                    {
                        if (item.facteur == 0.0001)
                        {
                            dejatester = true;
                            break;
                        }
                    }
                    val = dejatester ? -1 : 0.0001;
                }

                return val;
            }
            else
                return valInit;
        }

        bool needToTestMore(double intervale_de_satisfaction)
        {
            double strongerOK = getStrongerOK();
            double lowerFailed = getlowerFailed();
            //Si possible, on compare
            if (strongerOK != -1 && lowerFailed != -1)
            {
                double ecart = Math.Abs(lowerFailed - strongerOK);
                return (ecart > intervale_de_satisfaction);
            }
            else
                return true;
        }

        double getStrongerOK()
        {
            //Trouver le résultat OK avec la valeur la plus forte
            double strongerOK = -1;

            foreach (Test t in tests)
                if (t.tested)
                    if (t.resultat == true)
                        if (strongerOK == -1)
                            strongerOK = t.facteur;
                        else
                            if (strongerOK < t.facteur)
                            strongerOK = t.facteur;
            return strongerOK;
        }

        double getlowerFailed()
        {
            //Trouver le résultat échec avec la valeur la plus faible
            double lowerFailed = -1;
            foreach (Test t in tests)
                if (t.tested)
                    if (t.resultat == false)
                        if (lowerFailed == -1)
                            lowerFailed = t.facteur;
                        else
                            if (lowerFailed > t.facteur)
                            lowerFailed = t.facteur;
            return lowerFailed;
        }





        void DessinePoint(Mat qr_i, TypePoint typpt, int X, int Y, int diametre, Scalar couleur_mark, int taille_px)
        {
            switch (typpt)
            {
                case TypePoint.Carrée:
                    Cv2.Rectangle(qr_i, new OpenCvSharp.Rect(X, Y, diametre, diametre), couleur_mark, thickness: -1);
                    break;

                case TypePoint.Disque:
                    Cv2.Circle(qr_i, X, Y, (int)(diametre / 1.5), couleur_mark, thickness: -1);
                    break;

                case TypePoint.Arrondi:
                    Cv2.Circle(qr_i, X - taille_px / 4, Y - taille_px / 4, (int)(diametre / 1.9), couleur_mark, thickness: -1);
                    Cv2.Circle(qr_i, X - taille_px / 4, Y + taille_px / 4, (int)(diametre / 1.9), couleur_mark, thickness: -1);
                    Cv2.Circle(qr_i, X + taille_px / 4, Y - taille_px / 4, (int)(diametre / 1.9), couleur_mark, thickness: -1);
                    Cv2.Circle(qr_i, X + taille_px / 4, Y + taille_px / 4, (int)(diametre / 1.9), couleur_mark, thickness: -1);
                    break;

                default:
                    break;
            }

        }

        void Display_QRCode(System.Drawing.Bitmap image)
        {
            qrcode_image = image;
            bitmapsource = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(image);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void Save()
        {
            if (qrcode_image == null) return;

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
                qrcode_image.Save(saveFileDialog.FileName, format);
            }
        }

        private void Select_Logo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            List<string> filters = new List<string>();
            filters.Add("PNG (*.png)|*.png");
            filters.Add("JPG (*.jpg)|*.jpg");
            filters.Add("BMP (*.bmp)|*.bmp");

            openFileDialog.Filter = string.Join("|", filters);

            if (openFileDialog.ShowDialog() == true)
            {
                FileInfo file = new FileInfo(openFileDialog.FileName);
                LoadLogo(file);
            }
        }

        void LoadLogo(FileInfo file)
        {
            logo = new Bitmap(file.FullName);
            bitmapsource_logo = Convert(logo);
            Compute();
        }

        public BitmapImage Convert(Bitmap src)
        {
            MemoryStream ms = new MemoryStream();
            ((System.Drawing.Bitmap)src).Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }
    }
}
