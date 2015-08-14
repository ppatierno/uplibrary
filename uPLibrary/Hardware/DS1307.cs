using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace uPLibrary.Hardware
{
    /// <summary>
    /// Driver for DS1307 Maxim RTC
    /// </summary>
    public class DS1307
    {
        
        #region Constants...

        private const ushort ADDRESS = 0x68;                                        // 1101000 (see on datasheet)
        private const int CLOCK_RATE_KHZ = 100;                                     // DS1307 works only in I2C standard mode (100 Khz)

        // timekeeper registers addresses
        internal const byte SECONDS_ADDR                            = 0x00;         // Seconds address register
        internal const byte MINUTES_ADDR                            = 0x01;         // Minutes address register
        internal const byte HOURS_ADDR                              = 0x02;         // Hours address register
        internal const byte DAY_ADDR                                = 0x03;         // Day address register
        internal const byte DATE_ADDR                               = 0x04;         // Date address register
        internal const byte MONTH_ADDR                              = 0x05;         // Month address register
        internal const byte YEAR_ADDR                               = 0x06;         // Year address register
        internal const byte CONTROL_ADDR                            = 0x07;         // Control address register

        // bit masks
        internal const byte CLOCK_HALT                              = (1 << 7);     // clock halt for enabling/disabling oscillator
        internal const byte HOUR_12_24                              = (1 << 6);     // 12/25 hour mode (1 --> 12 hour, 0 --> 24 hour)
        internal const byte MODE_AM_PM                              = (1 << 5);     // AM/PM mode (1 --> PM, 0 --> AM)
        internal const byte OUTPUT_CONTROL                          = (1 << 7);     // output control (output level of SQW/OUT pin
        internal const byte SQUARE_WAVE_ENABLE                      = (1 << 4);     // enable oscillator output
        internal const byte RATE_SELECT_BIT0                        = (1 << 0);     // rate select bit 0
        internal const byte RATE_SELECT_BIT1                        = (1 << 1);     // rate select bit 1

        // rate select
        internal const byte RATE_SELECT_1Hz                         = 0;
        internal const byte RATE_SELECT_4096kHz                     = RATE_SELECT_BIT0;
        internal const byte RATE_SELECT_8192kHz                     = RATE_SELECT_BIT1;
        internal const byte RATE_SELECT_32768kHz                    = RATE_SELECT_BIT0 | RATE_SELECT_BIT1;
        internal const byte RATE_SELECT_MASK                        = RATE_SELECT_BIT0 | RATE_SELECT_BIT1;

        // I2C transaction timeout (ms)
        private const int TIMEOUT_TRANS = 1000;

        // size of buffers I2C communications 
        private const int RAM_ADDRESS_SIZE = 1;
        private const int RAM_DATA_SIZE = 1;

        #endregion

        #region Fields...

        // reference to I2C device
        I2CDevice i2c;

        // buffers for one and two bytes for I2C communications
        private byte[] ramAddress;
        private byte[] ramData;

        #endregion

        #region Ctor...

        /// <summary>
        /// Constructor
        /// </summary>
        public DS1307()
        {
            I2CDevice.Configuration i2cConfig = new I2CDevice.Configuration(ADDRESS, CLOCK_RATE_KHZ);
            this.i2c = new I2CDevice(i2cConfig);

            this.ramAddress = new byte[RAM_ADDRESS_SIZE];
            this.ramData = new byte[RAM_DATA_SIZE];
        }

        #endregion

        /// <summary>
        /// Get date and time from RTC
        /// </summary>
        /// <returns>Date and time read</returns>
        public DateTime GetDateTime()
        {
            DateTime dateTime;

            // read seconds converting from BCD
            this.ramAddress[0] = SECONDS_ADDR;
            this.ReadRam(this.ramAddress, this.ramData);
            // avoid clock halt bit
            this.ramData[0] = (byte)(this.ramData[0] & ~CLOCK_HALT);
            int second = (this.ramData[0] >> 4) * 10 + (this.ramData[0] & 0x0F);

            // read minutes converting from BCD
            this.ramAddress[0] = MINUTES_ADDR;
            this.ReadRam(this.ramAddress, this.ramData);
            int minute = (this.ramData[0] >> 4) * 10 + (this.ramData[0] & 0x0F);

            // read hours converting from BCD
            this.ramAddress[0] = HOURS_ADDR;
            this.ReadRam(this.ramAddress, this.ramData);
            // avoid 12/24 hour bit (forced 24 hour)
            this.ramData[0] = (byte)(this.ramData[0] & ~HOUR_12_24);
            int hour = (this.ramData[0] >> 4) * 10 + (this.ramData[0] & 0x0F);

            // read day converting from BCD
            this.ramAddress[0] = DAY_ADDR;
            this.ReadRam(this.ramAddress, this.ramData);
            DayOfWeek day = (DayOfWeek)this.ramData[0];

            // read date converting from BCD
            this.ramAddress[0] = DATE_ADDR;
            this.ReadRam(this.ramAddress, this.ramData);
            int date = (this.ramData[0] >> 4) * 10 + (this.ramData[0] & 0x0F);

            // read month converting from BCD
            this.ramAddress[0] = MONTH_ADDR;
            this.ReadRam(this.ramAddress, this.ramData);
            int month = (this.ramData[0] >> 4) * 10 + (this.ramData[0] & 0x0F);

            // read year converting from BCD
            this.ramAddress[0] = YEAR_ADDR;
            this.ReadRam(this.ramAddress, this.ramData);
            int year = (this.ramData[0] >> 4) * 10 + (this.ramData[0] & 0x0F);
            year += 2000;

            dateTime = new DateTime(year, month, date, hour, minute, second);

            return dateTime;
        }

        /// <summary>
        /// Set date and time on RTC
        /// </summary>
        /// <param name="dateTime">Date and time to write</param>
        public void SetDateTime(DateTime dateTime)
        {
            // programming seconds in BCD
            this.ramAddress[0] = SECONDS_ADDR;
            this.ramData[0] = (byte)((dateTime.Second / 10) << 4);
            this.ramData[0] |= (byte)(dateTime.Second % 10);
            // CH bit to 0 for enabling clock
            this.ramData[0] = (byte)(this.ramData[0] & ~CLOCK_HALT);
            this.WriteRam(this.ramAddress, this.ramData);

            // programming minutes in BCD
            this.ramAddress[0] = MINUTES_ADDR;
            this.ramData[0] = (byte)((dateTime.Minute / 10) << 4);
            this.ramData[0] |= (byte)(dateTime.Minute % 10);
            this.WriteRam(this.ramAddress, this.ramData);

            // programming hours in BCD
            this.ramAddress[0] = HOURS_ADDR;
            this.ramData[0] = (byte)((dateTime.Hour / 10) << 4);
            this.ramData[0] |= (byte)(dateTime.Hour % 10);
            // force 24H mode
            this.ramData[0] = (byte)(this.ramData[0] & ~HOURS_ADDR);
            this.WriteRam(this.ramAddress, this.ramData);

            // programming day of week in BCD
            this.ramAddress[0] = DAY_ADDR;
            this.ramData[0] = (byte)(dateTime.DayOfWeek + 1);
            this.WriteRam(this.ramAddress, this.ramData);

            // programming date in BCD
            this.ramAddress[0] = DATE_ADDR;
            this.ramData[0] = (byte)((dateTime.Day / 10) << 4);
            this.ramData[0] |= (byte)(dateTime.Day % 10);
            this.WriteRam(this.ramAddress, this.ramData);

            // programming month in BCD
            this.ramAddress[0] = MONTH_ADDR;
            this.ramData[0] = (byte)((dateTime.Month / 10) << 4);
            this.ramData[0] |= (byte)(dateTime.Month % 10);
            this.WriteRam(this.ramAddress, this.ramData);

            // programming year in BCD
            this.ramAddress[0] = YEAR_ADDR;
            int year = dateTime.Year - 2000;
            this.ramData[0] = (byte)((year / 10) << 4);
            this.ramData[0] |= (byte)(year % 10);
            this.WriteRam(this.ramAddress, this.ramData);
        }

        /// <summary>
        /// Configure SquareWave output pin behaviour
        /// </summary>
        /// <param name="outputControl">Output level of SQW/OUT pin when square wave output is disabled</param>
        /// <param name="squareWaveEnable">Enable/disable oscillator output</param>
        /// <param name="rateSelect">Frequency of square wave output when enabled</param>
        public void ConfigureSquareWave(bool outputControl, bool squareWaveEnable, RateSelect rateSelect)
        {
            this.ramAddress[0] = CONTROL_ADDR;

            this.ReadRam(this.ramAddress, this.ramData);
            
            // output control
            this.ramData[0] = outputControl ? (byte)(this.ramData[0] | OUTPUT_CONTROL) : (byte)(this.ramData[0] & ~OUTPUT_CONTROL);
            // square wave enable
            this.ramData[0] = squareWaveEnable ? (byte)(this.ramData[0] | SQUARE_WAVE_ENABLE) : (byte)(this.ramData[0] & ~SQUARE_WAVE_ENABLE);
            // square wave output frequency
            this.ramData[0] = (byte)(this.ramData[0] & ~RATE_SELECT_MASK);
            this.ramData[0] |= (byte)rateSelect;

            this.WriteRam(this.ramAddress, this.ramData);
        }

        /// <summary>
        /// Execute a write I2C transaction to a RAM location
        /// </summary>
        /// <param name="address">RAM address location for writing</param>
        /// <param name="data">Data buffer write to register</param>
        private void WriteRam(byte[] address, byte[] data)
        {
            I2CDevice.I2CTransaction[] i2cTx = new I2CDevice.I2CTransaction[1];

            // write RAM address and data to write
            i2cTx[0] = I2CDevice.CreateWriteTransaction(new byte[] { address[0], data[0] });

            // execute transaction
            if (this.i2c.Execute(i2cTx, TIMEOUT_TRANS) != (address.Length + data.Length))
                throw new ApplicationException("Error executing I2C reading from register");
        }

        /// <summary>
        /// Execute a read I2C transaction from a RAM location
        /// </summary>
        /// <param name="address">RAM address location for reading</param>
        /// <param name="data">Data buffer read from register</param>
        private void ReadRam(byte[] address, byte[] data)
        {
            I2CDevice.I2CTransaction[] i2cTx = new I2CDevice.I2CTransaction[2];

            // write RAM address location to read
            i2cTx[0] = I2CDevice.CreateWriteTransaction(address);
            // read data from RAM location
            i2cTx[1] = I2CDevice.CreateReadTransaction(data);

            // execute transaction
            if (this.i2c.Execute(i2cTx, TIMEOUT_TRANS) != (address.Length + data.Length))
                throw new ApplicationException("Error executing I2C reading from register");
        }
    }

    /// <summary>
    /// Rate select
    /// </summary>
    public enum RateSelect
    {
        /// <summary>
        /// 1 Hz
        /// </summary>
        _1Hz = DS1307.RATE_SELECT_1Hz,

        /// <summary>
        /// 4096 kHz
        /// </summary>
        _4096kHz = DS1307.RATE_SELECT_4096kHz,

        /// <summary>
        /// 8192 kHz
        /// </summary>
        _8192kHz = DS1307.RATE_SELECT_8192kHz,

        /// <summary>
        /// 32768 kHz
        /// </summary>
        _32768kHz = DS1307.RATE_SELECT_32768kHz
    }
}
