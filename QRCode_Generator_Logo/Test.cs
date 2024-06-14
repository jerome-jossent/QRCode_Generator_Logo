using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QRCode
{
   public class Test
    {
        public double facteur = -1;
        public bool tested;
        public bool resultat;

        public Test(double val)
        {
            facteur = val;
            tested = false;
            resultat = false;
        }
    }
}
