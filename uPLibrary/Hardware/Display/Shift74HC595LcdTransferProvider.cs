using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using uPLibrary.Hardware;

namespace uPLibrary.Hardware.Display
{
    /// <summary>
    /// Provider for communication with LCD vid shift register NXP 74HC595
    /// </summary>
    public class Shift74HC595LcdTransferProvider : ILcdTransferProvider
    {
        #region Fields...

        private readonly OutputPort rsPort;         // Register Select (0 : Intruction Register (write) / Busy flag (read), 1 : Data Register (write/read))
        private readonly OutputPort rwPort;         // Select Read/Write (0: Write, 1 : Read)
        private readonly OutputPort enablePort;     // Enable

        // reference to shift register driver
        private ShiftRegister74HC595 shift;

        // interface data mode
        private InterfaceDataMode interfaceDataMode;

        #endregion

        #region Ctors Bit Banging Mode...

        /// <summary>
        /// Constructor using bit banging
        /// </summary>
        /// <param name="interfaceDataMode">Interface data mode</param>
        /// <param name="rs">Pin Register Select</param>
        /// <param name="rw">Pin Select Read/Write</param>
        /// <param name="enable">Pin Enable</param>
        /// <param name="ds">Pin Serial Data</param>
        /// <param name="shcp">Pin Shift Register Clock</param>
        /// <param name="stcp">Pin Storage Register Clock</param>
        /// <param name="mr">Pin Master Reset</param>
        /// <param name="oe">Pin Output Enable</param>
        /// <param name="bitOrder">Bit order during transferring</param>
        public Shift74HC595LcdTransferProvider(InterfaceDataMode interfaceDataMode, Cpu.Pin rs, Cpu.Pin rw, Cpu.Pin enable,
            Cpu.Pin ds, Cpu.Pin shcp, Cpu.Pin stcp, Cpu.Pin mr, Cpu.Pin oe, BitOrder bitOrder)
        {
            // set interface data mode (4 or 8 bits)
            this.interfaceDataMode = interfaceDataMode;

            // Register Select (rs) pin is necessary
            if (rs == Cpu.Pin.GPIO_NONE)
                throw new ArgumentException("Register Select (RS) pin is necessary");
            this.rsPort = new OutputPort(rs, false);

            // you can save 1 pin setting from your board, wiring RW pin to GND for write only operation
            if (rw != Cpu.Pin.GPIO_NONE)
                this.rwPort = new OutputPort(rw, false);

            // Enable (enable) pin is necessary
            if (enable == Cpu.Pin.GPIO_NONE)
                throw new ArgumentException("Enable (EN) signal pin is necessary");
            this.enablePort = new OutputPort(enable, false);

            this.shift = new ShiftRegister74HC595(ds, shcp, stcp, mr, oe, bitOrder);
        }

        /// <summary>
        /// Constructor without using MR and OE
        /// </summary>
        public Shift74HC595LcdTransferProvider(InterfaceDataMode interfaceDataMode, Cpu.Pin rs, Cpu.Pin rw, Cpu.Pin enable, Cpu.Pin ds, Cpu.Pin shcp, Cpu.Pin stcp, BitOrder bitOrder)
            : this(interfaceDataMode, rs, rw, enable, ds, shcp, stcp, Cpu.Pin.GPIO_NONE, Cpu.Pin.GPIO_NONE, bitOrder)
        {
        }

        /// <summary>
        /// Constructor without using MR and OE
        /// BitOrder LSB first
        /// </summary>
        public Shift74HC595LcdTransferProvider(InterfaceDataMode interfaceDataMode, Cpu.Pin rs, Cpu.Pin rw, Cpu.Pin enable, Cpu.Pin ds, Cpu.Pin shcp, Cpu.Pin stcp)
            : this(interfaceDataMode, rs, rw, enable, ds, shcp, stcp, Cpu.Pin.GPIO_NONE, Cpu.Pin.GPIO_NONE, BitOrder.LSBtoMSB)
        {
        }

        #endregion

        #region Ctors SPI Mode...

        /// <summary>
        /// Constructor using SPI mode
        /// </summary>
        /// <param name="interfaceDataMode">Interface data mode</param>
        /// <param name="rs">Pin Register Select</param>
        /// <param name="rw">Pin Select Read/Write</param>
        /// <param name="enable">Pin Enable</param>
        /// <param name="spiModule">SPI device module</param>
        /// <param name="stcp">Pin Storage Register Clock</param>
        /// <param name="mr">Pin Master Reset</param>
        /// <param name="oe">Pin Output Enable</param>
        /// <param name="bitOrder">Bit order during transferring</param>
        public Shift74HC595LcdTransferProvider(InterfaceDataMode interfaceDataMode, Cpu.Pin rs, Cpu.Pin rw, Cpu.Pin enable, SPI.SPI_module spiModule, Cpu.Pin stcp, Cpu.Pin mr, Cpu.Pin oe, BitOrder bitOrder)
        {
            // set interface data mode (4 or 8 bits)
            this.interfaceDataMode = interfaceDataMode;

            // Register Select (rs) pin is necessary
            if (rs == Cpu.Pin.GPIO_NONE)
                throw new ArgumentException("Register Select (RS) pin is necessary");
            this.rsPort = new OutputPort(rs, false);

            // you can save 1 pin setting from your board, wiring RW pin to GND for write only operation
            if (rw != Cpu.Pin.GPIO_NONE)
                this.rwPort = new OutputPort(rw, false);

            // Enable (enable) pin is necessary
            if (enable == Cpu.Pin.GPIO_NONE)
                throw new ArgumentException("Enable (EN) signal pin is necessary");
            this.enablePort = new OutputPort(enable, false);

            this.shift = new ShiftRegister74HC595(spiModule, stcp, mr, oe, bitOrder);
        }

        /// <summary>
        /// Constructor without using MR and OE
        /// </summary>
        public Shift74HC595LcdTransferProvider(InterfaceDataMode interfaceDataMode, Cpu.Pin rs, Cpu.Pin rw, Cpu.Pin enable, SPI.SPI_module spiModule, Cpu.Pin stcp, BitOrder bitOrder)
            : this(interfaceDataMode, rs, rw, enable, spiModule, stcp, Cpu.Pin.GPIO_NONE, Cpu.Pin.GPIO_NONE, bitOrder)
        {
        }

        /// <summary>
        /// Constructor without using MR and OE
        /// BitOrder LSB first
        /// </summary>
        public Shift74HC595LcdTransferProvider(InterfaceDataMode interfaceDataMode, Cpu.Pin rs, Cpu.Pin rw, Cpu.Pin enable, SPI.SPI_module spiModule, Cpu.Pin stcp)
            : this(interfaceDataMode, rs, rw, enable, spiModule, stcp, Cpu.Pin.GPIO_NONE, Cpu.Pin.GPIO_NONE, BitOrder.LSBtoMSB)
        {
        }

        #endregion

        #region ILcdTransferProvider interface...

        public InterfaceDataMode InterfaceDataMode
        {
            get { return this.interfaceDataMode; }
        }

        public void Send(byte value, bool rsMode)
        {
            this.rsPort.Write(rsMode);

            this.shift.Output(value);

            this.Enable();
        }

        #endregion

        /// <summary>
        /// Move enable signal
        /// </summary>
        private void Enable()
        {
            this.enablePort.Write(true);
            this.enablePort.Write(false);
            this.enablePort.Write(true);
        }
    }
}
