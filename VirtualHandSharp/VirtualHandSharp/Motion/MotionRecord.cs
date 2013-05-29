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
using VirtualHandSharp.Position;

namespace VirtualHandSharp.Motion
{
    /// <summary>
    /// A record of movement, which keeps a sequence of 
    /// </summary>
    public class MotionRecord
    {
        private List<SequenceItem> sequence;

        /// <summary>
        /// The name of the record. Will always be upper case.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value.ToUpper(); }
        }
        /// <summary>
        /// The name of the record.
        /// </summary>
        private string name;
        /// <summary>
        /// Gets the current item (one after the last one that was matched).
        /// </summary>
        public SequenceItem Current
        {
            get { return (current < 0) ? null : sequence[current]; }
        }
        /// <summary>
        /// Gets the next item.
        /// </summary>
        public SequenceItem Next
        {
            get { return sequence[current + 1]; }
        }
        /// <summary>
        /// The index for the current item.
        /// </summary>
        private int current;

        /// <summary>
        /// Creates a default MotionRecord.
        /// </summary>
        /// <param name="name">The name of this record.</param>
        public MotionRecord(string name)
        {
            Name = name;
            sequence = new List<SequenceItem>();
            current = -1;
        }
        /// <summary>
        /// Appends a positionrecord to the sequence.
        /// </summary>
        /// <param name="data">The record that should be added.</param>
        public void Add(SequenceItem data)
        {
            sequence.Add(data);
            // Hook into the position's Changed event.
            // data.PositionChanged += this.HandChanged;
        }

        /// <summary>
        /// Initializes the record's modifiers, for each sequenceitem.
        /// </summary>
        public void InitModifiers()
        {
            for (int i = 0; i < sequence.Count; i++)
            {
                if (sequence[i].HasLeniencyModifier)
                    sequence[i].InitLeniency();

                if (sequence[i].HasTransitionModifier)
                {
                    if (i == sequence.Count - 1)
                        throw new MalformedException("The last item of a sequence cannot have the transition modifier (~).");

                    sequence[i].InitTransition(sequence[i + 1].Position);
                }
            }
        }
        /// <summary>
        /// Checks whether the positionrecord completes this sequence.
        /// </summary>
        /// <param name="current">The current position.</param>
        /// <returns>Whether this motion has been matched.</returns>
        public bool CheckMatched(PositionRecord current)
        {
            if (current == null)
                return false;

            if (Next == current)
            {
                this.current++;
                return checkEnd();
            }
            else if (this.current == -1)
            {
                return false;
            }
            else if (Current.CancelsSequence(current))
            {
                this.current = -1;
                return false;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Checks whether we have reached the end of the sequence.
        /// </summary>
        /// <returns>Whether the end of the sequence has been reached.</returns>
        private bool checkEnd()
        {
            if (current == sequence.Count - 1)
            {
                current = -1;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
