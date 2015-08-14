using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace uPLibrary.Hardware
{
    /// <summary>
    /// Args for Pir motion event
    /// </summary>
    public class PirEventArgs : EventArgs
    {
        /// <summary>
        /// Motion detected state
        /// </summary>
        public bool Motion { get; internal set; }

        /// <summary>
        /// Timestamp when motion detected or not occured
        /// </summary>
        public DateTime Time { get; internal set; }
    }

    /// <summary>
    /// Driver for Passive Infrared Sensor
    /// </summary>
    public class Pir : IDisposable
    {
        /// <summary>
        /// Delegate that define motion event handler
        /// </summary>
        public delegate void MotionEventHandler(object sender, PirEventArgs e);

        #region Fields...

        // reference to interrupt port
        private InterruptPort intPort;

        // motion event
        public event MotionEventHandler Motion;

        // Pir enable state
        private bool enabled;

        #endregion

        #region Properties...

        /// <summary>
        /// Pir enable state
        /// </summary>
        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
            set
            {
                if (value != this.enabled)
                {
                    this.enabled = value;
                    if (this.enabled)
                        this.intPort.EnableInterrupt();
                    else
                        this.intPort.DisableInterrupt();
                }
            }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pin">Pin connected to the PIR</param>
        /// <param name="glitchFilter">Input debouncing filter (default is true)</param>
        /// <param name="resistor">The resistor mode that establishes a default state for the port (default is Disabled)</param>
        /// <param name="interrupt">Defines the type of edge-change triggering the interrupt (default is InterruptEdgeBoth)</param>
        /// <param name="enabled">Intial Pir enable state</param>
        public Pir(Cpu.Pin pin,
            bool glitchFilter = true,
            Port.ResistorMode resistor = Port.ResistorMode.Disabled,
            Port.InterruptMode interrupt = Port.InterruptMode.InterruptEdgeBoth,
            bool enabled = true)
        {
            this.intPort = new InterruptPort(pin, glitchFilter, resistor, interrupt);
            this.intPort.OnInterrupt += new NativeEventHandler(intPort_OnInterrupt);

            this.Enabled = enabled;
        }

        /// <summary>
        /// Wrapper method for raising motion event
        /// </summary>
        /// <param name="motion">Motion detected state</param>
        /// <param name="time">Timestamp when motion detected or not occured</param>
        private void OnMotion(bool motion, DateTime time)
        {
            if (this.Motion != null)
                this.Motion(this, new PirEventArgs() { Motion = motion, Time = time });
        }

        /// <summary>
        /// Interrupt event handler
        /// </summary>
        /// <param name="port">Port number that triggered the interrupt</param>
        /// <param name="state">The state of the interrupt edge</param>
        /// <param name="time">Timestamp when the interrupt occured</param>
        private void intPort_OnInterrupt(uint pin, uint state, DateTime time)
        {
            this.OnMotion(state == 1, time);

            // clear interrupt if interrupt mode is set on level (high or low)
            if ((this.intPort.Interrupt == Port.InterruptMode.InterruptEdgeLevelHigh) ||
                (this.intPort.Interrupt == Port.InterruptMode.InterruptEdgeLevelLow))
                this.intPort.ClearInterrupt();
        }

        #region IDisposable and Dispose Pattern...

        /// <summary>
        /// Disponse() method from IDisposable interface
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Internal dispose method
        /// </summary>
        /// <param name="disposing">Disposing managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.intPort.DisableInterrupt();
                this.intPort.Dispose();
                this.intPort = null;
            }
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~Pir()
        {
            this.Dispose(false);
        }

        #endregion
    }
}
