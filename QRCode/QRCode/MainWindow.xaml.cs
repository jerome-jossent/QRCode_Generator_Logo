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
        Properties.Settings __ = Properties.Settings.Default;
        Bitmap im;

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
            cbx_point.ItemsSource = Enum.GetValues(typeof(QRCode_.TypePoint)).Cast<QRCode_.TypePoint>();
            cbx_point.SelectedIndex = cbx_point.Items.Count - 1;
            nud_taille.Value = __.taille;
            tbx_text.Text = __.txt;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
            CvInvoke.DestroyAllWindows();
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
            img_logo.Source = Bitmap_Tools.BitmapToBitmapSource((Bitmap)System.Drawing.Image.FromFile(__.chemin_logo));
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
            img_point_image.Source = Bitmap_Tools.BitmapToBitmapSource((Bitmap)System.Drawing.Image.FromFile(__.chemin_point_image));
        }

        private void nud_taille_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            __.taille = (int)nud_taille.Value;
        }

        private void cpk_fond_Closed(object sender, RoutedEventArgs e)
        {
            __.couleur_fond = Bitmap_Tools.ColorTOColor(cpk_fond.SelectedColor);
        }

        private void cpk_point_Closed(object sender, RoutedEventArgs e)
        {
            __.couleur_point = Bitmap_Tools.ColorTOColor(cpk_point.SelectedColor);
        }

        private void tbx_text_TextChanged(object sender, TextChangedEventArgs e)
        {
            __.txt = tbx_text.Text;
        }

        private void btn_generateQRCode(object sender, RoutedEventArgs e)
        {
            System.Drawing.Color Coul_point = Bitmap_Tools.ColorTOColor(cpk_point.SelectedColor);
            System.Drawing.Color Coul_fond = Bitmap_Tools.ColorTOColor(cpk_fond.SelectedColor);
            QRCode_.TypePoint forme = (QRCode_.TypePoint)cbx_point.SelectedItem;

            QRCode_ qr = new QRCode_();
            im = qr.QRCode_Generate(__.txt,
                Coul_point,
                Coul_fond,
                parametre: __.taille,
                typpt: forme,

                rb_image: rb_image.IsChecked == true,
                rb_couleur: rb_couleur.IsChecked == true,
                ckb_logo: ckb_logo.IsChecked == true,
                dot_color: Bitmap_Tools.ColorTOBgra(cpk_point.SelectedColor),
                fond_color: Bitmap_Tools.ColorTOBgra(cpk_fond.SelectedColor),

                chemin_point_image: __.chemin_point_image,
                chemin_logo: __.chemin_logo,

                intervale_de_satisfaction: __.intervale_de_satisfaction,

                logo_facteur_de_surface: 0.01
                );

            tb.AppendText(string.Join("\n", qr.messages));
            tb.ScrollToEnd();
            tb.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);

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

        private static Action EmptyDelegate = delegate () { };
        void dessine(Bitmap bmp)
        {
            BitmapSource bmps = Bitmap_Tools.BitmapToBitmapSource((Bitmap)bmp.Clone());
            img_QRCode.Source = null;
            img_QRCode.Source = bmps;
            img_QRCode.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }
        #endregion
    }
}