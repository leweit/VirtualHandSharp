using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualHandSharp.Position
{
    /// <summary>
    /// Matching using RMSD.
    /// </summary>
    public class RmsdMatching : MatchingStrategy
    {
        /// <summary>
        /// If this one is higher, it will be easier to match stuff.
        /// </summary>
        public static double TOLLERANCE { get; set; }
        /// <summary>
        /// Static constructor.
        /// </summary>
        static RmsdMatching()
        {
            TOLLERANCE = 22.5;
        }
        /// <summary>
        /// Compares two handdatas and tells whether they match.
        /// </summary>
        /// <param name="position">The position that should be matched.</param>
        /// <param name="current">The current data.</param>
        /// <param name="tollerance">How much each joint is allowed to deviate.</param>
        /// <returns>Whether the data matches.</returns>
        public bool AreSimilar(PositionRecord position, HandData current, double tollerance = 0)
        {
            tollerance = Math.Pow(tollerance > 0 ? tollerance : TOLLERANCE, 2);
            // Number of joints.
            int max = (int)HandData.Joint.MAX;
            // The total deviation.
            double total = 0, frac;
            // The number of non-ignored joints.
            int nr = 0;
            for (int i = 0; i < max; i++)
            {
                if (position.Ignored[i]) 
                    continue;
                // Add square deviation of this joint to the total.
                frac = position[i].Degrees - current[i].Degrees;
                total += frac * frac;
                nr++;
            }

            // Check whether this one matches.
            if (total / nr < tollerance)
                return true;
            else
                return false;
        }
    }
}
