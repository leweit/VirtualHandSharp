using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using VirtualHandSharp.Position;
using VirtualHandSharp.Motion;

namespace VirtualHandSharp
{
    /// <summary>
    /// The <see cref="VirtualHandSharp"/> namespace containing the classes required for
    /// the creation and usage of a Hand object.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    /// <summary>
    /// An instance of this class can communicate with a connected CyberGlove. Its data gets updated
    /// at a set interval. The finger objects can either be accessed by their names (thumb, index, etc)
    /// or through the "fingers" array, which is thumb-indexed (meaning thumb's index is 0, pinkie's index is 4, etc).
    /// </summary>
    /// <author>Arno Sluismans</author>
    public class Hand : HandData
    {
        #region Members
        /// <summary>
        /// The interval at which we want the hand's data to be refreshed. (Expressed in milliseconds).
        /// This property is write-only.
        /// </summary>
        /// <author>Arno Sluismans</author>
        public int Interval
        {
            set
            {
                if (pollTimer != null)
                {
                    // Only set the interval if the pollTimer object exists
                    pollTimer.Change(0, value);
                }
                interval = value;
            }
            get
            {
                return interval;
            }
        }
        private int interval;
        /// <summary>
        /// An IntPtr that points to address containing the C++ class of the CyberGlove. 
        /// Accessing its methods requires for them to be bridged in VirtualHandBridge.dll
        /// and should be done with caution. By any means, this is the most difficult part of this project.
        /// See below (at the end of this file) for a couple of such examples.
        /// </summary>
        private IntPtr vhtHand = IntPtr.Zero;
        /// <summary>
        /// The timer that handles the interval at which data will be polled.
        /// </summary>
        private Timer pollTimer = null;
        /// <summary>
        /// The current position. Null means none was recognized.
        /// </summary>
        public PositionRecord CurrentPosition { get; protected set; }
        /// <summary>
        /// Returns the last position that was recognized.
        /// </summary>
        public PositionRecord LastPosition { get; protected set; }
        #endregion
        #region Properties
        /// <summary>
        /// Gets or sets whether the polling for data is currently enabled.
        /// </summary>
        /// <author>Arno Sluismans</author>
        public bool Polling { get; private set; }
        #endregion
        #region Events
        /// <summary>
        /// Event handler for the hand's PositionChanged event.
        /// </summary>
        /// <param name="sender">The event's sender. Usually "this".</param>
        /// <param name="position">The new position.</param>
        public delegate void PositionChangedHandler(Hand sender, PositionRecord position);
        /// <summary>
        /// Event that gets called when the hand's position has changed.
        /// </summary>
        public event PositionChangedHandler PositionChanged;
        /// <summary>
        /// Invokes the PositionChanged event. Useful for derived classes.
        /// </summary>
        /// <param name="sender">The event's sender. Usually "this".</param>
        /// <param name="position">The new position.</param>
        protected void PositionHasChanged(Hand sender, PositionRecord position)
        {
            if (PositionChanged != null)
                PositionChanged(sender, position);
        }
        /// <summary>
        /// Handler for the DataUpdated event.
        /// </summary>
        public delegate void DataUpdatedHandler(Hand sender);
        /// <summary>
        /// Event that gets called when the data is updated.
        /// </summary>
        public event DataUpdatedHandler DataUpdated;
        /// <summary>
        /// Invokes the DataUpdated event. Useful for derived classes.
        /// </summary>
        /// <param name="sender">The sender. Usually "this".</param>
        protected void DataHasUpdated(Hand sender)
        {
            if (DataUpdated != null)
                DataUpdated(sender);
        }
        /// <summary>
        /// Handler for the MotionDetected event.
        /// </summary>
        /// <param name="sender">The hand that matched the motion.</param>
        /// <param name="motion">The motion that was matched.</param>
        public delegate void MotionDetectedHandler(Hand sender, MotionRecord motion);
        /// <summary>
        /// The event that gets called when a motion is matched.
        /// </summary>
        public event MotionDetectedHandler MotionDetected;
        /// <summary>
        /// Invokes the MotionDetected event. 
        /// </summary>
        /// <param name="sender">The sender. Usually "this".</param>
        /// <param name="motion">The newly detected motion.</param>
        protected void MotionHasDetected(Hand sender, MotionRecord motion)
        {
            if (MotionDetected != null)
                MotionDetected(sender, motion);
        }
        #endregion
        #region Public Functions
        /// <summary>
        /// Default constructor; creates all fingers and initiates connection with the CyberGlove. Basically,
        /// after the constructor has been called, this object is ready for use.
        /// </summary>
        /// <author>Arno Sluismans</author>
        public Hand() : base()
        {
            initConnection();
        }
        /// <summary>
        /// Calls the C++ function that deletes the CyberHand object.
        /// </summary>
        /// <author>Arno Sluismans</author>
        public void Delete()
        {
            // Delete the hand.
            DeleteHand(vhtHand);
            // Set our local hand variable to null.
            vhtHand = IntPtr.Zero;
        }
        /// <summary>
        /// Can be used to manually request an update of data. This is especially useful if you want
        /// to disable the timer and decide manually when things should be updated.
        /// </summary>
        /// <author>Arno Sluismans</author>
        public void Update()
        {
            // request an update
            pollJointData();
        }
        /// <summary>
        /// Makes the hand vibrate.
        /// </summary>
        /// <param name="v">How hard it should vibrate.</param>
        public void Vibrate(double v)
        {
            VibrateAll(vhtHand, v);
        }
        /// <summary>
        /// Sets Polling to true, and (re)starts the pollTimer. If needed, it also creates
        /// a new Timer object to serve as pollTimer.
        /// </summary>
        /// <author>Arno Sluismans</author>
        public void StartPolling()
        {
            // Tell the application that we are actively polling for data.
            Polling = true;
            // If the timer has not been initialised, we initialise it.
            if (pollTimer == null)
            {
                // Instantiate the timer
                pollTimer = new Timer(pollTimerTick, null, 0, Interval);
            }
            else
            {
                pollTimer.Change(0, Interval);
            }
        }
        
