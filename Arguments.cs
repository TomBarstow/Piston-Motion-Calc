using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace PistonMotion
{
    public class Arguments
    {
        public string Filename { get; set; }
        public double Stroke { get; set; }
        public double Bore { get; set; }
        public double RodLength { get; set; }
        public double DeckHeight { get; set; }
        public double CompHeight { get; set; }
        public double GasketHeight { get; set; }
        public int RPM { get; set; }
    }
}
