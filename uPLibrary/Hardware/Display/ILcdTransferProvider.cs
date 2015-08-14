using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace uPLibrary.Hardware.Display
{
    /// <summary>
    /// Interface for LCD transfer provider
    /// </summary>
    public interface ILcdTransferProvider
    {
        /// <summary>
        /// Send a command to the LCD
        /// </summary>
        /// <param name="value">Value byte (bit from DB0 to DB7) to send</param>
        /// <param name="rsMode">Register Select mode (false = command, true = data)</param>
        void Send(byte value, bool rsMode);

        /// <summary>
        /// Specify 8-bit or 4-bit mode for the provider
        /// </summary>
        InterfaceDataMode InterfaceDataMode { get; }
    }

    /// <summary>
    /// Interface data mode
    /// </summary>
    public enum InterfaceDataMode
    {
        /// <summary>
        /// 4-bit interface mode
        /// </summary>
        _4Bit,

        /// <summary>
        /// 8-bit interface mode
        /// </summary>
        _8Bit
    }
}
