using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;

namespace uPLibrary.Hardware
{
    /// <summary>
    /// Driver for SHT1X humidity and temperature sensor
    /// </summary>
    public class SHT1X
    {
        #region Constants ...

        // address (3 bit, only b000 supportted);
        private const byte ADDR = 0x00;

        // commands (5 bits)
        private const byte CMD_MEASURE_TEMP = 0x03;     // b00011
        private const byte CMD_MEASURE_RH = 0x05;       // b00101
        private const byte CMD_READ_STATUS_REG = 0x07;  // b00111
        private const byte CMD_WRITE_STATUS_REG = 0x06; // b00110
        private const byte CMD_SOFT_RESET = 0x1E;       // b11110

        // d1, d2 coefficients for temperature
        // Celsius
        private double D1_VDD5V_C = -40.1F;
        private double D1_VDD4V_C = -39.8F;
        private double D1_VDD3_5V_C = -39.7F;
        private double D1_VDD3V_C = -39.6F;
        private double D1_VDD2_5V_C = -39.4F;
        private double D2_14BIT_C = 0.01F;
        private double D2_12BIT_C = 0.04F;
        // Farenheit
        private double D1_VDD5V_F = -40.2F;
        private double D1_VDD4V_F = -39.6F;
        private double D1_VDD3_5V_F = -39.5F;
        private double D1_VDD3V_F = -39.3F;
        private double D1_VDD2_5V_F = -38.9F;
        private double D2_14BIT_F = 0.018F;
        private double D2_12BIT_F = 0.072F;

        // c1, c2, c3, t1, t2 coefficients for relative humidity
        // 12 bit resolution
        private double C1_12BIT = -2.0468F;
        private double C2_12BIT = 0.0367F;
        private double C3_12BIT = -1.5955E-6F;
        private double T1_12BIT = 0.01F;
        private double T2_12BIT = 0.00008F;
        // 8 bit resolution
        private double C1_8BIT = -2.0468F;
        private double C2_8BIT = 0.5872F;
        private double C3_8BIT = -4.0845E-4F;
        private double T1_8BIT = 0.01F;
        private double T2_8BIT = 0.00128F;

        // mask for bits inside status register
        private byte END_OF_BATTERY_MASK = 0x40;
        private byte HEATER_MASK = 0x04;
        private byte NO_RELOAD_FROM_OTP_MASK = 0x02;
        private byte LESS_RESOLUTION = 0x01;

        #endregion

        #region Fields ...

        // output port for clock
        private OutputPort clkPort;
        // tristate port for input/output for data 
        private TristatePort dataPort;
        // voltage powering sensor
        private Voltage voltage;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="data">Pin for DATA</param>
        /// <param name="clk">Pin for CLOCK</param>
        /// <param name="voltage">Voltage powering sensor</param>
        public SHT1X(Cpu.Pin data, Cpu.Pin clk, Voltage voltage)
        {
            this.clkPort = new OutputPort(clk, false);
            this.dataPort = new TristatePort(data, false, true, Port.ResistorMode.Disabled);
            // set as output
            this.dataPort.Active = true;

            this.voltage = voltage;
        }

        /// <summary>
        /// Execute transmission start sequence for commands
        /// </summary>
        private void StartTransmission()
        {
            if (!this.dataPort.Active)
                // set data port as output
                this.dataPort.Active = true;

            // transmission start sequence
            //this.dataPort.Write(true);
            this.clkPort.Write(true);
            this.dataPort.Write(false);
            this.clkPort.Write(false);
            this.clkPort.Write(true);
            this.dataPort.Write(true);
            this.clkPort.Write(false);
        }

        /// <summary>
        /// Send a command to the sensor
        /// </summary>
        /// <param name="cmd">Command to send</param>
        /// <returns>ACK received from sensor</returns>
        private bool SendCommand(byte cmd)
        {
            byte send = (byte)(ADDR | cmd);

            if (!this.dataPort.Active)
                // set data port as output
                this.dataPort.Active = true;

            // trasmission start sequence
            this.StartTransmission();

            return this.WriteByte(send);
        }

