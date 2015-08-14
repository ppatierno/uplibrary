using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace uPLibrary.Hardware.Display
{
    /// <summary>
    /// Provider for communication with LCD vid GPIO pins
    /// </summary>
    public class GpioLcdTransferProvider : ILcdTransferProvider
    {
        #region Constants...

        // data bus dimension
        private const int DATA_LINE = 8;

        #endregion

        #region Fields...

        // ports
        private readonly OutputPort rsPort;         // Register Select (0 : Intruction Register (write) / Busy flag (read), 1 : Data Register (write/read))
        private readonly OutputPort rwPort;         // Select Read/Write (0: Write, 1 : Read)
        private readonly OutputPort enablePort;     // Enable
        private readonly OutputPort[] dbPorts;      // Data Bus

        // interface data mode
        private InterfaceDataMode interfaceDataMode;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="interfaceDataMode">Interface data mode</param>
        /// <param name="rs">Pin Register Select</param>
        /// <param name="rw">Pin Select Read/Write</param>
        /// <param name="enable">Pin Enable</param>
        /// <param name="db0">Pin Data Bus Line 0</param>
        /// <param name="db1">Pin Data Bus Line 1</param>
        /// <param name="db2">Pin Data Bus Line 2</param>
        /// <param name="db3">Pin Data Bus Line 3</param>
        /// <param name="db4">Pin Data Bus Line 4</param>
        /// <param name="db5">Pin Data Bus Line 5</param>
        /// <param name="db6">Pin Data Bus Line 6</param>
        /// <param name="db7">Pin Data Bus Line 7</param>
        public GpioLcdTransferProvider(InterfaceDataMode interfaceDataMode, Cpu.Pin rs, Cpu.Pin rw, Cpu.Pin enable, Cpu.Pin db0, Cpu.Pin db1, Cpu.Pin db2, Cpu.Pin db3,
            Cpu.Pin db4, Cpu.Pin db5, Cpu.Pin db6, Cpu.Pin db7)
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

            // create ports for data bus lines
            Cpu.Pin[] dbPins = { db0, db1, db2, db3, db4, db5, db6, db7 };
            this.dbPorts = new OutputPort[DATA_LINE];
            for (int i = 0; i < DATA_LINE; i++)
            {
                if (dbPins[i] != Cpu.Pin.GPIO_NONE)
                    this.dbPorts[i] = new OutputPort(dbPins[i], false);
            }
        }

        /// <summary>
        /// Constructor with default 8 bit interface mode
        /// </summary>
        public GpioLcdTransferProvider(Cpu.Pin rs, Cpu.Pin rw, Cpu.Pin enable, Cpu.Pin db0, Cpu.Pin db1, Cpu.Pin db2, Cpu.Pin db3,
            Cpu.Pin db4, Cpu.Pin db5, Cpu.Pin db6, Cpu.Pin db7) 
            : this(InterfaceDataMode._8Bit, rs, rw, enable, db0, db1, db2, db3, db4, db5, db6, db7)
        {
        }

        /// <summary>
        /// Constructor with default 4 bit interface mode
        /// </summary>
        public GpioLcdTransferProvider(Cpu.Pin rs, Cpu.Pin rw, Cpu.Pin enable, Cpu.Pin db4, Cpu.Pin db5, Cpu.Pin db6, Cpu.Pin db7)
            : this(InterfaceDataMode._4Bit, rs, rw, enable, Cpu.Pin.GPIO_NONE, Cpu.Pin.GPIO_NONE, Cpu.Pin.GPIO_NONE, Cpu.Pin.GPIO_NONE, db4, db5, db6, db7)
        {
        }

        #region ILcdTransferProvider interface...

        public InterfaceDataMode InterfaceDataMode
        {
            get { return this.interfaceDataMode; }
        }

        public void Send(byte value, bool rsMode)
        {
            this.rsPort.Write(rsMode);

            if (this.rwPort != null)
                this.rwPort.Write(false);

            // 8 bit interface mode
            if (this.interfaceDataMode == InterfaceDataMode._8Bit)
            {
                // send to each data bus line the corrisponding bit inside data byte
                for (int i = 0; i < DATA_LINE; i++)
                {
                    // send from lsb to msb
                    this.dbPorts[i].Write(((value >> i) & 0x01) == 0x01);
                }
            }
            // 4 bit interface mode
            else
            {
                // get and send higher 4 bit
                byte nibble = (byte)((value >> 4) & 0x0F);
                for (int i = 0; i < DATA_LINE / 2; i++)
                {
                    this.dbPorts[DATA_LINE / 2 + i].Write(((nibble >> i) & 0x01) == 0x01);
                }

                // pulse enable
                this.Enable();

                // get and send lower 4 bit
                nibble = (byte)(value & 0x0F);
                for (int i = 0; i < DATA_LINE / 2; i++)
                {
                    this.dbPorts[DATA_LINE / 2 + i].Write(((nibble >> i) & 0x01) == 0x01);
                }
            }

            // pulse enable
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
