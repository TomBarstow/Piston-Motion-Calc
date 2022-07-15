using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PistonMotion
{
    public class Arguments
    {
        public string Filename { get; set; }
        public double Stroke { get; set; }
        public double RodLength { get; set; }
        public int RPM { get; set; }
    }
}
