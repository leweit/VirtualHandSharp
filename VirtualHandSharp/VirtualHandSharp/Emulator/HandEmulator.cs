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
    public class HandEmulator : Hand
    {
        private List<HandData> sequence;
        private int currentPos;
        public string InputPath { get; private set; }
        public bool Recur;
        public HandEmulator(string path, bool recur)
            : base(false)
        {
            InputPath = path;
            Recur = recur;
            currentPos = 0;
            sequence = new List<HandData>();
            readSequence();
        }

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

            checkIfEnded();
        }

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
    }
}