        /// <summary>
        /// Sets Polling to false, so that the timer will stop running. No new updates will
        /// be made, unless asked specifically with the Update() method, or when the timer is told
        /// to continue.
        /// </summary>
        public void StopPolling()
        {
            Polling = false;
        }
        #endregion
        #region Private Functions

        /// <summary>
        /// Populates the hand's joints with the given array of doubles.
        /// </summary>
        /// <param name="data">The joint values. Needs to have a length of 22.</param>
        private void Populate(double[] data)
        {
            int max = NR_FINGERS * NR_JOINTS + 2;
            if (data.Length != max)
            {
                throw new ArgumentException("The data's length needs to be 22.", "data");
            }
            // Go through the fingers array to populate the fingers with their data.
            for (int i = 0; i < NR_FINGERS; i++)
            {
                // Populate the finger with joint data.
                Fingers[i].SetJoints(
                    data[i * NR_JOINTS + 0], // Inner joint
                    data[i * NR_JOINTS + 1], // Middle joint
                    data[i * NR_JOINTS + 2]  // Outer joint
                );
                // Get the abduction between this finger and the next.
                double abd = data[i * NR_JOINTS + 3];
                if (i >= NR_FINGERS - 1)
                {
                    palmArch.Value = abd;
                }
                else
                {
                    Fingers[i].NextAbduction.Value = abd;
                    Fingers[i + 1].PreviousAbduction.Value = abd;
                }
            }
            // Set wrist pitch and yaw.
            wristPitch.Value = data[max - 2];
            wristYaw.Value = data[max - 1];
        }
        