        /// <summary>
        /// Read a byte from sensor
        /// </summary>
        /// <param name="sendAck">Send ACK or not</param>
        /// <returns>Byte read from sensor</returns>
        private byte ReadByte(bool sendAck)
        {
            byte resp = 0x00;

            if (this.dataPort.Active)
                // set data port as input
                this.dataPort.Active = false;

            // read bits from MSB
            for (int i = 7; i >= 0; i--)
            {
                this.clkPort.Write(true);
                resp |= (this.dataPort.Read()) ? (byte)(1 << i) : (byte)0;
                this.clkPort.Write(false);
            }

            // set data port as output
            this.dataPort.Active = true;
            // send or not ACK
            this.dataPort.Write(!sendAck);
            this.clkPort.Write(true);
            this.clkPort.Write(false);
            this.dataPort.Write(true);

            return resp;
        }

        /// <summary>
        /// Write a byte to the sensor
        /// </summary>
        /// <param name="data">Byte data to send</param>
        /// <returns>ACK received from sensor</returns>
        private bool WriteByte(byte data)
        {
            if (!this.dataPort.Active)
                // set data port as output
                this.dataPort.Active = true;

            byte b;
            // shift out bits from MSB
            for (int i = 7; i >= 0; i--)
            {
                b = (byte)(data & (1 << i));

                this.dataPort.Write(b != 0);
                this.clkPort.Write(true);
                //this.dataPort.Write(true);
                this.clkPort.Write(false);
            }

            // set data port as input
            this.dataPort.Active = false;
            // wait for ACK (data pin LOW in input)
            this.clkPort.Write(true);
            bool ack = this.dataPort.Read();
            this.clkPort.Write(false);

            return ack;
        }

        /// <summary>
        /// Read bits temperature
        /// </summary>
        /// <returns>Bits temperature</returns>
        public int ReadTemperatureRaw()
        {
            this.SendCommand(CMD_MEASURE_TEMP);

            if (this.dataPort.Active)
                // set data port as input
                this.dataPort.Active = false;

            // while sensor is measuring temp, waiting for LOW on data pin (measure finished)
            while (this.dataPort.Read()) 
                Thread.Sleep(10);

            int resp = 0;
            resp |= this.ReadByte(true);

            resp = resp << 8;
            resp |= this.ReadByte(false);

            return resp;
        }

        /// <summary>
        /// Read bits relative humidity
        /// </summary>
        /// <returns>Bits relative humidity</returns>
        public int ReadRelativeHumidityRaw()
        {
            this.SendCommand(CMD_MEASURE_RH);

            if (this.dataPort.Active)
                // set data port as input
                this.dataPort.Active = false;

            // while sensor is measuring temp, waiting for LOW on data pin (measure finished)
            while (this.dataPort.Read()) 
                Thread.Sleep(10);

            int resp = 0;
            resp |= this.ReadByte(true);

            resp = resp << 8;
            resp |= this.ReadByte(false);

            return resp;
        }

        /// <summary>
        /// Execute a reset on connection with sensor
        /// </summary>
        public void ResetConnection()
        {
            if (!this.dataPort.Active)
                // set data port as output
                this.dataPort.Active = true;

            this.dataPort.Write(true);
            // toggle 9 times clock signal (see datasheet)
            for (int i = 0; i < 10; i++)
            {
                this.clkPort.Write(true);
                this.clkPort.Write(false);
            }
        }

        /// <summary>
        /// Read the status register
        /// </summary>
        /// <returns>Content of status register</returns>
        public byte ReadStatusRegister()
        {
            this.SendCommand(CMD_READ_STATUS_REG);

            byte status = this.ReadByte(false);
            return status;
        }

        /// <summary>
        /// Write the status register
        /// </summary>
        /// <param name="status">Content of status register to write</param>
        /// <returns>Ack received or not</returns>
        public bool WriteStatusRegister(byte status)
        {
            this.SendCommand(CMD_WRITE_STATUS_REG);
            return this.WriteByte(status);
        }

