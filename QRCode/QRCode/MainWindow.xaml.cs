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
using System.IO;
using System.Windows.Threading;
using System.Threading;
using System.Drawing.Imaging;
using Microsoft.Win32;

using MessagingToolkit.QRCode.Codec;
using MessagingToolkit.QRCode.Codec.Data;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using System.Runtime.InteropServices;
using Emgu.CV.Util;

namespace QRCode
{
    public partial class MainWindow : Window
    {
        enum TypePoint { Carrée }//, Disque, Hexagone}

        #region Variables
        Properties.Settings __ = Properties.Settings.Default;

        Gma.QrCodeNet.Encoding.QrCode Code;
        Gma.QrCodeNet.Encoding.QrEncoder Encoder = new Gma.QrCodeNet.Encoding.QrEncoder(Gma.QrCodeNet.Encoding.ErrorCorrectionLevel.H);
        //string chemin_logo = "";
        //string chemin_point_image = "";
        //double intervale_de_satisfaction = 0.003;
        Image<Bgra, Byte> EmguImageCouleur;
        Bitmap im;
        List<test> tests;
        #endregion

        #region IHM
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cpk_point.SelectedColor = System.Windows.Media.Color.FromArgb(255,
                __.couleur_point.R, __.couleur_point.G, __.couleur_point.B);
            cpk_fond.SelectedColor = System.Windows.Media.Color.FromArgb(255,
                __.couleur_fond.R, __.couleur_fond.G, __.couleur_fond.B);
            cbx_point.ItemsSource = Enum.GetValues(typeof(TypePoint)).Cast<TypePoint>();
            cbx_point.SelectedIndex = cbx_point.Items.Count-1;
            nud_taille.Value = __.taille;
            tbx_text.Text = __.txt;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }


