using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace QRCode
{
    internal static class Bitmap_Tools
    {
        public static System.Drawing.Color ColorTOColor(System.Windows.Media.Color? Couleur)
        {
            System.Windows.Media.Color C = (System.Windows.Media.Color)Couleur;
            return System.Drawing.Color.FromArgb(C.A, C.R, C.G, C.B);
        }
        //public static Bgra ColorTOBgra(System.Windows.Media.Color? Couleur)
        //{
        //    System.Windows.Media.Color C = (System.Windows.Media.Color)Couleur;
        //    return new Bgra(C.B, C.G, C.R, C.A);
        //}

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

        public static void BitmapAddBitmap(Bitmap img, Bitmap logo, double logo_facteur_de_surface)
        {
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
    }
}
