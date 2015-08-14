using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace uPLibrary.Hardware
{
    /// <summary>
    /// Driver for TB6612FNG dual motor driver
    /// </summary>
    public class TB6612FNG : IDisposable
    {
        #region Constants...

        private const int DEFAULT_PWM_FREQUENCY = 10000; // Hz
        private const int MAX_PWM_FREQUENCY = 100000; // Hz

        #endregion

        #region Fields...

        // output ports and PWM for motor A
        private OutputPort aIn1Port;
        private OutputPort aIn2Port;
        private PWM aPwm;
        // output ports and PWM for motor B
        private OutputPort bIn1Port;
        private OutputPort bIn2Port;
        private PWM bPwm;
        // standby pin
        private OutputPort standbyPort;

        // motors mode
        private MotorMode motorModeA;
        private MotorMode motorModeB;

        // motors speed
        private int motorSpeedA;
        private int motorSpeedB;

        private bool isDisposed = false;

        #endregion

        #region Properties...

        /// <summary>
        /// Speed for Motor A
        /// </summary>
        public int MotorSpeedA
        {
            get
            {
                if (this.IsMotorEnabledA)
                    return this.motorSpeedA;
                else
                    throw new ApplicationException("Motor A is disabled !!");
            }
            set
            {
                if ((value >= 0) && (value <= 100))
                    this.motorSpeedA = value;
                this.aPwm.DutyCycle = (double)this.motorSpeedA / 100;
            }
        }

        /// <summary>
        /// Speed for Motor B
        /// </summary>
        public int MotorSpeedB
        {
            get
            {
                if (this.IsMotorEnabledB)
                    return this.motorSpeedB;
                else
                    throw new ApplicationException("Motor B is disabled !!");
            }
            set
            {
                if ((value >= 0) && (value <= 100))
                    this.motorSpeedB = value;
                this.bPwm.DutyCycle = (double)this.motorSpeedB / 100;
            }
        }

        /// <summary>
        /// Mode for Motor A
        /// </summary>
        public MotorMode MotorModeA
        {
            get
            {
                if (this.IsMotorEnabledA)
                    return this.motorModeA;
                else
                    throw new ApplicationException("Motor A is disabled !!");
            }
            set
            {
                switch (value)
                {
                    case MotorMode.CCW:
                        this.motorModeA = MotorMode.CCW;
                        this.aIn1Port.Write(false);
                        this.aIn2Port.Write(true);
                        break;
                    case MotorMode.CW:
                        this.motorModeA = MotorMode.CW;
                        this.aIn1Port.Write(true);
                        this.aIn2Port.Write(false);
                        break;
                    case MotorMode.ShortBrake:
                        this.motorModeA = MotorMode.ShortBrake;
                        this.aIn1Port.Write(true);
                        this.aIn2Port.Write(true);
                        break;
                    case MotorMode.Stop:
                        this.motorModeA = MotorMode.Stop;
                        this.aIn1Port.Write(false);
                        this.aIn2Port.Write(false);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Mode for Motor B
        /// </summary>
        public MotorMode MotorModeB
        {
            get
            {
                if (this.IsMotorEnabledB)
                    return this.motorModeB;
                else
                    throw new ApplicationException("Motor B is disabled !!");
            }
            set
            {
                switch (value)
                {
                    case MotorMode.CCW:
                        this.motorModeB = MotorMode.CCW;
                        this.bIn1Port.Write(false);
                        this.bIn2Port.Write(true);
                        break;
                    case MotorMode.CW:
                        this.motorModeB = MotorMode.CW;
                        this.bIn1Port.Write(true);
                        this.bIn2Port.Write(false);
                        break;
                    case MotorMode.ShortBrake:
                        this.motorModeB = MotorMode.ShortBrake;
                        this.bIn1Port.Write(true);
                        this.bIn2Port.Write(true);
                        break;
                    case MotorMode.Stop:
                        this.motorModeB = MotorMode.Stop;
                        this.bIn1Port.Write(false);
                        this.bIn2Port.Write(false);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Driver motor in standby
        /// </summary>
        public bool IsStandBy
        {
            get
            {
                if (standbyPort != null)
                    // standby signal is active low
                    return !this.standbyPort.Read();
                else
                    throw new ApplicationException("StandBy not managed !");
            }
            set
            {
                if (standbyPort != null)
                    // standby signal is active low
                    this.standbyPort.Write(!value);
                else
                    throw new ApplicationException("StandBy not managed !");
            }
        }

        /// <summary>
        /// Motor A enabled status
        /// </summary>
        public bool IsMotorEnabledA { get; private set; }

        /// <summary>
        /// Motor B enabled status
        /// </summary>
        public bool IsMotorEnabledB { get; private set; }

        #endregion

        /// <summary>
        /// Constructor for using only motor A
        /// </summary>
        /// <param name="aIn1">Pin channel A input 1</param>
        /// <param name="aIn2">Pin channel A input 2</param>
        /// <param name="aPwmChannel">Pwm channel A</param>
        /// <param name="standby">Pin standby</param>
        public TB6612FNG(Cpu.Pin aIn1, Cpu.Pin aIn2, Cpu.PWMChannel aPwmChannel, Cpu.Pin standby = Cpu.Pin.GPIO_NONE)
        {
            this.aIn1Port = new OutputPort(aIn1, false);
            this.aIn2Port = new OutputPort(aIn2, false);

            this.aPwm = new PWM(aPwmChannel, DEFAULT_PWM_FREQUENCY, 0, false);
            
            this.IsMotorEnabledA = true;
            this.MotorModeA = MotorMode.Stop;
            this.MotorSpeedA = 0;

            if (standby != Cpu.Pin.GPIO_NONE)
                // standby pin is active low so for not standby, set true output value
                this.standbyPort = new OutputPort(standby, true);

            this.aPwm.Start();
        }

        /// <summary>
        /// Constructor for using both motors A and B
        /// </summary>
        /// <param name="aIn1">Pin channel A input 1</param>
        /// <param name="aIn2">Pin channel A input 2</param>
        /// <param name="aPwmChannel">Pwm channel A</param>
        /// <param name="bIn1">Pin channel B input 1</param>
        /// <param name="bIn2">Pin channel B input 2</param>
        /// <param name="bPwmChannel">Pwm channel B</param>
        /// <param name="standby">Pin standby</param>
        public TB6612FNG(Cpu.Pin aIn1, Cpu.Pin aIn2, Cpu.PWMChannel aPwmChannel,
                         Cpu.Pin bIn1, Cpu.Pin bIn2, Cpu.PWMChannel bPwmChannel, 
                         Cpu.Pin standby = Cpu.Pin.GPIO_NONE)
            : this(aIn1, aIn2, aPwmChannel, standby)
        {
            this.bIn1Port = new OutputPort(bIn1, false);
            this.bIn2Port = new OutputPort(bIn2, false);

            this.bPwm = new PWM(bPwmChannel, DEFAULT_PWM_FREQUENCY, 0, false);
            
            this.IsMotorEnabledB = true;
            this.MotorModeB = MotorMode.Stop;

            this.bPwm.Start();
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
            if (!this.isDisposed)
            {
                // dispose managed resources
                if (disposing)
                {
                    // dispose motor A resources
                    if (this.IsMotorEnabledA)
                    {
                        this.aIn1Port.Write(false);
                        this.aIn2Port.Write(false);
                        this.aPwm.DutyCycle = 0;
                        this.aIn1Port.Dispose();
                        this.aIn2Port.Dispose();
                        this.aPwm.Dispose();
                    }

                    // dispose motor B resources
                    if (this.IsMotorEnabledB)
                    {
                        this.bIn1Port.Write(false);
                        this.bIn2Port.Write(false);
                        this.bPwm.DutyCycle = 0;
                        this.bIn1Port.Dispose();
                        this.bIn2Port.Dispose();
                        this.bPwm.Dispose();
                    }
                }
            }
            this.isDisposed = true;
        }

        #endregion
    }



    /// <summary>
    /// Motor mode
    /// </summary>
    public enum MotorMode
    {
        /// <summary>
        /// Counter Clock Wise
        /// </summary>
        CCW,

        /// <summary>
        /// Clock Wise
        /// </summary>
        CW,

        /// <summary>
        /// Short Brake
        /// </summary>
        ShortBrake,

        /// <summary>
        /// Stop
        /// </summary>
        Stop
    }
}
