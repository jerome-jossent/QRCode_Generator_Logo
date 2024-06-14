using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using MessagingToolkit.QRCode.Codec.Data;
using MessagingToolkit.QRCode.Codec;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Threading;
using System.Windows;
using System.Runtime.Remoting.Messaging;

namespace QRCode
{
    internal class QRCode_
    {

       public enum TypePoint { Carrée, Disque }
        Gma.QrCodeNet.Encoding.QrCode Code;
        Gma.QrCodeNet.Encoding.QrEncoder Encoder = new Gma.QrCodeNet.Encoding.QrEncoder(Gma.QrCodeNet.Encoding.ErrorCorrectionLevel.H);
        Image<Bgra, byte> EmguImageCouleur;
        //Bitmap im;
        List<Test> tests;

        public List<string> messages;

        #region QR Code
        public Bitmap QRCode_Generate(string TXT, 
            System.Drawing.Color Coul_point, 
            System.Drawing.Color Coul_fond, 
            int parametre, 
            TypePoint typpt, 

            bool ckb_logo,
            bool rb_couleur,
            bool rb_image,
            Bgra dot_color,
            Bgra fond_color,

            string chemin_point_image,
            string chemin_logo,

            double intervale_de_satisfaction,

            double logo_facteur_de_surface = 0.1,
            bool logo_facteur_de_surface_auto = true)

