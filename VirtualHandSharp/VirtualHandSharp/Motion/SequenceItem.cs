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
        public bool HasTransitionModifier { get; private set; }
        public bool HasLenientModifier { get; private set; }

        private HashSet<PositionRecord> transition;
        private HashSet<PositionRecord> leniency;
        public void AddToTransition(PositionRecord position)
        {
            transition.Add(position);
        }

        public PositionRecord Position { get; private set; }
        private static bool isAlphanumeric(string word)
        {
            for (int i = 0; i < word.Length; i++)
            {
                if (!Char.IsLetterOrDigit(word[i]))
                    return false;
            }
            return true;
        }
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

        private void parseModifiers(List<char> modifiers)
        {
            HasLenientModifier = false;
            HasTransitionModifier = false;

            foreach (char m in modifiers)
            {
                parseModifier(m);
            }
        }

        private void parseModifier(char m)
        {
            switch (m)
            {
                case '*':
                    HasLenientModifier = true;
                    break;
                case '~':
                    HasTransitionModifier = true;
                    break;
                default:
                    throw new MalformedException(string.Format("The modifier {0} is invalid", m));
            }
        }

        public void InitLeniency()
        {
            if (!HasLenientModifier)
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

        private static bool isModifier(char modifier)
        {
            return (modifier == '*' || modifier == '~' /* || modifier == '!' */);
        }

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



        public static bool operator !=(SequenceItem si, PositionRecord curr)
        {
            return ! (si == curr);
        }

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

        private SequenceItem(string name)
        {
            Position = PositionParser.GetByName(name);
            transition = new HashSet<PositionRecord>();
            leniency = new HashSet<PositionRecord>();
        }
    }
}
