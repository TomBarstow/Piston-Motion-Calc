using System;

namespace PistonMotion
{
    /// <summary>
    /// Represents a cam profile with symmetric lift characteristics
    /// </summary>
    public class CamProfile
    {
        // Cam specifications
        public double MaxLift { get; set; }
        public double DurationAtLift { get; set; }  // Duration at specified lift (e.g., duration @ 0.050")
        public double CheckingLift { get; set; }    // The lift height used for duration measurement (e.g., 0.050")
        public double LobeCenterline { get; set; }  // Degrees ATDC when cam lobe is at maximum lift (converted for exhaust)
        public double RockerRatio { get; set; } = 1.0;  // Rocker arm ratio (1.0 = direct actuation)

        // Valve specifications
        public double ValveDiameter { get; set; }
        public double ValveAngle { get; set; } = 45.0;  // Valve seat angle in degrees
        public double PocketDepth { get; set; }         // Depth of valve relief in piston

        // Calculated properties
        public double TotalDuration { get; private set; }
        public double OpeningPoint { get; private set; }   // Crank angle when valve starts opening
        public double ClosingPoint { get; private set; }   // Crank angle when valve closes

        /// <summary>
        /// Initializes the cam profile and calculates timing events
        /// </summary>
        public void Initialize()
        {
            CalculateTimingEvents();
        }

        /// <summary>
        /// Calculates the total duration and opening/closing points based on the duration at checking lift
        /// </summary>
        private void CalculateTimingEvents()
        {
            // For a symmetric profile, we need to extrapolate from the checking lift to seat-to-seat duration
            // This uses a simplified model - real cams have ramp rates that vary

            // Assume the ramp from seat to checking lift takes about 20-30% of the total duration
            // This is a reasonable approximation for most performance cams
            double rampFactor = 1.4; // Multiplier to go from checking lift duration to seat-to-seat

            TotalDuration = DurationAtLift * rampFactor;

            // Calculate opening and closing points (symmetric around lobe centerline)
            double halfDuration = TotalDuration / 2.0;
            OpeningPoint = LobeCenterline - halfDuration;
            ClosingPoint = LobeCenterline + halfDuration;

            // Normalize angles to 0-720 degree range (full 4-stroke cycle)
            OpeningPoint = NormalizeAngle720(OpeningPoint);
            ClosingPoint = NormalizeAngle720(ClosingPoint);
        }

        /// <summary>
        /// Calculates the cam lift at a given crank angle using a symmetric polynomial profile
        /// </summary>
        /// <param name="crankAngle">Crank angle in degrees ATDC</param>
        /// <returns>Cam lift at the specified angle</returns>
        public double GetCamLiftAtAngle(double crankAngle)
        {
            crankAngle = NormalizeAngle720(crankAngle);

            // Check if valve is closed (outside of duration)
            if (!IsValveOpen(crankAngle))
                return 0.0;

            // Calculate position relative to lobe centerline (-1 to +1)
            double halfDuration = TotalDuration / 2.0;
            double angleFromCenter = crankAngle - LobeCenterline;

            // Handle wrap-around for angles near 0/720
            if (angleFromCenter > 360) angleFromCenter -= 720;
            if (angleFromCenter < -360) angleFromCenter += 720;

            double normalizedPosition = angleFromCenter / halfDuration;

            // Clamp to valid range
            if (Math.Abs(normalizedPosition) > 1.0) return 0.0;

            // Symmetric polynomial lift curve (4th order for smooth acceleration)
            // This creates a smooth, symmetric profile with good acceleration characteristics
            double x = Math.Abs(normalizedPosition);
            // Using a cosine-based curve for more realistic cam profile
            double liftRatio = (1 + Math.Cos(x * Math.PI)) / 2.0;

            return Math.Max(0, MaxLift * liftRatio);
        }

        /// <summary>
        /// Gets the actual valve lift accounting for rocker ratio
        /// </summary>
        /// <param name="crankAngle">Crank angle in degrees ATDC</param>
        /// <returns>Actual valve lift</returns>
        public double GetValveLiftAtAngle(double crankAngle)
        {
            return GetCamLiftAtAngle(crankAngle) * RockerRatio;
        }

        /// <summary>
        /// Checks if the valve is open at the given crank angle
        /// </summary>
        /// <param name="crankAngle">Crank angle in degrees ATDC</param>
        /// <returns>True if valve is open</returns>
        public bool IsValveOpen(double crankAngle)
        {
            crankAngle = NormalizeAngle720(crankAngle);
            double normalizedOpening = NormalizeAngle720(OpeningPoint);
            double normalizedClosing = NormalizeAngle720(ClosingPoint);

            // Handle cases where duration spans across 0/720 degrees
            if (normalizedOpening > normalizedClosing)
            {
                // Duration spans across 0 degrees (e.g., opens at 680°, closes at 40°)
                return crankAngle >= normalizedOpening || crankAngle <= normalizedClosing;
            }
            else
            {
                // Normal case
                return crankAngle >= normalizedOpening && crankAngle <= normalizedClosing;
            }
        }

        /// <summary>
        /// Normalizes an angle to 0-720 degree range for 4-stroke cycle
        /// </summary>
        private double NormalizeAngle720(double angle)
        {
            while (angle < 0) angle += 720;
            while (angle >= 720) angle -= 720;
            return angle;
        }

        /// <summary>
        /// Returns a string representation of the cam profile
        /// </summary>
        public override string ToString()
        {
            return $"Lift: {MaxLift:F3}, Duration: {DurationAtLift:F0}° @ {CheckingLift:F3}, Centerline: {LobeCenterline:F1}°, Rocker: {RockerRatio:F2}";
        }
    }

    /// <summary>
    /// Contains both intake and exhaust cam profiles for an engine
    /// </summary>
    public class CamSpecification
    {
        public CamProfile IntakeCam { get; set; } = new CamProfile();
        public CamProfile ExhaustCam { get; set; } = new CamProfile();

        /// <summary>
        /// Initializes both cam profiles with default values if not already set
        /// </summary>
        public void Initialize()
        {
            IntakeCam.Initialize();
            ExhaustCam.Initialize();
        }

        /// <summary>
        /// Sets the exhaust cam centerline from BTDC measurement to ATDC for calculations
        /// </summary>
        /// <param name="centerlineBTDC">Exhaust centerline in degrees BTDC</param>
        public void SetExhaustCenterlineFromBTDC(double centerlineBTDC)
        {
            // Convert BTDC to ATDC for 4-stroke cycle
            // Exhaust stroke occurs 360-720° ATDC, so BTDC on exhaust stroke = 360 - BTDC
            ExhaustCam.LobeCenterline = 360 - centerlineBTDC;
        }

        /// <summary>
        /// Gets the combined valve lift information at a given crank angle
        /// </summary>
        /// <param name="crankAngle">Crank angle in degrees ATDC (0-720 for full 4-stroke cycle)</param>
        /// <returns>Tuple containing (intake lift, exhaust lift)</returns>
        public (double IntakeLift, double ExhaustLift) GetValveLiftAtAngle(double crankAngle)
        {
            return (IntakeCam.GetValveLiftAtAngle(crankAngle),
                    ExhaustCam.GetValveLiftAtAngle(crankAngle));
        }

        /// <summary>
        /// Gets summary information about the cam specification
        /// </summary>
        public override string ToString()
        {
            return $"Cam Specification:\n" +
                   $"  Intake:  {IntakeCam}\n" +
                   $"  Exhaust: {ExhaustCam}";
        }
    }
}