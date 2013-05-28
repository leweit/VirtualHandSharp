using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualHandSharp;
using System.IO;
using System.Threading;
namespace VirtualHandSharp.Emulator
{
    public class Recorder
    {
        /// <summary>
        /// The timer that handles the interval at which data will be polled.
        /// </summary>
        private Timer pollTimer = null;
        public string OutputPath { get; private set; }        
        private List<HandData> sequence;
        private Hand hand;
        public Recorder(Hand hand, int frequency, string outputPath)
        {
            if (frequency < 1 || 1000 < frequency)
            {
                throw new ArgumentException("Frequency should be between 1 and 1000", "frequency");
            }
            sequence = new List<HandData>();
            this.hand = hand;
            this.hand.StopPolling();
            OutputPath = outputPath;

            pollTimer = new Timer(pollTimerTick, null, 0, 1000 / frequency);
        }

        private void pollTimerTick(object state)
        {
            hand.Update();
        }

        public void Start()
        {
            hand.DataUpdated += DataUpdated;
            hand.StartPolling();
        }

        public void DataUpdated(Hand sender)
        {
            HandData data = new HandData();
            for (int i = 0; i < 22; i++)
            {
                data[i].Value = sender[i].Radians;
            }
            sequence.Add(data);
        }

        public void Stop()
        {
            hand.DataUpdated -= DataUpdated;
            writeToFile();
            
        }
        private void writeToFile()
        {
            File.Create(OutputPath).Close();
            StreamWriter file = new StreamWriter(OutputPath);
            foreach (HandData record in sequence)
            {
                writeRecord(file, record);
            }
            file.Close();
            file.Dispose();
        }
        private void writeRecord(StreamWriter output, HandData record)
        {
            output.WriteLine(record.CSV);
        }
    }
}
