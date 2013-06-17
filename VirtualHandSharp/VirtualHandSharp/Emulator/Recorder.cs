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
        /// <param name="fps">The refresh rate per second.</param>
        /// <param name="outputPath">The path to the output file. This file will be overwritten.</param>
        public Recorder(Hand hand, int fps, string outputPath)
        {
            // The FPS has to be between 1 and 1000
            if (fps < 1 || 1000 < fps)
            {
                throw new ArgumentException("Frequency should be between 1 and 1000", "frequency");
            }
            // Initialise the sequence, set members
            OutputPath = outputPath;
            sequence = new List<HandData>();
            // Connect the hand, make it stop polling.
            this.hand = hand;
            this.hand.StopPolling();
            // Make a timer which tell the hand when to poll, instead.
            pollTimer = new Timer(pollTimerTick, null, 0, 1000 / fps);
        }
        /// <summary>
        /// The handler for the Timer's ticks. Will update hand data and 
        /// write it to the sequence.
        /// </summary>
        /// <param name="state">The timer's state.</param>
        private void pollTimerTick(object state)
        {
            // Update the hand's data.
            hand.Update();
        }
        /// <summary>
        /// Begins polling and recording.
        /// </summary>
        public void Start()
        {
            // Hook into the update event.
            hand.DataUpdated += DataUpdated;
        }
        /// <summary>
        /// Event handler for the DataUpdated event; This will add the new data to the
        /// current sequence.
        /// </summary>
        /// <param name="sender">The hand that got updated.</param>
        public void DataUpdated(Hand sender)
        {
            // Make a new container for the data.
            HandData data = new HandData();
            // Populate the container.
            for (int i = 0; i < 22; i++)
            {
                data[i].Value = sender[i].Radians;
            }
            // Add the new HandData to the sequence list.
            sequence.Add(data);
        }
        /// <summary>
        /// Tells the recorder to stop polling. This will write the data to 
        /// the output file.
        /// </summary>
        public void Stop()
        {
            // Unhook from the DataUpdated event.
            hand.DataUpdated -= DataUpdated;
            // Write the sequence down.
            writeToFile();
        }
        /// <summary>
        /// Writes the sequence to the output file.
        /// </summary>
        private void writeToFile()
        {
            // Create the file and make the streamwriter. We use Create because 
            // the file will be overwritten.
            StreamWriter file = new StreamWriter(File.Create(OutputPath));
            // Write each record to the file.
            foreach (HandData record in sequence)
            {
                file.WriteLine(record.CSV);
            }
            // Destroy the streamreader.
            file.Close();
            file.Dispose();
        }
    }
}
