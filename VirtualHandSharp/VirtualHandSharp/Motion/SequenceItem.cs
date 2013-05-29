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
using VirtualHandSharp;
namespace VirtualHandSharp.Motion
{
    /// <summary>
    /// Extends the PositionRecord class to also be able to tell that a position was 
    /// not matched, but is close enough to this record for it to be ignored. The sequence
    /// will not end when such a position is found.
    /// </summary>
    public class SequenceItem
    {
        /// <summary>
        /// Whether the transition modifier is enabled.
        /// </summary>
        public bool HasTransitionModifier { get; private set; }
        /// <summary>
        /// Whether the leniency modifier is enabled.
        /// </summary>
        public bool HasLeniencyModifier { get; private set; }
        /// <summary>
        /// The list of records that may appear in the transition from this item to the next.
        /// </summary>
        private HashSet<PositionRecord> transition;
        /// <summary>
        /// The list of records that are similar to this item's record.
        /// </summary>
        private HashSet<PositionRecord> leniency;
        /// <summary>
        /// Adds a position to the transition list. 
        /// </summary>
        /// <param name="position">The position that's part of the transition from
        /// this position to the next.</param>
        public void AddToTransition(PositionRecord position)
        {
            transition.Add(position);
        }
        /// <summary>
        /// The position that represents this SequenceItem.
        /// </summary>
        public PositionRecord Position { get; private set; }
        /// <summary>
        /// Whether a word is alphanumeric or not. Important for names.
        /// </summary>
        /// <param name="word">The input word.</param>
        /// <returns>Whether the parameter is alphanumeric or not.</returns>
        private static bool isAlphanumeric(string word)
        {
            for (int i = 0; i < word.Length; i++)
            {
                if (!Char.IsLetterOrDigit(word[i]))
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Creates a new SequenceItem from an input file token.
        /// </summary>
        /// <param name="token">The token, containing a positionrecord's name and some modifiers.</param>
        /// <returns>A SequenceItem created from this token.</returns>
        public static SequenceItem FromToken(string token)
        {
            // List of modifiers that specify details about how the item can be matched.
            List<char> modifiers = new List<char>();
            // Strip the end of the token from its modifiers, and 
            // store those modifiers in the list.
            while(isModifier(token[token.Length - 1]))
            {
                modifiers.Add(token[token.Length - 1]);
                token = token.Substring(0, token.Length - 1);
            }
            // If the name still contains any characters that are not alphanumeric,
            // throw an exception.
            if (!isAlphanumeric(token))
            {
                throw new MalformedException(string.Format("Name must be alphanumeric, and all modifiers should appear at the end of the name. This token is invalid: {0}", token));
            }
            SequenceItem rv = new SequenceItem(token);
            rv.Position = PositionParser.GetByName(token);
            rv.parseModifiers(modifiers);
            return rv;
        }
        /// <summary>
        /// Parses a list of modifiers.
        /// </summary>
        /// <param name="modifiers">The list of modifiers that need to be applied to this item.</param>
        private void parseModifiers(List<char> modifiers)
        {
            HasLeniencyModifier = false;
            HasTransitionModifier = false;

            foreach (char m in modifiers)
            {
                parseModifier(m);
            }
        }
        /// <summary>
        /// Parses a modifier.
        /// </summary>
        /// <param name="m">The modifier that needs to be applied to this item.</param>
        private void parseModifier(char m)
        {
            switch (m)
            {
                case '*':
                    HasLeniencyModifier = true;
                    break;
                case '~':
                    HasTransitionModifier = true;
                    break;
                default:
                    throw new MalformedException(string.Format("The modifier {0} is invalid", m));
            }
        }
        /// <summary>
        /// Configures the Leniency modifier for this item.
        /// </summary>
        public void InitLeniency()
        {
            if (!HasLeniencyModifier)
                throw new Exception("Trying to init leniency, but the SequenceItem is not specified to be lenient.");
            if (leniency == null)
                leniency = new HashSet<PositionRecord>();
            else
                leniency.Clear();

            List<PositionRecord> list = PositionParser.GetAllMatches(Position);
            foreach (PositionRecord pr in list)
            {
                leniency.Add(pr);
            }
        }
        /// <summary>
        /// Configures the Transition modifier for this item.
        /// </summary>
        /// <param name="next">The next position; required to have an end point for the transition.</param>
        public void InitTransition(HandData next)
        {
            if (!HasTransitionModifier)
                throw new Exception("Trying to init transition, but the SequenceItem is not specified to allow a transition.");
            if (transition == null)
                transition = new HashSet<PositionRecord>();
            else
                transition.Clear();

            double[] diffs = new double[22];
            double iterations = 10.0;
            HandData needle = new HandData();

            for (int i = 0; i < diffs.Length; i++)
            {
                diffs[i] = (next[i].Radians - Position[i].Radians) / iterations;
                needle[i].Value = Position[i].Radians;
            }

            for (int i = 0; i < iterations; i++)
            {
                // Prepare the needle by making its values approach two's some more.
                needle.Add(diffs);
                // Check if this matches a position.
                List<PositionRecord> list = PositionParser.GetAllMatches(needle, 1.5);
                foreach (PositionRecord pr in list)
                {
                    if (transition.Add(pr))
                        Console.WriteLine("Found a transitional item: {0}", pr.Name);
                }
            }
        }
        /// <summary>
        /// Tells whether a character
        /// </summary>
        /// <param name="modifier"></param>
        /// <returns></returns>
        private static bool isModifier(char modifier)
        {
            return (modifier == '*' || modifier == '~' /* || modifier == '!' */);
        }
        /// <summary>
        /// Compares a sequence item and a positionrecord, telling whether the 
        /// positionrecord is part of the sequence.
        /// </summary>
        /// <param name="si">The sequence item.</param>
        /// <param name="curr">The current PositionRecord.</param>
        /// <returns>Whether the position matches this sequence item.</returns>
        public static bool operator ==(SequenceItem si, PositionRecord curr)
        {
            if (si.Position == curr)
            {
                return true;
            }
            foreach (PositionRecord pr in si.leniency)
            {
                if (pr == curr)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Inverts the return value of the == operator.
        /// </summary>
        /// <param name="si">The sequence item.</param>
        /// <param name="curr">The current PositionRecord.</param>
        /// <returns>True if the position does not match this sequence item.</returns>
        public static bool operator !=(SequenceItem si, PositionRecord curr)
        {
            return ! (si == curr);
        }
        /// <summary>
        /// Whether or not this position would cancel the current sequence.
        /// </summary>
        /// <param name="curr">The current position.</param>
        /// <returns>Whether the sequence's progress will be cancelled by this position's occurrence.</returns>
        public bool CancelsSequence(PositionRecord curr)
        {
            if (this == curr)
            {
                return false;
            }
            foreach (PositionRecord pr in transition)
            {
                if (pr == curr)
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Creates a new SequenceItem with this name.
        /// </summary>
        /// <param name="name">The new sequenceitem's name.</param>
        private SequenceItem(string name)
        {
            Position = PositionParser.GetByName(name);
            transition = new HashSet<PositionRecord>();
            leniency = new HashSet<PositionRecord>();
        }
        /// <summary>
        /// Whether this item equals the other.
        /// </summary>
        /// <param name="obj">Other item.</param>
        /// <returns>Whether the two items are equal.</returns>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        /// <summary>
        /// Gets the item's hashcode.
        /// </summary>
        /// <returns>The item's hashcode.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
