﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PistonMotion
{
    public class PistonResult
    {
        public PistonResult(int angle, double pistonPosition, double pistonVelocity)
        {
            Angle = angle;
            PistonPosition = pistonPosition;
            PistonVelocity = pistonVelocity;
        }

        public int Angle { get; set; }
        public double PistonPosition { get; set; }
        public double PistonVelocity { get; set; }

        public override string ToString()
        {
            return $"{Angle},{PistonPosition},{PistonVelocity}";
        }
    }
}
