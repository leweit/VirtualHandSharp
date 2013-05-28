﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualHandSharp.Position
{
    /// <summary>
    /// This class allows for a hand's momentary position to be recorded and edited.
    /// </summary>
    public class PositionRecord : HandData
    {	
        #region Static
        /// <summary>
        /// An array that tells how precise each joint should be in calculation of 
        /// matches.
        /// </summary>
        protected static double[] precision;
        /// <summary>
        /// Static constructor.
        /// </summary>
        static PositionRecord()
        {
            precision = new double[] {
                0.4, // 0
                0.3, // 1
                0.4, // 2
                0.15, // 3
                0.5, // 4
                0.5, // 5
                0.5, // 6
                0.5, // 7
                0.5, // 8
                0.5, // 9
                0.0005, // 10, This sensor's angle calculation is so seriously broken, but at least we can still use it for relative comparisons.
                0.5, // 11
                0.5, // 12
                0.5, // 13
                0.15, // 14
                0.5, // 15
                0.5, // 16
                0.5, // 17
                0.15, // 18
                0.5, // 19
                0.5, // 20
                0.3, // 21
            };
        }
	    #endregion
        #region Events

        #endregion
        #region Members
        /// <summary>
        /// The name of the record.
        /// </summary>
        private string name;
	    #endregion
	    #region Properties
        /// <summary>
        /// A list of booleans that tells whether certain values should be ignored.
        /// For instance, if Ignored[1] is true, the sensor at index 1 will not be checked
        /// in the calculation of matches.
        /// </summary>
        public bool[] Ignored = new bool[22];
        /// <summary>
        /// The name of the record. Will always be upper case.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value.ToUpper(); }
        }
        /// <summary>
        /// The comma separated values string that represents this record. 
        /// Useful for writing to files.
        /// </summary>
        public override string CSV
        {
            get
            {
                string rv = "";
                for (int i = 0; i < Ignored.Length; i++)
                {
                    rv += " : ";
                    rv += Ignored[i] ? "*" : this[i].Radians.ToString();
                }
                return rv.Substring(3);
            }
        }
        /// <summary>
        /// If this is true, this position is an actual position of itself,
        /// and not just part of a motion sequence. If it's false, it means that
        /// no special reports should be made of this position's occurrence.
        /// </summary>
        public bool Standalone { get; set; }
	    #endregion
	    #region Public Functions
        /// <summary>
        /// Constructs a record with a name.
        /// </summary>
        /// <param name="name">The name, which will be saved in upper case.</param>
        public PositionRecord(string name)
        {
            if (name != null)
                Name = name;
            // Give angles the normal precision.
            for (int i = 0; i < precision.Length; i++)
            {
                this[i].MaxDiff = precision[i];
            }
            Standalone = true;
        }
        /// <summary>
        /// Tells whether a hand's current position is similar to this one.
        /// </summary>
        /// <param name="other">The other hand.</param>
        /// <returns>Whether the other hand is similar to this one.</returns>
        public bool IsSimilar(HandData other)
        {
            return IsSimilar(other, 1.0);
        }
        /// <summary>
        /// Tells whether a hand's current position is similar to this one, with
        /// the precision altered by the given argument.
        /// Not to confused with IsSimilar(PositionRecord, double) which is a more 
        /// specific implementation.
        /// </summary>
        /// <param name="other">The other hand.</param>
        /// <param name="precision">The factor by which the default precision should be multiplied.</param>
        /// <returns>Whether the other hand is similar to this one.</returns>
        public bool IsSimilar(HandData other, double precision)
        {
            int max = NR_FINGERS * NR_JOINTS + 2;
            for (int i = 0; i < max; i++)
            {
                if (Ignored[i])
                    continue;
                else if (!this[i].IsSimilar(other[i], precision))
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Tells whether a hand's current position is similar to this one, with
        /// the precision altered by the given argument.
        /// Not to confused with IsSimilar(HandData, double) which is a less 
        /// specific implementation.
        /// </summary>
        /// <param name="other">The other handposition.</param>
        /// <param name="precision">The factor by which the default precision should be multiplied.</param>
        /// <returns>Whether the other hand is similar to this one.</returns>
        public bool IsSimilar(PositionRecord other, double precision)
        {
            int max = NR_FINGERS * NR_JOINTS + 2;
            for (int i = 0; i < max; i++)
            {
                if (Ignored[i] || other.Ignored[i])
                    continue;
                else if (!this[i].IsSimilar(other[i], precision))
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Tells the record that all wrist properties can be ignored.
        /// </summary>
        public void IgnoreWrist()
        {
            Ignored[21] = Ignored[20] = Ignored[19] = true;
        }
        /// <summary>
        /// Tells the record that all abductions can be ignored.
        /// </summary>
        public void IgnoreAbductions()
        {
            Ignored[3] = Ignored[10] = Ignored[14] = Ignored[18] = true;
        }
        /// <summary>
        /// Tells the record that all Thumb properties can be ignored.
        /// </summary>
        public void IgnoreThumb()
        {
            Ignored[0] = Ignored[1] = Ignored[2] = Ignored[3] = true;
        }

        /// <summary>
        /// Copies the joint values over from a given hand to this one.
        /// </summary>
        /// <param name="data">The hand we need to copy.</param>
        public void CloneData(HandData data)
        {
            for (int i = 0; i < NR_FINGERS * NR_JOINTS + 2; i++)
            {
                this[i].Value = data[i].Radians;
            }
        }
	    #endregion
    }
}