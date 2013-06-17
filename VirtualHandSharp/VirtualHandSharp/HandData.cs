/*
 * Copyright (C) 2013 Rovaniemi University of Applied Sciences (Rovaniemen Ammattikorkeakoulu)
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of 
 * this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation the rights 
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of 
 * the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all 
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
 * OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VirtualHandSharp
{
    /// <summary>
    /// A wrapper for a hand's joint data. Joints can be accessed with the [] operator, or through finger objects.
    /// </summary>
    public class HandData
    {
        #region Static
        /// <summary>
        /// Indexes for the array of joints, as defined by the user manual, page 20.
        /// </summary>
        public enum Joint : int
        {
            /// <summary>
            /// Sensor number 0 as defined by the user manual (page 20).
            /// </summary>
            THUMB_INNER = 0,
            /// <summary>
            /// Sensor number 1 as defined by the user manual (page 20).
            /// </summary>
            THUMB_MIDDLE = 1,
            /// <summary>
            /// Sensor number 2 as defined by the user manual (page 20).
            /// </summary>
            THUMB_OUTER = 2,
            /// <summary>
            /// Sensor number 3 as defined by the user manual (page 20).
            /// </summary>
            THUMB_ABD = 3,
            /// <summary>
            /// Sensor number 4 as defined by the user manual (page 20).
            /// </summary>
            INDEX_INNER = 4,
            /// <summary>
            /// Sensor number 5 as defined by the user manual (page 20).
            /// </summary>
            INDEX_MIDDLE = 5,
            /// <summary>
            /// Sensor number 6 as defined by the user manual (page 20).
            /// </summary>
            INDEX_OUTER = 6,
            /// <summary>
            /// Sensor number 7 as defined by the user manual (page 20).
            /// </summary>
            MIDDLE_INNER = 7,
            /// <summary>
            /// Sensor number 9 as defined by the user manual (page 20).
            /// </summary>
            MIDDLE_MIDDLE = 8,
            /// <summary>
            /// Sensor number 10 as defined by the user manual (page 20).
            /// </summary>
            MIDDLE_OUTER = 9,
            /// <summary>
            /// Sensor number 11 as defined by the user manual (page 20).
            /// </summary>
            MIDDLE_INDEX_ABD = 10,
            /// <summary>
            /// Sensor number 12 as defined by the user manual (page 20).
            /// </summary>
            RING_INNER = 11,
            /// <summary>
            /// Sensor number 13 as defined by the user manual (page 20).
            /// </summary>
            RING_MIDDLE = 12,
            /// <summary>
            /// Sensor number 14 as defined by the user manual (page 20).
            /// </summary>
            RING_OUTER = 13,
            /// <summary>
            /// Sensor number 15 as defined by the user manual (page 20).
            /// </summary>
            RING_MIDDLE_ABD = 14,
            /// <summary>
            /// Sensor number 16 as defined by the user manual (page 20).
            /// </summary>
            PINKIE_INNER = 15,
            /// <summary>
            /// Sensor number 17 as defined by the user manual (page 20).
            /// </summary>
            PINKIE_MIDDLE = 16,
            /// <summary>
            /// Sensor number 18 as defined by the user manual (page 20).
            /// </summary>
            PINKIE_OUTER = 17,
            /// <summary>
            /// Sensor number 19 as defined by the user manual (page 20).
            /// </summary>
            PINKIE_RING_ABD = 18,
            /// <summary>
            /// Sensor number 20 as defined by the user manual (page 20).
            /// </summary>
            PALM_ARCH = 19,
            /// <summary>
            /// Sensor number 21 as defined by the user manual (page 20).
            /// </summary>
            WRIST_PITCH = 20,
            /// <summary>
            /// Sensor number 22 as defined by the user manual (page 20).
            /// </summary>
            WRIST_YAW = 21,
            /// <summary>
            /// Overflow.
            /// </summary>
            MAX = 22
        }
        /// <summary>
        /// The number of fingers per hand.
        /// </summary>
        public const int NR_FINGERS = 5;
        /// <summary>
        /// The number of joints per finger.
        /// </summary>
        public const int NR_JOINTS = 4;
        #endregion
        #region Properties
        /// <summary>
        /// The comma separated values string that represents this record. 
        /// Useful for writing to files.
        /// </summary>
        public virtual string CSV
        {
            get
            {
                string rv = "";
                for (int i = 0; i < 22; i++)
                {
                    rv += " : ";
                    rv += this[i].Radians.ToString();
                }
                return rv.Substring(3);
            }
        }
        /// <summary>
        /// Gets some debug output. Specifically, it returns the fingers' names and whether they
        /// are respectively stretched or bent. It also returns said fingers' respective debug strings, which
        /// include the joints' data.
        /// </summary>
        public string DebugString
        {
            get
            {
                string rv = ""; // Return value
                rv += "Glove:\n";
                // Add thumb data
                rv += "Thumb:\n";
                rv += "    [" + thumb.DebugString + "];\n";
                // Add index data
                rv += "Index:\n";
                rv += "    [" + index.DebugString + "];\n";
                // Add middle finger data
                rv += "Middle:\n";
                rv += "    [" + middle.DebugString + "];\n";
                // Add ring finger data
                rv += "Ring:\n";
                rv += "    [" + ring.DebugString + "];\n";
                // Add pinkie data
                rv += "Pinkie:\n";
                rv += "    [" + pinkie.DebugString + "];\n";
                // Pitch and yaw:
                rv += "Wrist:\n";
                rv += "    [" + WristPitch + ", " + WristYaw + ", " + PalmArch + "]\n\n";
                return rv;
            }
        }
        /// <summary>
        /// Gets the angle specified by an index (using the Hand.Joint enum) as specified
        /// by page 20 of the user manual. DO NOT USE THIS UNLESS FOR DEBUGGING. JOINT INDEXES
        /// MAY CHANGE AND BREAK YOUR DATA.
        /// </summary>
        /// <param name="i">The index.</param>
        /// <returns>The requested angle.</returns>
        public Angle this[int i]
        {
            get
            {
                return this[(Joint)i];
            }
        }
        /// <summary>
        /// Gets the angle specified by an index (using the Hand.Joint enum) as specified
        /// by page 20 of the user manual.
        /// </summary>
        /// <param name="i">The index.</param>
        /// <returns>The requested angle.</returns>
        public Angle this[Joint i]
        {
            get
            {
                switch (i)
                {
                    case Joint.THUMB_INNER:
                        return thumb.Inner;
                    case Joint.THUMB_MIDDLE:
                        return thumb.Middle;
                    case Joint.THUMB_OUTER:
                        return thumb.Outer;
                    case Joint.THUMB_ABD:
                        return thumb.NextAbduction;
                    case Joint.INDEX_INNER:
                        return index.Inner;
                    case Joint.INDEX_MIDDLE:
                        return index.Middle;
                    case Joint.INDEX_OUTER:
                        return index.Outer;
                    case Joint.MIDDLE_INNER:
                        return middle.Inner;
                    case Joint.MIDDLE_MIDDLE:
                        return middle.Middle;
                    case Joint.MIDDLE_OUTER:
                        return middle.Outer;
                    case Joint.MIDDLE_INDEX_ABD:
                        return index.NextAbduction;
                    case Joint.RING_INNER:
                        return ring.Inner;
                    case Joint.RING_MIDDLE:
                        return ring.Middle;
                    case Joint.RING_OUTER:
                        return ring.Outer;
                    case Joint.RING_MIDDLE_ABD:
                        return middle.NextAbduction;
                    case Joint.PINKIE_INNER:
                        return pinkie.Inner;
                    case Joint.PINKIE_MIDDLE:
                        return pinkie.Middle;
                    case Joint.PINKIE_OUTER:
                        return pinkie.Outer;
                    case Joint.PINKIE_RING_ABD:
                        return ring.NextAbduction;
                    case Joint.PALM_ARCH:
                        return palmArch;
                    case Joint.WRIST_PITCH:
                        return wristPitch;
                    case Joint.WRIST_YAW:
                        return wristYaw;
                    default:
                        throw new IndexOutOfRangeException("Index needs to be less than 22");
                }
            }
        }
        #endregion
        #region Members
        /// <summary>
        /// The Finger object representing the thumb.
        /// </summary>
        protected Finger thumb = null;
        /// <summary>
        /// The Finger object representing the index finger.
        /// </summary>
        protected Finger index = null;
        /// <summary>
        /// The Finger object representing the middle finger.
        /// </summary>
        protected Finger middle = null;
        /// <summary>
        /// The Finger object representing the ring finger.
        /// </summary>
        protected Finger ring = null;
        /// <summary>
        /// The Finger object representing the pinkie finger.
        /// </summary>
        protected Finger pinkie = null;
        /// <summary>
        /// An array of the fingers; useful for loops that need to access all fingers.
        /// It is indexed from thumb to pinkie, meaning that the thumb is at the lowest index,
        /// and the pinkie is the highest index.
        /// </summary>
        public Finger[] Fingers { get; protected set; }
        /// <summary>
        /// Gets or sets the Wrist's yaw.
        /// </summary>
        public double WristYaw
        {
            get { return wristYaw.Value; }
        }
        /// <summary>
        /// Gets or sets the Wrist's pitch.
        /// </summary>
        public double WristPitch
        {
            get { return wristPitch.Value; }
        }
        /// <summary>
        /// Gets or sets the Palm Arm. (Causes the pinkie to rotate across palm.
        /// </summary>
        public double PalmArch
        {
            get { return palmArch.Value; }
        }
        /// <summary>
        /// The wrist yaw's angle object.
        /// </summary>
        protected Angle wristYaw;
        /// <summary>
        /// The wrist pitch's angle object.
        /// </summary>
        protected Angle wristPitch;
        /// <summary>
        /// The palm arch's angle object.
        /// </summary>
        protected Angle palmArch;
        #endregion
        #region Public Functions
        /// <summary>
        /// Adds another HandData's values to this one's. 
        /// </summary>
        /// <param name="adder">The object whose angles need to be added to this object's.</param>
        public void Add(HandData adder)
        {
            for (int i = 0; i < 22; i++)
            {
                this[i].Value += adder[i].Value;
            }
        }
        /// <summary>
        /// Adds values to the already existing values.
        /// </summary>
        /// <param name="adder">The values to be added.</param>
        public void Add(double[] adder)
        {
            if (adder.Length != 22)
            {
                throw new ArgumentException("The length of the array has to be 22.", "double[] other");
            }

            for (int i = 0; i < 22; i++)
            {
                this[i].Value += adder[i];
            } 
        }
        /// <summary>
        /// Default constructor.
        /// </summary>
        public HandData()
        {
            // Create finger objects.
            thumb = new Finger(0);
            index = new Finger(1);
            middle = new Finger(2);
            ring = new Finger(3);
            pinkie = new Finger(4);
            wristYaw = new Angle();
            wristPitch = new Angle();
            palmArch = new Angle();
            // Fill the fingers array with the newly created fingers.
            Fingers = new Finger[NR_FINGERS] { thumb, index, middle, ring, pinkie };
        }
        /// <summary>
        /// Clones a HandData's data into this one.
        /// </summary>
        /// <param name="data">The data that should be cloned.</param>
        public void Populate(HandData data)
        {
            int max = NR_FINGERS * NR_JOINTS + 2;
            // Copy it all over.
            for (int i = 0; i < max; i++)
            {
                this[i].Value = data[i].Radians;
            }
        }
        /// <summary>
        /// Populates the data from a CSV line. 
        /// </summary>
        /// <param name="csv">The doubles, separated by a symbol.</param>
        /// <param name="sep">The separator. : by default.</param>
        public virtual void Populate(string csv, char sep = ':' )
        {
            int max = NR_FINGERS * NR_JOINTS + 2;
            // Split the CSV.
            string[] tokens = csv.Split(sep);
            // A csv with not exactly 22 values would be invalid.
            if (tokens.Length != max)
                throw new ArgumentException("The csv did not contain exactly 22 values.");
            // Populate the parsed data.
            for (int i = 0; i < max; i++)
                this[i].Value = Double.Parse(tokens[i].Trim());
        }
        #endregion
    }
}
