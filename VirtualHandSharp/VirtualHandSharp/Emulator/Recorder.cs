using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualHandSharp;
using System.IO;
using System.Threading;
namespace VirtualHandSharp.Emulator
{
    /// <summary>
    /// A recorder can create Emulation records based on 
    /// the user's movements. The HandEmulator 
    /// can then emulate a hand and replay this record.
    /// </summary>
    public class Recorder
    {
        /// <summary>
        /// The timer that handles the interval at which data will be polled.
        /// </summary>
        private Timer pollTimer = null;
        /// <summary>
        /// The path to the file that should be written to.
        /// </summary>
        public string OutputPath { get; private set; }
        /// <summary>
        /// The sequence of data that will be written to the output file.
        /// </summary>
        private List<HandData> sequence;
        /// <summary>
        /// The connected hand.
        /// </summary>
        private Hand hand;
        /// <summary>
        /// Creates a custom recorder.
        /// </summary>
        /// <param name="hand">The connected hand.</param>
        /// <param name="frequency">The refresh rate per second.</param>
        /// <param name="outputPath">The path to the output file. This file will be overwritten.</param>
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
        /// <summary>
        /// The handler for the Timer's ticks. Will update hand data and write it to the sequence.
        /// </summary>
        /// <param name="state"></param>
        private void pollTimerTick(object state)
        {
            hand.Update();
        }
        /// <summary>
        /// Begins polling and recording.
        /// </summary>
        public void Start()
        {
            hand.DataUpdated += DataUpdated;
            hand.StartPolling();
        }
        /// <summary>
        /// Event handler for the DataUpdated event; This will add the new data to the
        /// current sequence.
        /// </summary>
        /// <param name="sender">The hand that got updated.</param>
        public void DataUpdated(Hand sender)
        {
            HandData data = new HandData();
            for (int i = 0; i < 22; i++)
            {
                data[i].Value = sender[i].Radians;
            }
            sequence.Add(data);
        }
        /// <summary>
        /// Tells the recorder to stop polling. This will write the data to 
        /// the output file.
        /// </summary>
        public void Stop()
        {
            hand.DataUpdated -= DataUpdated;
            writeToFile();
            
        }
        /// <summary>
        /// Writes the sequence to the output file.
        /// </summary>
        private void writeToFile()
        {
            File.Create(OutputPath).Close();
            StreamWriter file = new StreamWriter(OutputPath);
            foreach (HandData record in sequence)
            {
                file.WriteLine(record.CSV);
            }
            file.Close();
            file.Dispose();
        }
    }
}
