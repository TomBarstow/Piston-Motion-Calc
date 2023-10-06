namespace PistonMotion
{
    public class Arguments
    {
        public string FileLocation { get; set; }
        public string Filename { get; set; }
        public bool IsMetric { get; set; }
        public double Stroke { get; set; }
        public double Bore { get; set; }
        public double RodLength { get; set; }
        public double DeckHeight { get; set; }
        public double CompHeight { get; set; }
        public double PistonVolume { get; set; }
        public double ChamberVolume { get; set; }
        public double GasketHeight { get; set; }
        public int RPM { get; set; }
        public int CylinderCount { get; set; }

    }
}
