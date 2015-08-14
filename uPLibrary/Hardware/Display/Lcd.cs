using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.Threading;
using System.Text;

namespace uPLibrary.Hardware.Display
{
    /// <summary>
    /// Lcd class for compatible HD44780U controller LCD display
    /// </summary>
    public class Lcd
    {
        #region Constants...

        // commands
        private const byte CLEAR_DISPLAY = 0x01;
        private const byte RETURN_HOME = 0x02;
        private const byte ENTRY_MODE_SET = 0x04;
        private const byte DISPLAY_CONTROL = 0x08;
        private const byte SHIFT_MOVE = 0x10;
        private const byte FUNCTION_SET = 0x20;
        private const byte SET_CGRAM_ADDRESS = 0x40;
        private const byte SET_DDRAM_ADDRESS = 0x80;

        // Entry Mode Set flags

        // cursor direction
        private const byte CURSOR_DIRECTION_INCREMENT = 0x02;
        private const byte CURSOR_DIRECTION_DECREMENT = 0x00;
        // display shifting
        private const byte DISPLAY_SHIFT_ENABLE = 0x01;
        private const byte DISPLAY_SHIFT_DISABLE = 0x00;

        // Display On/Off Control flags

        // blinking of cursor
        private const byte BLINK_CURSOR_ENABLE = 0x01;
        private const byte BLINK_CURSOR_DISABLE = 0x00;
        // cursor status (position character)
        private const byte CURSOR_CHAR_ENABLE = 0x02;
        private const byte CURSOR_CHAR_DISABLE = 0x00;
        // display status (on/off)
        private const byte DISPLAY_STATUS_ON = 0x04;
        private const byte DISPLAY_STATUS_OFF = 0x00;

        // Cursor or display shift flags

        // shift display or move cursor
        private const byte SHIFT_DISPLAY = 0x08;
        private const byte MOVE_CURSOR = 0x00;

        // shift/move direction
        private const byte SHIFT_RIGHT = 0x04;
        private const byte SHIFT_LEFT = 0x00;

        // Function set flags

        // character font
        private const byte CHAR_FONT_5x10DOTS = 0x04;
        private const byte CHAR_FONT_5x8DOTS = 0x00;

        // number of display lines
        private const byte DISPLAY_LINES_2LINE = 0x08;
        private const byte DISPLAY_LINES_1LINE = 0x00;

        // interface data length
        private const byte DATA_LENGTH_8BIT = 0x10;
        private const byte DATA_LENGTH_4BIT = 0x00;
            
        #endregion

        #region Fields...

        // provider for transferring data to LCD
        private ILcdTransferProvider provider;

        // number of display lines
        private byte lines;
        // number of characters per lines (columns)
        private byte columns;
        // command for setting display
        private byte functionSet;

        // show cursor on display
        private bool showCursor;
        // blinking cursor position character
        private bool blinkCursor;
        // display status (on/off)
        private bool displayOn;

        #endregion

        #region Properties...

        /// <summary>
        /// Show cursor on display
        /// </summary>
        public bool ShowCursor
        {
            get { return this.showCursor; }
            set
            {
                if (this.showCursor != value)
                {
                    this.showCursor = value;
                    this.DisplayControl();
                }
            }
        }

        /// <summary>
        /// Blinking cursor position character
        /// </summary>
        public bool BlinkCursor
        {
            get { return this.blinkCursor; }
            set
            {
                if (this.blinkCursor != value)
                {
                    this.blinkCursor = value;
                    this.DisplayControl();
                }
            }
        }

