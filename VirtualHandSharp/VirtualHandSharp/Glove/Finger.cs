using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualHandSharp.Glove
{
    /// <summary>
    /// Represents a finger, and allows for CyberGlove's joint data to be 
    /// saved in doubles (Inner, Middle and Outer) that are read-only.
    /// </summary>
    /// <author>Arno Sluismans</author>
    public class Finger
    {    

        /// <summary>
        /// The joint closest to the palm.
        /// 
        /// Close to 0 means that the joint is stretched.
        /// Negative means that the joint is bent towards the palm.
        /// Positive means that the joint is bent backwards (barely possible).
        /// </summary>
        public double Inner 
        {
            get { return inner.Value; }
        }
        private Angle inner;
        /// <summary>
        /// The middle joint.
        /// 
        /// Close to 0 means that the joint is stretched.
        /// Negative means that the joint is bent towards the palm.
        /// Positive means that the joint is bent backwards (barely possible).
        /// </summary>
        public double Middle 
        { 
            get { return middle.Value; }
        }
        private Angle middle;
        /// <summary>
        /// The joint closest to the fingertip.
        /// 
        /// Close to 0 means that the joint is stretched.
        /// Negative means that the joint is bent towards the palm.
        /// Positive means that the joint is bent backwards (barely possible).
        /// </summary>
        public double Outer
        {
            get { return outer.Value; }
        }
        private Angle outer;
        /// <summary>
        /// Gets or sets the array of joints. Inner indexed.
        /// </summary>
        public double[] Joints { get; private set; }
        /// <summary>
        /// Gets or sets the abduction between this finger and the one previous to it.
        /// (i.e.: closer to the thumb)
        /// </summary>
        public Angle PreviousAbduction { get; set; }
        /// <summary>
        /// Gets or sets the abduction between this finger and the one next to it.
        /// (i.e.: away from the thumb)
        /// </summary>
        public Angle NextAbduction { get; set; }

        /// <summary>
        /// Gets the sum of all joints' angles. This can be useful to see how far the finger
        /// has been curled.
        /// </summary>
        /// <author>Arno Sluismans</author>
        private double CurlFactor
        {
            get
            {
                return inner.Radians + middle.Radians + outer.Radians;
            }
        }
        /// <summary>
        /// Returns a debug string, containing the joints' data.
        /// </summary>
        /// <author>Arno Sluismans</author>
        public string DebugString
        {
            get
            {
                return Inner + ", " + Middle + ", " + Outer + 
                    (NextAbduction == null ? "" : (", " + NextAbduction.Value));
            }
        }
        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="id">The index of the finger. Thumb is 0, index is 1, etc.</param>
        /// <author>Arno Sluismans</author>
        public Finger(int id)
        {
            if (id > 0)
            {
                PreviousAbduction = new Angle();
            }
            if (id < Hand.NR_FINGERS - 1)
            {
                NextAbduction = new Angle();
            }
            inner = new Angle();
            middle = new Angle();
            outer = new Angle();
            Joints = new double[] {Inner, Middle, Outer};
        }


        /// <summary>
        /// Calculates and returns whether or not the finger is stretched. 
        /// </summary>
        /// <author>Arno Sluismans</author>
        public bool IsStretched
        {
            get
            {
                return (-1.0 < CurlFactor &&
                               CurlFactor < 1.0);
            }
        }
        /// <summary>
        /// Calculates and returns whether or not the finger is bent.
        /// </summary>
        /// <author>Arno Sluismans</author>
        public bool IsBent
        {
            get { return !IsStretched; }
        }
        /// <summary>
        /// Calculates and returns whether or not the finger is curled.
        /// (NOT IMPLEMENTED YET.)
        /// </summary>
        /// <author>Arno Sluismans</author>
        public bool IsCurled
        {
            get { return false; }
        }

        /// <summary>
        /// Populates the joints' data with the given data. 
        /// </summary>
        /// <param name="inner">The inner joint's angle data.</param>
        /// <param name="middle">The middle joint's angle data.</param>
        /// <param name="outer">The outer joint's angle data.</param>
        /// <author>Arno Sluismans</author>
        public void setJoints(double inner, double middle, double outer)
        {
            // Set the inner joint
            this.inner.Value = inner;
            // Set the middle joint
            this.middle.Value = middle;
            // Set the outer joint
            this.outer.Value = outer;
        }
    }

}
