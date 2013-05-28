using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualHandSharp
{
    /// <summary>
    /// A class that serves as a wrapper for angle values. Input should always be
    /// in radians. 
    /// </summary>
    public class Angle
    {
        #region Static
        /// <summary>
        /// An enum that can be used to determine what unit of angle measurements is being used.
        /// </summary>
        /// <example>
        /// Angle.UseAngleUnit = Angle.AngleUnit.DEGREES;
        /// </example>
        public enum AngleUnit
        {
            /// <summary>
            /// Tells Angle to return data in degrees.
            /// </summary>
            DEGREES,
            /// <summary>
            /// Tells Angle to return data in radians.
            /// </summary>
            RADIANS
        }
        /// <summary>
        /// The maximum allowed difference between two angles in order for them 
        /// to be considered similar, in the IsSimilar() function.
        /// </summary>
        public static double MAX_DIFF { get; set; }
        /// <summary>
        /// The AngleUnit that should be used in values returned by properties. Note that 
        /// they still always need to be SET using radians.
        /// </summary>
        public static AngleUnit UseAngleUnit { get; set; }
        /// <summary>
        /// Static constructor.
        /// </summary>
        static Angle()
        {
            UseAngleUnit = AngleUnit.RADIANS;
            MAX_DIFF = 0.5;
        }
        #endregion
        #region Members
        /// <summary>
        /// The value of the angle. This always needs to be in radians.
        /// </summary>
        private double v;
        #endregion
        #region Properties
        /// <summary>
        /// The value of the angle.
        /// </summary>
        public double Value
        {
            get
            {
                if (UseAngleUnit == AngleUnit.RADIANS)
                    return v;
                else
                    return radianToDegree(v);
            }
            set
            {
                v = value;
            }
        }
        /// <summary>
        /// Gets the value of the angle in degrees.
        /// </summary>
        public double Degrees
        {
            get
            {
                return radianToDegree(v);
            }
        }
        /// <summary>
        /// Gets the value of the angle in radians.
        /// </summary>
        public double Radians
        {
            get
            {
                return v;
            }
        }
        /// <summary>
        /// Gets or sets the maximum allowed difference for angles that 
        /// get compared to this one.
        /// </summary>
        public double MaxDiff
        {
            get;
            set;
        }
        #endregion
        #region Public Functions
        /// <summary>
        /// Creates a default Angle.
        /// </summary>
        public Angle()
            : this(MAX_DIFF)
        {
        }
        /// <summary>
        /// Creates a custom Angle.
        /// </summary>
        /// <param name="maxDiff">The maximum allowed difference for 
        /// angles that get compared to this one.</param>
        public Angle(double maxDiff)
        {
            v = 0;
            MaxDiff = maxDiff;
        }
        /// <summary>
        /// Compares an angle to this one, and returns whether they are near each other.
        /// </summary>
        /// <param name="other">The angle to compare to this one.</param>
        public bool IsSimilar(Angle other)
        {
            return IsSimilar(other, 1.0);
        }

        /// <summary>
        /// Compares an angle to this one, and returns whether their difference is within
        /// the maximum allowed difference. (To see if they are near each other.)
        /// </summary>
        /// <param name="other">The other angle.</param>
        /// <param name="precision">The factor by which the default 
        /// precision should be increased .</param>
        /// <returns>Whether the two angles are near each other.</returns>
        public bool IsSimilar(Angle other, double precision)
        {
            double diff = other.Radians - this.v;
            // If |diff| is less than the maximum allowed difference, return true.
            return (-(precision * MaxDiff) <= diff && diff <= (precision * MaxDiff));
        }
        #endregion
        #region Private Functions
        /// <summary>
        /// Convert radian angle to degrees.
        /// </summary>
        /// <param name="angle">The angle in radians.</param>
        /// <returns>The angle in degrees.</returns>
        private double radianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }
        #endregion
    }
}
