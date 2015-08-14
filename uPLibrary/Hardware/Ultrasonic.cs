using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;

namespace uPLibrary.Hardware
{
    /// <summary>
    /// Args for distance event
    /// </summary>
    public class UltrasonicEventArgs : EventArgs
    {
        /// <summary>
        /// Distance (mm)
        /// </summary>
        public long Distance { get; internal set; }

        /// <summary>
        /// Time (us)
        /// </summary>
        public long Time { get; internal set; }
    }

    /// <summary>
    /// Driver for Ultrasonic sensor
    /// </summary>
    public class Ultrasonic : IDisposable
    {
        // 1 tick = 0.1 us (10 ticks = 1 us)
        private const long TICKS_PER_MICROSECONDS = 10;
        // 1 tick -> sound goes for 29.14 mm
        private const double MM_PER_TICKS = 29.14;
        // with round trip
        private const double MM_PER_TRICKS_ROUND_TRIP = MM_PER_TICKS * 2;

        /// <summary>
        /// Delegate that defines distance event handler
        /// </summary>
        public delegate void DistanceEventHandler(object sender, UltrasonicEventArgs e);

        // reference to tristate port for driving sensor and reading data
        private TristatePort trPort;
        // timer for periodic distance measure
        private Timer timer;

        // distance event
        public event DistanceEventHandler Distance;

        // period for distance measure
        private int period;

        /// <summary>
        /// Period for distance measure
        /// </summary>
        public int Period
        {
            get
            {
                return this.period;
            }
            set
            {
                if (value != this.period)
                {
                    this.period = value;
                    this.timer.Change(this.period, this.period);
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pin">Pin connected to the Ultrasonic sensor</param>
        /// <param name="glitchFilter">Input debouncing filter (default is true)</param>
        /// <param name="resistor">The resistor mode that establishes a default state for the port (default is Disabled)</param>
        /// <param name="period">Period for distance measure</param>
        public Ultrasonic(Cpu.Pin pin,
            bool glitchFilter = true,
            Port.ResistorMode resistor = Port.ResistorMode.Disabled,
            int period = 5000)
        {
            this.trPort = new TristatePort(pin, false, glitchFilter, resistor);
            this.period = period;

            // set duetime as period
            this.timer = new Timer(DistanceMeasureCallback, null, this.period, this.period);
        }

        private void DistanceMeasureCallback(object state)
        {
            long ticks = this.GetTicks();
            
            // raise event with distance in mm and time in us
            this.OnDistance((long)(ticks / MM_PER_TRICKS_ROUND_TRIP), (long)(ticks / TICKS_PER_MICROSECONDS));
        }

        /// <summary>
        /// Get distance measured by sensor
        /// </summary>
        /// <returns>Distance (mm) measured</returns>
        public long GetDistance()
        {
            return (long)(this.GetTicks() / MM_PER_TRICKS_ROUND_TRIP);
        }

        /// <summary>
        /// Get time (in ticks) measuring distance
        /// </summary>
        /// <returns>Time (in ticks) measured</returns>
        private long GetTicks()
        {
            // set tristate port as output port
            this.trPort.Active = true;
            // pulse to sensor for starting measure
            this.trPort.Write(false);
            this.trPort.Write(true);
            this.trPort.Write(false);

            // set tristate port as input port
            this.trPort.Active = false;
            // wait for rising edge
            while (!this.trPort.Read()) ; // do nothing while input signal is low
            // mark start time of high level
            long startTicks = DateTime.Now.Ticks;

            // wait for falling edge
            while (this.trPort.Read()) ; // do nothing while input signal is high

            // return high level signal duration (in ticks) : 1 tick = 0.1 us
            return (DateTime.Now.Ticks - startTicks);
        }
        
        /// <summary>
        /// Wrapper method for raising distance event
        /// </summary>
        /// <param name="distance">Distance from object</param>
        /// <param name="time">Ultrasonic wave round trip time</param>
        private void OnDistance(long distance, long time)
        {
            if (this.Distance != null)
                this.Distance(this, new UltrasonicEventArgs() { Distance = distance, Time = time });
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
                this.timer.Dispose();
                this.trPort.Dispose();
                this.trPort = null;
            }
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~Ultrasonic()
        {
            this.Dispose(false);
        }

        #endregion
    }
}