        private void btn_logo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != true)
            {
                __.chemin_logo = "";
                img_logo.Source = null;
                return;
            }
            __.chemin_logo = ofd.FileName;
            img_logo.Source = BitmapToBitmapSource((Bitmap)System.Drawing.Image.FromFile(__.chemin_logo));
        }
        
        private void rb_couleur_Checked(object sender, RoutedEventArgs e) { IHM_update(); }
        private void rb_image_Checked(object sender, RoutedEventArgs e) { IHM_update(); }
        private void ckb_logo_Checked(object sender, RoutedEventArgs e) { IHM_update(); }

        private void IHM_update()
        {
            if (cpk_point == null) return;
            cpk_point.IsEnabled = (rb_couleur.IsChecked == true);
            btn_point_image.IsEnabled = (rb_image.IsChecked == true);
            img_point_image.IsEnabled = (rb_image.IsChecked != true);
            btn_logo.IsEnabled = (ckb_logo.IsChecked == true);
        }

        private void btn_point_image_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != true)
            {
                __.chemin_point_image = "";
                img_point_image.Source = null;
                return;
            }
            __.chemin_point_image = ofd.FileName;
            img_point_image.Source = BitmapToBitmapSource((Bitmap)System.Drawing.Image.FromFile(__.chemin_point_image));
        }
        
        private void nud_taille_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            __.taille = (int)nud_taille.Value;
        }

        private void cpk_fond_Closed(object sender, RoutedEventArgs e)
        {
            __.couleur_fond = ColorTOColor(cpk_fond.SelectedColor);
        }

        private void cpk_point_Closed(object sender, RoutedEventArgs e)
        {
            __.couleur_point = ColorTOColor(cpk_point.SelectedColor);
        }

        private void tbx_text_TextChanged(object sender, TextChangedEventArgs e)
        {
            __.txt = tbx_text.Text;
        }
        
        private void btn_generateQRCode(object sender, RoutedEventArgs e)
        {
            System.Drawing.Color Coul_point = ColorTOColor(cpk_point.SelectedColor);
            System.Drawing.Color Coul_fond = ColorTOColor(cpk_fond.SelectedColor);
            TypePoint forme = (TypePoint)cbx_point.SelectedItem;

            im = QRCode_Generate(__.txt, 
                Coul_point, 
                Coul_fond, 
                __.taille, 
                forme,
                logo_facteur_de_surface: 0.01);
            dessine(im);
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            if (im == null)
                return;

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Image PNG|*.png";
            if (sfd.ShowDialog() != true)
                return;
            im.Save(sfd.FileName);
        }

        #endregion

        #region QR Code
        private Bitmap QRCode_Generate(string TXT, System.Drawing.Color Coul_point, System.Drawing.Color Coul_fond, int paramètre, TypePoint typpt, string cheminlogo="", double logo_facteur_de_surface = 0.1, bool logo_facteur_de_surface_auto = true)
        {
            bool QRCode_OK;
            Bitmap img;
            tests = new List<test>();
            bool onreboucle;
            string commentaires;
            double val;

            Code = Encoder.Encode(TXT);

            tb.AppendText(Code.Matrix.Width + " x " + Code.Matrix.Height + "\n");
            tb.ScrollToEnd();

            int diametre = paramètre * 2 + 1;

            Bgra coul = new Bgra(Coul_fond.B, Coul_fond.G, Coul_fond.R, Coul_fond.A);
            Bgra coul_pt = new Bgra(Coul_point.B, Coul_point.G, Coul_point.R, Coul_point.A);

            Image<Gray, byte> EmguQR_NB = new Image<Gray, byte>(Code.Matrix.Width * diametre, Code.Matrix.Height * diametre);
            Gray blanc = new Gray(255);

//            InputArray hexagone = null;
//            System.Drawing.Point[] hexagone = null;
//            VectorOfPoint vectorOfPoint = null;
//            if (typpt == TypePoint.Hexagone)
//            {
//                hexagone = new System.Drawing.Point[]
////                hexagone = new InputArray()
//                {
//                    new System.Drawing.Point(5, 0),
//                    new System.Drawing.Point(10, 3),
//                    new System.Drawing.Point(10, 6),
//                    new System.Drawing.Point(5, 9),
//                    new System.Drawing.Point(0, 6),
//                    new System.Drawing.Point(0, 3)
//                };
//                vectorOfPoint = new VectorOfPoint(hexagone);

//                VectorOfPoint testPoint = new VectorOfPoint();

//                CvInvoke.ApproxPolyDP(vectorOfPoint, testPoint, 1.0, true);

//                CvInvoke.NamedWindow("First");
//                MCvScalar scalar = new MCvScalar(0);
//                Mat blackMat = new Mat(originalImage.Size, DepthType.Cv8U, originalImage.NumberOfChannels);
//                blackMat.SetTo(scalar);


                //CvInvoke.Imshow("First", blackMat);
//            }
            
            //Points
            for (int X = 0; X <= Code.Matrix.Width - 1; X++)
                for (int Y = 0; Y <= Code.Matrix.Height - 1; Y++)
                    if (Code.Matrix.InternalArray[X, Y])
                    {
                        int x = X * diametre;
                        int y = Y * diametre;
                        switch (typpt)
                        {
                            case TypePoint.Carrée:
                                EmguQR_NB.Draw(new System.Drawing.Rectangle(x, y, diametre, diametre), blanc , thickness: -1);
                                break;
                            //case TypePoint.Disque:
                            //    EmguQR_NB.Draw(new CircleF(new PointF(x + paramètre, y + paramètre), paramètre), blanc, thickness: -1);
                            //    break;
                            //case TypePoint.Hexagone:

                            //    EmguQR_NB.Draw(new System.Drawing.Rectangle(x + 3, y, 1, 1), blanc, thickness: -1);

                            //    EmguQR_NB.Draw(new System.Drawing.Rectangle(x + 1, y + 1, 5, 1), blanc, thickness: -1);

                            //    EmguQR_NB.Draw(new System.Drawing.Rectangle(x, y + 2, 7, 3), blanc, thickness: -1);
                                
                            //    EmguQR_NB.Draw(new System.Drawing.Rectangle(x + 1, y + 5, 5, 1), blanc, thickness: -1);

                            //    EmguQR_NB.Draw(new System.Drawing.Rectangle(x + 3, y + 6, 1, 1), blanc, thickness: -1);

                            //    EmguQR_NB.Draw(new System.Drawing.Rectangle(x + 3, y + 3, 1, 1), new Gray(0) , thickness: -1);
                                
                            //    break;
                            default:
                                break;
                        }
                    }

            //convert NB to Color
            EmguQR_NB = EmguQR_NB.Not();
            Image<Bgra, Byte> mask = EmguQR_NB.Convert<Bgra, byte>();
            Image<Bgra, Byte> fond=null;
            
            if (rb_couleur.IsChecked == true)
                fond = new Image<Bgra, byte>(mask.Width, mask.Height, ColorTOBgra(cpk_point.SelectedColor));

            if (rb_image.IsChecked == true)
                fond = new Image<Bgra, byte>(__.chemin_point_image);

            fond = fond.Resize(mask.Width, mask.Height, Inter.Lanczos4);
            EmguImageCouleur = fond + mask;

            Image<Bgra, Byte> EmguImageSortie = EmguImageCouleur;//.SmoothGaussian(3);

            //Test du QRCode
            img = EmguImageSortie.ToBitmap();
            QRCode_OK = QRCode_Check(TXT, img, out commentaires, "");
            Disp(commentaires);
            if (!QRCode_OK)
            {
                dessine(img);
                MessageBox.Show("Echec, modifier les paramètres.\n\n" + commentaires, ":(", MessageBoxButton.OK, MessageBoxImage.Error);
                return img;
            }

            //Ajout d'un logo
            if (ckb_logo.IsChecked == true && __.chemin_logo != "")
            { 
                do
                {
                    img = EmguImageSortie.ToBitmap();

                    val = nextValueToTest(logo_facteur_de_surface);
                    if (val == -1)
                    {
                        tb.AppendText("Echec");
                        break;
                    }
                    test t = new test(val);

                    BitmapAddBitmap(img, __.chemin_logo, val);

                    QRCode_OK = QRCode_Check(TXT, img, out commentaires, "f=" + val);
                    t.tested = true;
                    t.resultat = QRCode_OK;
                    tests.Add(t);

                    Disp(commentaires);

                    dessine(img);

                    if (!logo_facteur_de_surface_auto)
                        onreboucle = false;
                    else
                        onreboucle = needToTestMore(__.intervale_de_satisfaction);
                } while (onreboucle);

                if (tests[tests.Count - 1].resultat != true)
                {
                    img = EmguImageSortie.ToBitmap();
                    val = getStrongerOK();
                    BitmapAddBitmap(img, __.chemin_logo, val);
                    QRCode_OK = QRCode_Check(TXT, img, out commentaires, "f=" + val);
                    Disp(commentaires);
                }
            }

            //CvInvoke.PutText(EmguImageCouleur, "J", new System.Drawing.Point(3 * diametre + 2, 4 * diametre), FontFace.HersheyComplex, 1.0, new Bgr(255, 255, 255).MCvScalar);
            //CvInvoke.PutText(EmguImageCouleur, "J", new System.Drawing.Point(45 * diametre + 2, 4 * diametre), FontFace.HersheyComplex, 1.0, new Bgr(255, 255, 255).MCvScalar);
            //CvInvoke.PutText(EmguImageCouleur, "J", new System.Drawing.Point(3 * diametre + 2, 46 * diametre), FontFace.HersheyComplex, 1.0, new Bgr(255, 255, 255).MCvScalar);
            img = EmguImageSortie.ToBitmap();

            if (ckb_logo.IsChecked == true && __.chemin_logo != "")
            {
                val = getStrongerOK();
                BitmapAddBitmap(img, __.chemin_logo, val);
            }

            MessageBox.Show("Succès.", ":)", MessageBoxButton.OK, MessageBoxImage.Information);
            dessine(img);
            return img;
        }

        private void Disp(string commentaires)
        {
            tb.AppendText(commentaires);
            tb.ScrollToEnd();
            tb.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }

        private bool QRCode_Check(string TXT, Bitmap img, out string message, string param = "")
        {
            QRCodeDecoder decoder = new QRCodeDecoder();
            string message_décodé;
            bool res;
            try
            {
                message_décodé = decoder.Decode(new QRCodeBitmapImage(img));

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

        private double nextValueToTest(double valInit)
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
            {                //alors on veut essayer -
                double val = lowerFailed - 0.05;
                if (val < 0.001)
                {
                    bool dejatester = false;
                    foreach (test item in tests)
                    {
                        if (item.facteur == 0.001)
                        {
                            dejatester = true;
                            break;
                        }
                    }                    
                    val = dejatester?-1:0.001;
                }

                return val;
            }
            else
                return valInit;
        }   

        private bool needToTestMore(double intervale_de_satisfaction)
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

        private double getStrongerOK()
        {   //Trouver le résultat OK avec la valeur la plus forte
            double strongerOK = -1;

            foreach (test t in tests)
                if (t.tested)
                    if (t.resultat == true)
                        if (strongerOK == -1)
                            strongerOK = t.facteur;
                        else
                            if (strongerOK < t.facteur)
                            strongerOK = t.facteur;
            return strongerOK;
        }
        
        private double getlowerFailed()
        {
            //Trouver le résultat échec avec la valeur la plus faible
            double lowerFailed = -1;
            foreach (test t in tests)
                if (t.tested)
                    if (t.resultat == false)
                        if (lowerFailed == -1)
                            lowerFailed = t.facteur;
                        else
                            if (lowerFailed > t.facteur)
                            lowerFailed = t.facteur;
            return lowerFailed;
        }
        #endregion
      
        void dessine(Bitmap bmp)
        {
            BitmapSource bmps = BitmapToBitmapSource((Bitmap)bmp.Clone());
            img_QRCode.Source = null;
            img_QRCode.Source = bmps;
            img_QRCode.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }
        private static Action EmptyDelegate = delegate () { };

        #region Méthodes Caisse à outils
        public static System.Drawing.Color ColorTOColor(System.Windows.Media.Color? Couleur)
        {
            System.Windows.Media.Color C = (System.Windows.Media.Color)Couleur;
            return System.Drawing.Color.FromArgb(C.A, C.R, C.G, C.B);
        }
        public static Bgra ColorTOBgra(System.Windows.Media.Color? Couleur)
        {
            System.Windows.Media.Color C = (System.Windows.Media.Color)Couleur;
            return new Bgra(C.B, C.G, C.R, C.A);
        }

        public static System.Drawing.Image BitmapToImage(Bitmap bmp)
        {
            return bmp;
        }

        public static BitmapSource BitmapToBitmapSource(Bitmap bitmap)
        {
            if (bitmap == null)
                throw new ArgumentNullException("bitmap");

            var rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);

            var bitmapData = bitmap.LockBits(
                rect,
                ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            try
            {
                var size = (rect.Width * rect.Height) * 4;

                return BitmapSource.Create(
                    bitmap.Width,
                    bitmap.Height,
                    bitmap.HorizontalResolution,
                    bitmap.VerticalResolution,
                    PixelFormats.Bgra32,
                    null,
                    bitmapData.Scan0,
                    size,
                    bitmapData.Stride);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }

        public static Bitmap BitmapResize(Bitmap imgToResize, System.Drawing.Size size)
        {
            return (new Bitmap(imgToResize, size));
        }
        
        private static void BitmapAddBitmap(Bitmap img, string chemin_logo, double logo_facteur_de_surface)
        {
            //Chargement de la petite image
            Bitmap logo = new Bitmap(chemin_logo);

            //on redimensionne
            float r = (float)logo.Width / logo.Height;
            int s_qr_masque = (int)(img.Width * img.Height * logo_facteur_de_surface);
            int h = (int)Math.Pow(s_qr_masque / r, 0.5);
            int w = (int)(h * r);
            logo = BitmapResize(logo, new System.Drawing.Size(w, h));
           
            //on ajoute
            Graphics g = Graphics.FromImage(img);
            int left = (img.Width / 2) - (logo.Width / 2);
            int top = (img.Height / 2) - (logo.Height / 2);
            g.DrawImage(logo, new System.Drawing.Point(left, top));            
        }

        #region "Emgu to WPF"
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);
        public static BitmapSource EmguImageToBitmapSource(IImage image)
        {
            using (System.Drawing.Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap(); //obtain the Hbitmap

                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr); //release the HBitmap
                return bs;
            }
        }
        #endregion

        #endregion


    }

    class test
    {
        public double facteur = -1;
        public bool tested;
        public bool resultat;

        public test(double val)
        {
            facteur = val;
            tested = false;
            resultat = false;
        }
    }
}