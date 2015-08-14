using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;

namespace uPLibrary.Hardware
{
    /// <summary>
    /// Driver for a generic anemometer
    /// </summary>
    public class Anemometer
    {
        #region Constants ...

        // default watching period for calculating wind speed
        public const int DEFAULT_CALCULATE_PERIOD = 5000; // ms
        // interval for debouncing
        private const int DEBOUNCING_INTERVAL = 1; // ms

        // the following values are for the anemometer used for testing

        // reference wind speed
        private const float REFERENCE_WIND_SPEED = 10; // km/h
        // reference pulse for second
        private const float REFERENCE_PULSE_FOR_SECOND = 4;

        #endregion

        #region Fields ...

        // watching period for calculating wind speed
        private int calculatePeriod;
        // interrupt port bind to anemometer internal switch
        private InterruptPort inPort;
        // timer for calculating wind speed periodically
        private Timer timer;
        // previous pulse ticks
        private long prevPulseTicks;
        // anemometer pulse count
        private int pulseCount;

        // reference wind speed and pulse for second
        private float referenceWindSpeed;
        private float referencePulseForSecond;

        private object lockObj = new Object();

        #endregion

        #region Properties ...

        /// <summary>
        /// Wind speed
        /// </summary>
        public float WindSpeed { get; private set; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="inPin">Pin used for read anemometer internal switch</param>
        /// <param name="calculatePeriod">Period for calculating wind speed</param>
        /// <param name="referenceWindSpeed">Reference wind speed</param>
        /// <param name="referencePulseForSecond">Reference pulse for second</param>
        public Anemometer(Cpu.Pin inPin, 
            int calculatePeriod = DEFAULT_CALCULATE_PERIOD,
            float referenceWindSpeed = REFERENCE_WIND_SPEED,
            float referencePulseForSecond = REFERENCE_PULSE_FOR_SECOND)
        {
            this.calculatePeriod = calculatePeriod;
            this.referenceWindSpeed = referenceWindSpeed;
            this.referencePulseForSecond = referencePulseForSecond;

            this.inPort = new InterruptPort(inPin, false, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeLow);
            this.inPort.OnInterrupt += new NativeEventHandler(intPort_OnInterrupt);

            // create timer but not start it yet
            this.timer = new Timer(CalculateWindSpeed, null, Timeout.Infinite, 0);
        }

        /// <summary>
        /// Start periodically wind speed calculation
        /// </summary>
        public void Start()
        {
            this.timer.Change(0, this.calculatePeriod);
        }

        /// <summary>
        /// Stop periodically wind speed calculation
        /// </summary>
        public void Stop()
        {
            this.timer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        
        void intPort_OnInterrupt(uint data1, uint data2, DateTime time)
        {
            lock (this.lockObj)
            {
                long ticks = time.Ticks;
                // if two consecutive interrupts are very closed (inside DEBOUNCING_INTERVAL),
                // we need filter with a debouncing
                if (ticks - prevPulseTicks < DEBOUNCING_INTERVAL * TimeSpan.TicksPerMillisecond)
                    return;
                else
                {
                    prevPulseTicks = ticks;
                    pulseCount++;
                }
            }
        }

        void CalculateWindSpeed(object state)
        {
            lock (this.lockObj)
            {
                this.WindSpeed = (this.referenceWindSpeed * ((float)pulseCount / this.calculatePeriod) * (1000 / this.referencePulseForSecond));
                pulseCount = 0;
            }
        }
    }
}