        /// <summary>
        /// Read temperature from sensor
        /// </summary>
        /// <param name="unit">Temperature unit (°C or °F)</param>
        /// <returns>Temperature value</returns>
        public double ReadTemperature(TempUnit unit)
        {
            double temp;
            double d1 = 0, d2 = 0;
            // T = d1 + d2 * SOt

            // determinate d1, based on temperature unit and voltage
            if (unit == TempUnit.Celsius)
            {
                switch (this.voltage)
                {
                    case Voltage.Vdd_5V:
                        d1 = D1_VDD5V_C;
                        break;
                    case Voltage.Vdd_4V:
                        d1 = D1_VDD4V_C;
                        break;
                    case Voltage.Vdd_3_5V:
                        d1 = D1_VDD3_5V_C;
                        break;
                    case Voltage.Vdd_3V:
                        d1 = D1_VDD3V_C;
                        break;
                    case Voltage.Vdd_2_5V:
                        d1 = D1_VDD2_5V_C;
                        break;
                    default:
                        break;
                }
            }
            else if (unit == TempUnit.Farenheit)
            {
                switch (this.voltage)
                {
                    case Voltage.Vdd_5V:
                        d1 = D1_VDD5V_F;
                        break;
                    case Voltage.Vdd_4V:
                        d1 = D1_VDD4V_F;
                        break;
                    case Voltage.Vdd_3_5V:
                        d1 = D1_VDD3_5V_F;
                        break;
                    case Voltage.Vdd_3V:
                        d1 = D1_VDD3V_F;
                        break;
                    case Voltage.Vdd_2_5V:
                        d1 = D1_VDD2_5V_F;
                        break;
                    default:
                        break;
                }
            }

            // determinate d2, based on temperature unit and bit resolution
            byte status = this.ReadStatusRegister();
            
            // less resolution : 8bit RH, 12bit Temp
            if ((status & LESS_RESOLUTION) != 0x00)
            {
                if (unit == TempUnit.Celsius)
                    d2 = D2_12BIT_C;
                else if (unit == TempUnit.Farenheit)
                    d2 = D2_12BIT_F;
            }
            // high resolution : 12bit RH, 14bit Temp
            else
            {
                if (unit == TempUnit.Celsius)
                    d2 = D2_14BIT_C;
                else if (unit == TempUnit.Farenheit)
                    d2 = D2_14BIT_F;
            }

            temp = d1 + d2 * this.ReadTemperatureRaw();

            return temp;
        }

        /// <summary>
        /// Read relative humidity from sensor
        /// </summary>
        /// <returns>Relative humidity</returns>
        public double ReadRelativeHumidity()
        {
            double c1 = 0, c2 = 0, c3 = 0;
            double t1 = 0, t2 = 0;

            byte status = this.ReadStatusRegister();

            // less resolution, 8 bit
            if ((status & LESS_RESOLUTION) != 0x00)
            {
                c1 = C1_8BIT;
                c2 = C2_8BIT;
                c3 = C3_8BIT;
                t1 = T1_8BIT;
                t2 = T2_8BIT;
            }
            // high resolution, 12 bit
            else
            {
                c1 = C1_12BIT;
                c2 = C2_12BIT;
                c3 = C3_12BIT;
                t1 = T1_12BIT;
                t2 = T2_12BIT;
            }

            double SOrh = this.ReadRelativeHumidityRaw();

            double RHlinear = c1 + (c2 * SOrh) + (c3 * (SOrh * SOrh));

            // read temperature for compensation
            double temp = this.ReadTemperature(TempUnit.Celsius);

            double RH = ((temp - 25.0F) * (t1 + (t2 * SOrh))) + RHlinear;

            return RH;
        }

        /// <summary>
        /// Execute a soft reset
        /// </summary>
        /// <returns>ACK for the command</returns>
        public bool SoftReset()
        {
            bool ack = this.SendCommand(CMD_SOFT_RESET);

            // wait minimum 11 ms (datasheet)
            Thread.Sleep(20);

            return ack;
        }

        /// <summary>
        /// Temperature units
        /// </summary>
        public enum TempUnit
        {
            Celsius,
            Farenheit
        }

        /// <summary>
        /// Voltage powering sensor
        /// </summary>
        public enum Voltage
        {
            Vdd_5V,
            Vdd_4V,
            Vdd_3_5V,
            Vdd_3V,
            Vdd_2_5V
        }
    }
}
