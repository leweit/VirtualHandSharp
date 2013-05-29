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
using VirtualHandSharp;
using VirtualHandSharp.Position;
using VirtualHandSharp.Motion;
using System.IO;
namespace VirtualHandSharp.Emulator
{
    /// <summary>
    /// This class can emulate a CyberGlove by replaying a record created by 
    /// Emulator.Recorder. 
    /// </summary>
    public class HandEmulator : Hand
    {
        #region Members
        /// <summary>
        /// The emulator's sequence of handdata.
        /// </summary>
        private List<HandData> sequence;
        /// <summary>
        /// The current position of playback.
        /// </summary>
        private int currentPos;
        #endregion
        #region Properties
        /// <summary>
        /// The path to the input file which contains the data sequence.
        /// </summary>
        public string InputPath { get; private set; }
        /// <summary>
        /// Whether the hand should continue from the start after reaching the last position.
        /// If false, it will stop polling once it has reached the end.
        /// </summary>
        public bool Recur;
        #endregion
        #region Public Functions
        /// <summary>
        /// Creates a HandEmulator. The input file will also be read immediately.
        /// </summary>
        /// <param name="path">The path to the input file.</param>
        /// <param name="recur">Whether the emulation should repeat itself after it has ended.</param>
        public HandEmulator(string path, bool recur = true)
            : base(false)
        {
            InputPath = path;
            Recur = recur;
            currentPos = 0;
            sequence = new List<HandData>();
            readSequence();
        }
        #endregion
        #region Protected Functions
        /// <summary>
        /// Polls for updated data. Because this is an emulator, the next line in the
        /// record will be used.
        /// </summary>
        protected override void pollJointData()
        {
            // Clone the current item into this hand, and increase the index
            Populate(sequence[currentPos++]);
            // Update the position.
            PositionRecord pr = PositionParser.GetMatch(this);
            // Huge ugly if test...
            // Call the event if this position is recognized, and it's not the same
            // as the last recognized function.
            if (pr != null && pr != CurrentPosition && pr != LastPosition)
            {
                LastPosition = pr;
                PositionHasChanged(this, pr);
                // Check whether any motions have been detected...
                // There can be multiple matches.
                List<MotionRecord> matches = MotionParser.GetMatches(pr);
                foreach (MotionRecord mr in matches)
                {
                    MotionHasDetected(this, mr);
                }
            }
            CurrentPosition = pr;
            DataHasUpdated(this);
            // Check whether we need to stop polling now.
            checkIfEnded();
        }
        #endregion
        #region Private Functions
        /// <summary>
        /// Reads the input file specified by InputPath.
        /// </summary>
        private void readSequence()
        {
            if (!File.Exists(InputPath)) return;
            StreamReader reader = new StreamReader(InputPath);
            string line;
            HandData item;
            while (reader.Peek() != -1)
            {
                line = reader.ReadLine();
                item = new HandData();
                item.Populate(line);
                sequence.Add(item);
            }
            reader.Close();
            reader.Dispose();
        }
        /// <summary>
        /// Checks whether the end of the record has been reached. It then resets the 
        /// position or stops polling, depending on the Recur property.
        /// </summary>
        private void checkIfEnded()
        {
            if (currentPos >= sequence.Count)
            {
                if (!Recur)
                    StopPolling();
                else
                    currentPos = 0;
            }
        }
        #endregion
    }
}