        /// <summary>
        /// Initiates connection with the CyberGlove.
        /// </summary>
        private void initConnection()
        {
            // Initialise the Interval to a standard value.
            Interval = 1000;
            CurrentPosition = null;
            try
            {
                // Initialise the CyberHand connection.
                initVhtHand();
            }
            catch (DllNotFoundException e)
            {
                // VirtualHandBridge.dll most likely had a problem in it. Maybe a missing dependency.
                throw new ConnectionFailedException("DllNotFoundException in initVhtHand(): " + e.Message + " -- Are you sure that the CGS dlls and glut64.dll are in the correct directory?");
            }
            catch (SEHException e)
            {
                // The C++ library has thrown an exception.
                throw new ConnectionFailedException("SEHException in initVhtHand(): " + e.Message);
            }

            // If vhtHand is still not assigned to an actual spot in the memory, something must have
            // gone wrong without an exception being thrown. 
            if (vhtHand == IntPtr.Zero)
            {
                throw new ConnectionFailedException("The vhtHand was still NULL after its creation. Something must have gone wrong.");
            }
        }
        /// <summary>
        /// Requests renewed data from the CyberGlove and populates the finger objects with this
        /// new data.
        /// </summary>
        /// <author>Arno Sluismans</author>
        protected virtual void pollJointData()
        {
            // Calculate the size of the array.
            int max = NR_FINGERS * NR_JOINTS + 2;
            // Create an array of doubles, that will later be filled with joint data.
            double[] arr = new double[max];
            // Poll() fills arr with data and returns the size of the new array.
            // If that size is not 22, something weird is going on.
            int size = Poll(this.vhtHand, arr, max);
            StopPolling();
            // Fill the hand with data.
            Populate(arr);
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
       } 
        /// <summary>
        /// Event handler for the pollTimer's Tick event. It calls pollJointData().
        /// </summary>
        /// <param name="state"></param>
        /// <author>Arno Sluismans</author>
        protected void pollTimerTick(object state)
        {
            if (Polling)
            {
                // If we are still supposed to be polling for data, 
                // this is the moment to poll for it.
                pollJointData();
            }
            else
            {
                // If we're not supposed to be polling for data, we stop the
                // pollTimer's ticking as it has become useless.
                pollTimer.Change(System.Threading.Timeout.Infinite, Interval);
            }
        }
        /// <summary>
        /// Calls the C++ function that constructs a CyberGlove. It stores the pointer in 
        /// vhtHand, but please be aware that this pointer can not be used as such inside C# code.
        /// </summary>
        /// <author>Arno Sluismans</author>
        protected void initVhtHand()
        {
            // Call the C++ function that creates a CyberHand and returns the object as a pointer.
            vhtHand = CreateHand();
        }
        /// <summary>
        /// Constructs a hand, with the option to not connect to a CyberGlove.
        /// This can be useful for emulators and MOCK objects.
        /// </summary>
        /// <param name="connect">Whether or not it should connect to the CyberGlove.</param>
        protected Hand(bool connect)
        {
            if (connect)
                initConnection();
        }
        #endregion
        #region Marshalled Functions

        /// <summary>
        /// Creates a new CyberHand object.
        /// </summary>
        /// <returns>Returns a pointer to the newly created object.</returns>
        /// <author>Arno Sluismans</author>
        [DllImport("VirtualHandBridge.dll")]
        static public extern IntPtr CreateHand();

        /// <summary>
        /// Deletes the CyberHand object.
        /// </summary>
        /// <param name="hand">The pointer to the object we need to dispose of.</param>
        /// <author>Arno Sluismans</author>
        [DllImport("VirtualHandBridge.dll")]
        static public extern void DeleteHand(IntPtr hand);

        /// <summary>
        /// Polls the hand for its current data.
        /// </summary>
        /// <param name="hand">The CyberHand::Hand object.</param>
        /// <param name="buffer">The array of doubles that we will want to fill with data.</param>
        /// <param name="bufferSize">The length of the buffer. Normally this should be 15.</param>
        /// <returns>The new length of the buffer array. Normally this should be 15.</returns>
        [DllImport("VirtualHandBridge.dll")]
        static public extern int Poll(IntPtr hand, double[] buffer, int bufferSize);

        /// <summary>
        /// Makes the hand vibrate.
        /// </summary>
        /// <param name="hand">The hand object.</param>
        /// <param name="vibration">How hard it needs to vibrate.</param>
        [DllImport("VirtualHandBridge.dll")]
        static public extern void VibrateAll(IntPtr hand, double vibration);
        #endregion
    }
}