        {
            bool QRCode_OK;
            Bitmap img;
            tests = new List<Test>();
            bool onreboucle;
            string commentaires;
            double val;

            Code = Encoder.Encode(TXT);

            messages = new List<string>();
            messages.Add(Code.Matrix.Width + " x " + Code.Matrix.Height + "\n");

            int diametre = parametre * 2 + 1;

            Bgra coul = new Bgra(Coul_fond.B, Coul_fond.G, Coul_fond.R, Coul_fond.A);
            Bgra coul_pt = new Bgra(Coul_point.B, Coul_point.G, Coul_point.R, Coul_point.A);
            int marge = parametre * 2;

            //+1 pour les bords droite et bas à rajouter en +
            Image<Gray, byte> EmguQR_NB = new Image<Gray, byte>((Code.Matrix.Width + 1) * diametre + marge, (Code.Matrix.Height + 1) * diametre + marge);
            Gray blanc = new Gray(255);

            //Points
            for (int X = 0; X <= Code.Matrix.Width - 1; X++)
                for (int Y = 0; Y <= Code.Matrix.Height - 1; Y++)
                    if (Code.Matrix.InternalArray[X, Y])
                    {
                        int x = X * diametre + marge;
                        int y = Y * diametre + marge;
                        switch (typpt)
                        {
                            case TypePoint.Carrée:
                                EmguQR_NB.Draw(new System.Drawing.Rectangle(x, y, diametre, diametre), blanc, thickness: -1);
                                break;

                            case TypePoint.Disque:
                                //EmguQR_NB.Draw(new CircleF(new PointF(x, y), (int)(parametre*1.5)), blanc, thickness: -1);
                                EmguQR_NB.Draw(new CircleF(new PointF(x - parametre / 4, y - parametre / 4), (int)(parametre * 1.3)), blanc, thickness: -1);
                                EmguQR_NB.Draw(new CircleF(new PointF(x - parametre / 4, y + parametre / 4), (int)(parametre * 1.3)), blanc, thickness: -1);
                                EmguQR_NB.Draw(new CircleF(new PointF(x + parametre / 4, y - parametre / 4), (int)(parametre * 1.3)), blanc, thickness: -1);
                                EmguQR_NB.Draw(new CircleF(new PointF(x + parametre / 4, y + parametre / 4), (int)(parametre * 1.3)), blanc, thickness: -1);
                                break;

                            default:
                                break;
                        }
                    }

            //bord bas
            for (int X = 0; X <= EmguQR_NB.Cols; X++)
            {
                int x = X * diametre + marge;
                int y = EmguQR_NB.Rows;
                switch (typpt)
                {
                    case TypePoint.Carrée:
                        EmguQR_NB.Draw(new System.Drawing.Rectangle(x, y, diametre, diametre), blanc, thickness: -1);
                        break;

                    case TypePoint.Disque:
                        //EmguQR_NB.Draw(new CircleF(new PointF(x, y), (int)(parametre*1.5)), blanc, thickness: -1);
                        EmguQR_NB.Draw(new CircleF(new PointF(x - parametre / 4, y - parametre / 4), (int)(parametre * 1.3)), blanc, thickness: -1);
                        EmguQR_NB.Draw(new CircleF(new PointF(x - parametre / 4, y + parametre / 4), (int)(parametre * 1.3)), blanc, thickness: -1);
                        EmguQR_NB.Draw(new CircleF(new PointF(x + parametre / 4, y - parametre / 4), (int)(parametre * 1.3)), blanc, thickness: -1);
                        EmguQR_NB.Draw(new CircleF(new PointF(x + parametre / 4, y + parametre / 4), (int)(parametre * 1.3)), blanc, thickness: -1);
                        break;

                    default:
                        break;
                }
            }

            //bord droit
            for (int Y = 0; Y <= Code.Matrix.Height - 1; Y++)
            {
                int x = EmguQR_NB.Cols;
                int y = Y * diametre + marge;

                switch (typpt)
                {
                    case TypePoint.Carrée:
                        EmguQR_NB.Draw(new System.Drawing.Rectangle(x, y, diametre, diametre), blanc, thickness: -1);
                        break;

                    case TypePoint.Disque:
                        //EmguQR_NB.Draw(new CircleF(new PointF(x, y), (int)(parametre*1.5)), blanc, thickness: -1);
                        EmguQR_NB.Draw(new CircleF(new PointF(x - parametre / 4, y - parametre / 4), (int)(parametre * 1.3)), blanc, thickness: -1);
                        EmguQR_NB.Draw(new CircleF(new PointF(x - parametre / 4, y + parametre / 4), (int)(parametre * 1.3)), blanc, thickness: -1);
                        EmguQR_NB.Draw(new CircleF(new PointF(x + parametre / 4, y - parametre / 4), (int)(parametre * 1.3)), blanc, thickness: -1);
                        EmguQR_NB.Draw(new CircleF(new PointF(x + parametre / 4, y + parametre / 4), (int)(parametre * 1.3)), blanc, thickness: -1);
                        break;

                    default:
                        break;
                }
            }

            //convert NB to Color
            Image<Gray, byte> EmguQR_dotwhite = EmguQR_NB.Clone();

            EmguQR_NB = EmguQR_NB.Erode(2);


            EmguQR_NB = EmguQR_NB.Not();


            Image<Bgra, byte> mask = EmguQR_NB.Convert<Bgra, byte>();
            Image<Bgra, byte> fond = null;

            if (rb_couleur)
            {
                //Bgra dot_color = Bitmap_Tools.ColorTOBgra(cpk_point.SelectedColor);
                fond = new Image<Bgra, byte>(mask.Width, mask.Height, dot_color);
            }

            if (rb_image)
                fond = new Image<Bgra, byte>(chemin_point_image);

            fond = fond.Resize(mask.Width, mask.Height, Inter.Lanczos4);
            EmguImageCouleur = fond + mask;

            //Bgra fond_color = Bitmap_Tools.ColorTOBgra(cpk_fond.SelectedColor);
            Image<Bgra, byte> mask_dots = EmguQR_dotwhite.Convert<Bgra, byte>();
            Image<Bgra, byte> fond_dots = new Image<Bgra, byte>(mask.Width, mask.Height, fond_color);

            fond_dots = fond_dots.Resize(mask_dots.Width, mask_dots.Height, Inter.Lanczos4);
            Image<Bgra, byte> EmguImageCouleur_temp;
            EmguImageCouleur_temp = fond_dots + mask_dots;

            Image<Bgra, byte> EmguImageSortie = EmguImageCouleur & EmguImageCouleur_temp;

            //Test du QRCode
            img = EmguImageSortie.ToBitmap();
            QRCode_OK = QRCode_Check(TXT, img, out commentaires, "");
            Disp(commentaires);
            if (!QRCode_OK)
            {
                //dessine(img);
                //MessageBox.Show("Echec, modifier les paramètres.\n\n" + commentaires, ":(", MessageBoxButton.OK, MessageBoxImage.Error);
                return img;
            }

            //Ajout d'un logo
            if (ckb_logo && chemin_logo != "")
            {
                do
                {
                    img = EmguImageSortie.ToBitmap();

                    val = nextValueToTest(logo_facteur_de_surface);
                    if (val == -1)
                    {
                        //tb.AppendText("Echec");
                        messages.Add("Echec");
                        break;
                    }
                    Test t = new Test(val);

                    Bitmap_Tools.BitmapAddBitmap(img, chemin_logo, val);

                    QRCode_OK = QRCode_Check(TXT, img, out commentaires, "f=" + val);
                    t.tested = true;
                    t.resultat = QRCode_OK;
                    tests.Add(t);

                    Disp(commentaires);
                    //dessine(img);

                    if (!logo_facteur_de_surface_auto)
                        onreboucle = false;
                    else
                        onreboucle = needToTestMore(intervale_de_satisfaction);
                } while (onreboucle);

                if (tests[tests.Count - 1].resultat != true)
                {
                    img = EmguImageSortie.ToBitmap();
                    val = getStrongerOK();
                    Bitmap_Tools.BitmapAddBitmap(img, chemin_logo, val);
                    QRCode_OK = QRCode_Check(TXT, img, out commentaires, "f=" + val);
                    Disp(commentaires);
                }
            }

            img = EmguImageSortie.ToBitmap();

            if (ckb_logo && chemin_logo != "")
            {
                val = getStrongerOK();
                Bitmap_Tools.BitmapAddBitmap(img, chemin_logo, val);
            }

            MessageBox.Show("Succès.", ":)", MessageBoxButton.OK, MessageBoxImage.Information);
            //dessine(img);
            return img;
        }

        void Disp(string commentaires)
        {
            messages.Add(commentaires);
        }

        bool QRCode_Check(string TXT, Bitmap img, out string message, string param = "")
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
                if (val < 0.001)
                {
                    bool dejatester = false;
                    foreach (Test item in tests)
                    {
                        if (item.facteur == 0.001)
                        {
                            dejatester = true;
                            break;
                        }
                    }
                    val = dejatester ? -1 : 0.001;
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
        #endregion
    }
}