        /// <summary>
        /// Display status (on/off)
        /// </summary>
        public bool DisplayOn
        {
            get { return this.displayOn; }
            set
            {
                if (this.displayOn != value)
                {
                    this.displayOn = value;
                    this.DisplayControl();
                }
            }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="provider">LCD Transfer Provider to use for transferring data to LCD</param>
        public Lcd(ILcdTransferProvider provider)
        {
            if (provider == null)
                throw new ArgumentException("LCD Transfer Provider is necessary");
            this.provider = provider;

            // default set : display 1 line and character font 5 x 8 dots
            this.functionSet = DISPLAY_LINES_1LINE | CHAR_FONT_5x8DOTS;
            // set 4 or 8 bits interface data mode
            this.functionSet |= (provider.InterfaceDataMode == InterfaceDataMode._8Bit) ? DATA_LENGTH_8BIT : DATA_LENGTH_4BIT;

            this.lines = 1;
            this.columns = 16;

            // initialize default 1 x 16 display with 5x8 dots charanters
            this.Initialize(CharacterFont.Dots5x8, 1, 16);
        }

        #region Low level commands...

        /// <summary>
        /// Send command to Instruction Register
        /// </summary>
        /// <param name="command">Command byte to send</param>
        private void SendCommand(byte command)
        {
            this.provider.Send(command, false);
        }

        /// <summary>
        /// Send one data byte to display
        /// </summary>
        /// <param name="data">Data byte to send</param>
        private void SendData(byte data)
        {
            this.provider.Send(data, true);
        }

        /// <summary>
        /// Send a specified number of data bytes to display
        /// </summary>
        /// <param name="data">Buffer with data bytes</param>
        /// <param name="offset">Offset at which begin sending buffer bytes</param>
        /// <param name="count">Number of data bytes to send</param>
        private void SendData(byte[] data, int offset, int count)
        {
            int length = offset + count;
            for (int i = offset; i < length; i++)
            {
                SendData(data[i]);
            }
        }

        #endregion

        /// <summary>
        /// Execute display initialization
        /// </summary>
        /// <param name="characterFont">Character font</param>
        /// <param name="lines">Number of display lines</param>
        /// <param name="columns">Number of characters per lines (columns)</param>
        public void Initialize(CharacterFont characterFont, byte lines, byte columns)
        {
            // change default set (1 line) to 2 lines if requested
            if (lines > 1)
                this.functionSet |= DISPLAY_LINES_2LINE;

            this.lines = lines;
            this.columns = columns;

            // change default set (5 x 8 dots) to 5 x 10 dots if requested
            if (characterFont == CharacterFont.Dots5x10)
                this.functionSet |= CHAR_FONT_5x10DOTS;

            // Wait for more than 40 ms after VCC rises to 2.7 V
            // (Wait for more than 15 ms after VCC rises to 4.5 V)
            Thread.Sleep(50);

            // initialization (datasheet pag. 45 fig. 23)
            if (this.provider.InterfaceDataMode == InterfaceDataMode._8Bit)
            {
                this.SendCommand((byte)(FUNCTION_SET | this.functionSet));
                // Wait for more than 4.1 ms
                Thread.Sleep(5);

                this.SendCommand((byte)(FUNCTION_SET | this.functionSet));
                // Wait for more than 100 μs
                Thread.Sleep(1);

                this.SendCommand((byte)(FUNCTION_SET | this.functionSet));
            }
            // initialization (datasheet pag. 46 fig. 24)
            else
            {
                this.SendCommand(FUNCTION_SET | DATA_LENGTH_8BIT);
                // Wait for more than 4.1 ms
                Thread.Sleep(5);

                this.SendCommand(FUNCTION_SET | DATA_LENGTH_8BIT);
                // Wait for more than 4.1 ms
                Thread.Sleep(5);

                this.SendCommand(FUNCTION_SET | DATA_LENGTH_8BIT);
                // Wait for more than 100 μs
                Thread.Sleep(1);

                this.SendCommand(FUNCTION_SET | DATA_LENGTH_4BIT);
            }

            this.SendCommand((byte)(FUNCTION_SET | this.functionSet));
            
            // Display control
            this.blinkCursor = false;
            this.showCursor = false;
            this.displayOn = true;
            this.DisplayControl();
            // Display clear
            this.Clear();
            // Entry mode set
            this.EntryModeSet(DisplayShift.No, CursorDirection.Increment);
        }

        /// <summary>
        /// Clears entire display and sets DDRAM address 0 in Address Counter (AC)
        /// </summary>
        public void Clear()
        {
            //  DB7 DB6 DB5 DB4 DB3 DB2 DB1 DB0
            //  0   0   0   0   0   0   0   1

            this.SendCommand(CLEAR_DISPLAY);
            Thread.Sleep(2);
        }

        /// <summary>
        /// Sets DDRAM address 0 in Address Counter (AC). Also returns display from being
        /// shifted to original position. DDRAM contents remain unchanged.
        /// </summary>
        public void ReturnHome()
        {
            //  DB7 DB6 DB5 DB4 DB3 DB2 DB1 DB0
            //  0   0   0   0   0   0   1   -

            this.SendCommand(RETURN_HOME);
            Thread.Sleep(2); // 1,52 ms (datasheet)
        }

        /// <summary>
        /// Send a text to display
        /// </summary>
        /// <param name="text">Text to display</param>
        public void Write(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            this.SendData(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Sets cursor move direction and specifies display shift. These operations are
        /// performed during data write and read.
        /// </summary>
        /// <param name="displayShift">Enable or disable display shifting</param>
        /// <param name="cursorDirection">Cursor move direction</param>
        public void EntryModeSet(DisplayShift displayShift, CursorDirection cursorDirection)
        {
            //  DB7 DB6 DB5 DB4 DB3 DB2 DB1 DB0
            //  0   0   0   0   0   1   I/D S

            byte entryModeCmd = ENTRY_MODE_SET;

            // if requested display shifting
            if (displayShift == DisplayShift.Yes)
                entryModeCmd |= DISPLAY_SHIFT_ENABLE;

            // cursor direction if display shifting enabled
            if (cursorDirection == CursorDirection.Increment)
                entryModeCmd |= CURSOR_DIRECTION_INCREMENT;

            this.SendCommand(entryModeCmd); // 37 us (datasheet)
        }

        /// <summary>
        /// Sets entire display (D) on/off, cursor on/off (C), 
        /// and blinking of cursor position character (B).
        /// </summary>
        /// <param name="cursorBlinking">Blinking of cursor</param>
        /// <param name="cursorChar">Cursor position char</param>
        /// <param name="displayStatus">Display Status On/Off</param>
        private void DisplayControl(CursorBlinking cursorBlinking, CursorChar cursorChar, DisplayStatus displayStatus)
        {
            //  DB7 DB6 DB5 DB4 DB3 DB2 DB1 DB0
            //  0   0   0   0   1   D   C   B

            byte displayCtrlCmd = DISPLAY_CONTROL;

            // blinking of cursor
            if (cursorBlinking == CursorBlinking.Yes)
                displayCtrlCmd |= BLINK_CURSOR_ENABLE;
            this.blinkCursor = (cursorBlinking == CursorBlinking.Yes);

            // display cursor position char
            if (cursorChar == CursorChar.Display)
                displayCtrlCmd |= CURSOR_CHAR_ENABLE;
            this.showCursor = (cursorChar == CursorChar.Display);

            // display status on/off
            if (displayStatus == DisplayStatus.On)
                displayCtrlCmd |= DISPLAY_STATUS_ON;
            this.displayOn = (displayStatus == DisplayStatus.On);

            this.SendCommand(displayCtrlCmd); // 37 us (datasheet)
        }

        /// <summary>
        /// Sets entire display (D) on/off, cursor on/off (C), 
        /// and blinking of cursor position character (B).
        /// </summary>
        private void DisplayControl()
        {
            //  DB7 DB6 DB5 DB4 DB3 DB2 DB1 DB0
            //  0   0   0   0   1   D   C   B

            byte displayCtrlCmd = DISPLAY_CONTROL;

            displayCtrlCmd |= (this.blinkCursor) ? BLINK_CURSOR_ENABLE : BLINK_CURSOR_DISABLE;
            displayCtrlCmd |= (this.showCursor) ? CURSOR_CHAR_ENABLE : CURSOR_CHAR_DISABLE;
            displayCtrlCmd |= (this.displayOn) ? DISPLAY_STATUS_ON : DISPLAY_STATUS_OFF;

            this.SendCommand(displayCtrlCmd); // 37 us (datasheet)
        }

        /// <summary>
        /// Moves cursor and shifts display without changing DDRAM contents.
        /// </summary>
        /// <param name="shiftOperation">Shift display or cursor move</param>
        /// <param name="shiftDirection">Shift display or cursor move direction</param>
        private void Shift(ShiftOperation shiftOperation, ShiftDirection shiftDirection)
        {
            //  DB7 DB6 DB5 DB4 DB3 DB2 DB1 DB0
            //  0   0   0   1   SC  RL  -   -

            byte shiftCmd = SHIFT_MOVE;

            // display shift
            if (shiftOperation == ShiftOperation.DisplayShift)
                shiftCmd |= SHIFT_DISPLAY;

            // right direction
            if (shiftDirection == ShiftDirection.Right)
                shiftCmd |= SHIFT_RIGHT;

            this.SendCommand(shiftCmd); // 37 us (datasheet)
        }

        /// <summary>
        /// Shift display without changing DDRAM contents.
        /// </summary>
        /// <param name="direction">Shift display direction</param>
        public void ShiftDisplay(ShiftDirection direction)
        {
            this.Shift(ShiftOperation.DisplayShift, direction);
        }

        /// <summary>
        /// Move cursor without changing DDRAM contents.
        /// </summary>
        /// <param name="direction">Cursor move direction</param>
        public void MoveCursor(ShiftDirection direction)
        {
            this.Shift(ShiftOperation.CursorMove, direction);
        }

        /// <summary>
        /// Sets interface data length (DL), number of display lines (N), and character font (F).
        /// </summary>
        /// <param name="characterFont">Character font</param>
        /// <param name="lines">Number of display lines</param>
        /// <param name="dataLength">Interface data length</param>
        private void FunctionSet(CharacterFont characterFont, byte lines, DataLength dataLength)
        {
            //  DB7 DB6 DB5 DB4 DB3 DB2 DB1 DB0
            //  0   0   1   DL  N   F   -   -

            byte funcSetCmd = FUNCTION_SET;

            // char font 5x10 dots
            if (characterFont == CharacterFont.Dots5x10)
                funcSetCmd |= CHAR_FONT_5x10DOTS;

            // max two display lines
            funcSetCmd |= (lines > 1) ? DISPLAY_LINES_2LINE : DISPLAY_LINES_1LINE;

            // 8 bit data length
            if (dataLength == DataLength.DataBit8)
                funcSetCmd |= DATA_LENGTH_8BIT;

            this.SendCommand(funcSetCmd); // 37 us (datasheet)
        }

        /// <summary>
        /// Set the curson position
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="column">Column</param>
        public void SetCursorPosition(int row, int column)
        {
            // change column value to zero-based
            column -= 1;

            int address = (row == 2) ? column + 0x40 : column;
            this.SendCommand((byte)(SET_DDRAM_ADDRESS | address));
        }
    }

    /// <summary>
    /// Cursor move direction
    /// </summary>
    public enum CursorDirection
    {
        /// <summary>
        /// Move cursor to left
        /// (Decrements the DDRAM address by 1 when a character code is written into or read from DDRAM)
        /// </summary>
        Decrement = 0,

        /// <summary>
        /// Move cursor to right
        /// (Increments the DDRAM address by 1 when a character code is written into or read from DDRAM)
        /// </summary>
        Increment = 1
    }

    /// <summary>
    /// Display shift
    /// </summary>
    public enum DisplayShift
    {
        /// <summary>
        /// Display doesn't shift
        /// </summary>
        No = 0,

        /// <summary>
        /// Shifts the entire display either to the right (CursorDirection = Decrement) 
        /// or to the left (CursorDirection = Increment) 
        /// </summary>
        Yes = 1
    }

    /// <summary>
    /// Display status
    /// </summary>
    public enum DisplayStatus
    {
        /// <summary>
        /// Off
        /// </summary>
        Off = 0,

        /// <summary>
        /// On
        /// </summary>
        On = 1
    }

    /// <summary>
    /// Cursor char status
    /// </summary>
    public enum CursorChar
    {
        /// <summary>
        /// The cursor is not displayed
        /// </summary>
        Hide = 0,

        /// <summary>
        /// The cursor is displayed
        /// </summary>
        Display = 1
    }

    /// <summary>
    /// Blink cursor status
    /// </summary>
    public enum CursorBlinking
    {
        /// <summary>
        /// The cursor doesn't blink
        /// </summary>
        No = 0,

        /// <summary>
        /// The cursor blinks
        /// </summary>
        Yes = 1
    }

    /// <summary>
    /// Shift operation object
    /// </summary>
    public enum ShiftOperation
    {
        /// <summary>
        /// Move cursor
        /// </summary>
        CursorMove = 0,

        /// <summary>
        /// Display shift
        /// </summary>
        DisplayShift = 1
    }

    /// <summary>
    /// Shift direction
    /// </summary>
    public enum ShiftDirection
    {
        /// <summary>
        /// Shift to left
        /// </summary>
        Left = 0,

        /// <summary>
        /// Shift to right
        /// </summary>
        Right = 1
    }

    /// <summary>
    /// Interface data length
    /// </summary>
    public enum DataLength
    {
        /// <summary>
        /// 4 bit interface
        /// </summary>
        DataBit4 = 0,

        /// <summary>
        /// 8 bit interface
        /// </summary>
        DataBit8 = 1
    }

    /// <summary>
    /// Character font (in Dots)
    /// </summary>
    public enum CharacterFont
    {
        /// <summary>
        /// 5 x 8 Dots
        /// </summary>
        Dots5x8 = 0,

        /// <summary>
        /// 5 x 10 Dots
        /// </summary>
        Dots5x10 = 1
    }
}
