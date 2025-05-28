namespace PistonMotion
{
    /// <summary>
    /// Results to be written to CSV - now includes valve lift data
    /// </summary>
    public class CSVResults
    {
        public CSVResults(int angle, double pistonPosition, double pistonVelocity)
        {
            Angle = angle;
            PistonPosition = pistonPosition;
            PistonVelocity = pistonVelocity;
            IntakeValveLift = 0;
            ExhaustValveLift = 0;
        }

        public CSVResults(int angle, double pistonPosition, double pistonVelocity, double intakeValveLift, double exhaustValveLift)
        {
            Angle = angle;
            PistonPosition = pistonPosition;
            PistonVelocity = pistonVelocity;
            IntakeValveLift = intakeValveLift;
            ExhaustValveLift = exhaustValveLift;
        }

        public int Angle { get; set; }
        public double PistonPosition { get; set; }
        public double PistonVelocity { get; set; }
        public double IntakeValveLift { get; set; }
        public double ExhaustValveLift { get; set; }

        public override string ToString()
        {
            return $"{Angle},{PistonPosition},{PistonVelocity},{IntakeValveLift},{ExhaustValveLift}";
        }

        /// <summary>
        /// Returns a string representation without valve data (for backwards compatibility)
        /// </summary>
        public string ToStringBasic()
        {
            return $"{Angle},{PistonPosition},{PistonVelocity}";
        }
    }
}