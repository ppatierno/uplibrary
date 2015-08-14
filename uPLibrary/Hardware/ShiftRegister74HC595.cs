using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace uPLibrary.Hardware
{
    /// <summary>
    /// Driver for shift register 74HC595
    /// </summary>
    public class ShiftRegister74HC595
    {

        #region Fields...

        private OutputPort dsPort;      // Serial Data pin
        private OutputPort shcpPort;    // Shift Register Clock pin
        private OutputPort stcpPort;    // Storage Register Clock pin
        private OutputPort mrPort;      // Master Reset pin (active LOW)
        private OutputPort oePort;      // Output Enable (active LOW)

        // bit order during transferring
        private BitOrder bitOrder;

        // spi interface
        private SPI spi;
        // spi write buffer
        private byte[] spiBuffer;

        #endregion

        #region Ctors Bit Banging Mode...

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ds">Pin Serial Data</param>
        /// <param name="shcp">Pin Shift Register Clock</param>
        /// <param name="stcp">Pin Storage Register Clock</param>
        /// <param name="mr">Pin Master Reset</param>
        /// <param name="oe">Pin Output Enable</param>
        /// <param name="bitOrder">Bit order during transferring</param>
        public ShiftRegister74HC595(Cpu.Pin ds, Cpu.Pin shcp, Cpu.Pin stcp, Cpu.Pin mr, Cpu.Pin oe, BitOrder bitOrder)
        {
            // Serial Data (DS) pin is necessary
            if (ds == Cpu.Pin.GPIO_NONE)
                throw new ArgumentException("Serial Data (DS) pin is necessary");
            this.dsPort = new OutputPort(ds, false);

            // Shift Register Clock (SHCP) pin is necessary
            if (shcp == Cpu.Pin.GPIO_NONE)
                throw new ArgumentException("Shift Register Clock (SHCP) pin is necessary");
            this.shcpPort = new OutputPort(shcp, false);

            // you can save 1 pin connecting STCP and SHCP together
            if (stcp != Cpu.Pin.GPIO_NONE)
                this.stcpPort = new OutputPort(stcp, false);

            // you can save 1 pin connecting MR pin to Vcc (no reset)
            if (mr != Cpu.Pin.GPIO_NONE)
                this.mrPort = new OutputPort(mr, true);

            // you can save 1 pin connecting OE pin to GND (shift register output pins are always enabled)
            if (oe != Cpu.Pin.GPIO_NONE)
                this.oePort = new OutputPort(oe, false);

            this.bitOrder = bitOrder;
        }


        /// <summary>
        /// Constructor without using MR and OE
        /// </summary>
        public ShiftRegister74HC595(Cpu.Pin ds, Cpu.Pin shcp, Cpu.Pin stcp, BitOrder bitOrder)
            : this(ds, shcp, stcp, Cpu.Pin.GPIO_NONE, Cpu.Pin.GPIO_NONE, bitOrder)
        {
        }

        /// <summary>
        /// Constructor without using MR and OE.
        /// SHCP and STCP are connected together
        /// </summary>
        public ShiftRegister74HC595(Cpu.Pin ds, Cpu.Pin shcp, BitOrder bitOrder)
            : this(ds, shcp, Cpu.Pin.GPIO_NONE, Cpu.Pin.GPIO_NONE, Cpu.Pin.GPIO_NONE, bitOrder)
        {
        }

        /// <summary>
        /// Constructor without using MR and OE
        /// BitOrder LSB first
        /// </summary>
        public ShiftRegister74HC595(Cpu.Pin ds, Cpu.Pin shcp, Cpu.Pin stcp)
            : this(ds, shcp, stcp, Cpu.Pin.GPIO_NONE, Cpu.Pin.GPIO_NONE, BitOrder.LSBtoMSB)
        {
        }

        /// <summary>
        /// Constructor without using MR and OE.
        /// SHCP and STCP are connected together
        /// BitOrder LSB first
        /// </summary>
        public ShiftRegister74HC595(Cpu.Pin ds, Cpu.Pin shcp)
            : this(ds, shcp, Cpu.Pin.GPIO_NONE, Cpu.Pin.GPIO_NONE, Cpu.Pin.GPIO_NONE, BitOrder.LSBtoMSB)
        {
        }

        #endregion

        #region Ctors SPI Mode...

        /// <summary>
        /// Constructor using SPI mode
        /// </summary>
        /// <param name="spiModule">SPI device module</param>
        /// <param name="stcp">Pin Storage Register Clock</param>
        /// <param name="mr">Pin Master Reset</param>
        /// <param name="oe">Pin Output Enable</param>
        /// <param name="bitOrder">Bit order during transferring</param>
        public ShiftRegister74HC595(SPI.SPI_module spiModule, Cpu.Pin stcp, Cpu.Pin mr, Cpu.Pin oe, BitOrder bitOrder)
        {
            // SPI mode :
            // MOSI connected to DS (shift register)
            // SCKL connected to SHCP (shift register)
            SPI.Configuration spiConf = new SPI.Configuration(Cpu.Pin.GPIO_NONE, // chip select not necessary, we use stcp pin (latch)
                                                              false, // active state
                                                              0, // setup time
                                                              0, // hold time
                                                              false, // clock idle state (our clock is shcp and its idle state is low)
                                                              true, // clock edge (rising)
                                                              1000, // clock rate
                                                              spiModule); // spi bus

            this.spi = new SPI(spiConf);
            this.spiBuffer = new byte[1];

            // you can save 1 pin connecting MR pin to Vcc (no reset)
            if (mr != Cpu.Pin.GPIO_NONE)
                this.mrPort = new OutputPort(mr, true);

            // you can save 1 pin connecting OE pin to GND (shift register output pins are always enabled)
            if (oe != Cpu.Pin.GPIO_NONE)
                this.oePort = new OutputPort(oe, false);

            // you can save 1 pin connecting STCP and SHCP together
            if (stcp != Cpu.Pin.GPIO_NONE)
                this.stcpPort = new OutputPort(stcp, false);

            this.bitOrder = bitOrder;
        }

        /// <summary>
        /// Constructor without using MR and OE
        /// </summary>
        public ShiftRegister74HC595(SPI.SPI_module spiModule, Cpu.Pin stcp, BitOrder bitOrder)
            : this(spiModule, stcp, Cpu.Pin.GPIO_NONE, Cpu.Pin.GPIO_NONE, bitOrder)
        {
        }

        /// <summary>
        /// Constructor without using MR and OE
        /// BitOrder MSB first
        /// </summary>
        public ShiftRegister74HC595(SPI.SPI_module spiModule, Cpu.Pin stcp)
            : this(spiModule, stcp, Cpu.Pin.GPIO_NONE, Cpu.Pin.GPIO_NONE, BitOrder.MSBtoLSB)
        {
        }

        /// <summary>
        /// Constructor without using MR and OE.
        /// SHCP and STCP are connected together
        /// </summary>
        public ShiftRegister74HC595(SPI.SPI_module spiModule, BitOrder bitOrder)
            : this(spiModule, Cpu.Pin.GPIO_NONE, Cpu.Pin.GPIO_NONE, Cpu.Pin.GPIO_NONE, bitOrder)
        {
        }

        /// <summary>
        /// Constructor without using MR and OE.
        /// SHCP and STCP are connected together
        /// BitOrder MSB first
        /// </summary>
        public ShiftRegister74HC595(SPI.SPI_module spiModule)
            : this(spiModule, Cpu.Pin.GPIO_NONE, Cpu.Pin.GPIO_NONE, Cpu.Pin.GPIO_NONE, BitOrder.MSBtoLSB)
        {
        }

        #endregion

        /// <summary>
        /// Empty shift register loaded into storage register
        /// </summary>
        public void Empty()
        {
            if (this.stcpPort != null)
                this.stcpPort.Write(false);

            // OE Low
            if (this.oePort != null)
                this.oePort.Write(false);

            // MR Low
            if (this.mrPort != null)
                this.mrPort.Write(false);

            // STCP low to high transition
            if (this.stcpPort != null)
            {
                this.stcpPort.Write(true);
                this.stcpPort.Write(false);
            }
        }

        /// <summary>
        /// Shift register clear; parallel outputs in high-impedance OFF-state
        /// </summary>
        public void Clear()
        {
            // OE Low
            if (this.oePort != null)
                this.oePort.Write(true);

            // MR Low
            if (this.mrPort != null)
                this.mrPort.Write(false);
        }

        /// <summary>
        /// Logic HIGH-level shifted into shift register stage 0. Contents of all shift register stages shifted through
        /// </summary>
        /// <param name="value">Byte to shift from DS pin into register</param>
        private void ShiftOut(byte value)
        {
            // OE Low (enable output)
            if (this.oePort != null)
                this.oePort.Write(false);

            // MR High (no reset)
            if (this.mrPort != null)
                this.mrPort.Write(true);

            // bit banging mode
            if (this.spi == null)
            {
                // the bit banging mode sends LSB first so if
                // I want MSB first I need to reverse bits
                if (this.bitOrder == BitOrder.MSBtoLSB)
                    value = this.ReverseBits(value);

                this.dsPort.Write(false);
                this.shcpPort.Write(false);

                for (int i = 0; i < 8; i++)
                {
                    this.shcpPort.Write(false);
                    // send LSB first
                    this.dsPort.Write(((value >> i) & 0x01) == 0x01);
                    this.shcpPort.Write(true);
                }

                this.shcpPort.Write(false);
            }
            // spi mode
            else
            {
                // SPI is MSB first so if I want LSB first I need to reverse bits
                if (this.bitOrder == BitOrder.LSBtoMSB)
                    value = this.ReverseBits(value);

                this.spiBuffer[0] = value;
                this.spi.Write(this.spiBuffer);
            }
        }

        /// <summary>
        /// Contents of shift register stages (internal QnS) are transferred to the storage register and parallel output stages
        /// </summary>
        /// <param name="buffer">Buffer of bytes to transfer from DS to output</param>
        public void Output(params byte[] buffer)
        {
            // OE Low (enable output)
            if (this.oePort != null)
                this.oePort.Write(false);

            // MR High (no reset)
            if (this.mrPort != null)
                this.mrPort.Write(true);

            for (int i = 0; i < buffer.Length; i++)
            {
                if (this.stcpPort != null)
                    this.stcpPort.Write(false);

                this.ShiftOut(buffer[i]);

                if (this.stcpPort != null)
                {
                    this.stcpPort.Write(true);
                    this.stcpPort.Write(false);
                }
            }
        }

        /// <summary>
        /// Reverse bits order inside a byte (MSB to LSB and viceversa)
        /// </summary>
        /// <param name="value">Byte value to reverse</param>
        /// <returns>Byte value after reverse</returns>
        private byte ReverseBits(byte value)
        {
            byte result = 0x00;

            int i = 7, j = 0;

            while (i >= 0)
            {
                result |= (byte)(((value >> i) & 0x01) << j);
                i--;
                j++;
            }
            return result;
        }
    }

    /// <summary>
    /// Bit order
    /// </summary>
    public enum BitOrder
    {
        /// <summary>
        /// From MSB to LSB
        /// </summary>
        MSBtoLSB,

        /// <summary>
        /// From LSB to MSB
        /// </summary>
        LSBtoMSB
    }
}
