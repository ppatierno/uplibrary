using System;

namespace uPLibrary.Utilities
{
    /// <summary>
    /// Encoder class
    /// </summary>
    public static class Encoder
    {
        // extracted from full .Net Framework
        #region Base64 Encode/Decode

        private const int CB_B64_OUT_TRIO = 3;
        private const int CCH_B64_IN_QUARTET = 4;

        private static byte[] s_rgbBase64Decode = new byte[] { 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 
            0, 0x3e, 0, 0, 0, 0, 0, 0, 0, 0, 0x3f, 0x3e, 0, 0, 0, 0x3f, 
            0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x3b, 60, 0x3d, 0, 0, 0, 0, 0, 0, 
            0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 
            15, 0x10, 0x11, 0x12, 0x13, 20, 0x15, 0x16, 0x17, 0x18, 0x19, 0, 0, 0, 0, 0, 
            0, 0x1a, 0x1b, 0x1c, 0x1d, 30, 0x1f, 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x27, 40, 
            0x29, 0x2a, 0x2b, 0x2c, 0x2d, 0x2e, 0x2f, 0x30, 0x31, 50, 0x33, 0, 0, 0, 0, 0
         };

        private static char[] s_rgchBase64Encoding = new char[] { 
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 
            'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 
            'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 
            'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '!', '*'
         };

        public static byte[] FromBase64CharArray(char[] inString, int offset, int length)
        {
            if (length == 0)
            {
                return new byte[0];
            }
            int inLength = length;
            if ((inLength % 4) != 0)
            {
                throw new ArgumentException("Encoded string length should be multiple of 4");
            }
            int outCurPos = ((inLength + 3) / 4) * 3;
            if (inString[(offset + inLength) - 1] == '=')
            {
                outCurPos--;
                if (inString[(offset + inLength) - 2] == '=')
                {
                    outCurPos--;
                }
            }
            byte[] retArray = new byte[outCurPos];
            byte[] rgbOutput = new byte[4];
            for (int inCurPos = offset + inLength; inCurPos > offset; inCurPos -= 4)
            {
                int ibDest = 0;
                while (ibDest < 4)
                {
                    int ichGet = (inCurPos + ibDest) - 4;
                    if (inString[ichGet] == '=')
                    {
                        if ((ibDest < 2) || (inCurPos != (offset + inLength)))
                        {
                            throw new ArgumentException("Invalid base64 encoded string");
                        }
                        break;
                    }
                    rgbOutput[ibDest] = s_rgbBase64Decode[inString[ichGet]];
                    ibDest++;
                }
                switch (ibDest)
                {
                    case 2:
                        goto Label_00E1;

                    case 3:
                        break;

                    default:
                        retArray[--outCurPos] = (byte) (((rgbOutput[2] & 3) << 6) | rgbOutput[3]);
                        break;
                }
                retArray[--outCurPos] = (byte) (((rgbOutput[1] & 15) << 4) | ((rgbOutput[2] & 60) >> 2));
            Label_00E1:
                retArray[--outCurPos] = (byte) ((rgbOutput[0] << 2) | ((rgbOutput[1] & 0x30) >> 4));
            }
            return retArray;
        }

        public static byte[] FromBase64String(string inString)
        {
            if (inString == null)
            {
                throw new ArgumentNullException();
            }
            char[] chArray = inString.ToCharArray();
            return FromBase64CharArray(chArray, 0, chArray.Length);
        }

        private static int GetBase64EncodedLength(int binaryLen)
        {
            return (((binaryLen / 3) + (((binaryLen % 3) != 0) ? 1 : 0)) * 4);
        }

        public static string ToBase64String(byte[] inArray)
        {
            return ToBase64String(inArray, 0, inArray.Length);
        }

        public static string ToBase64String(byte[] inArray, int offset, int length)
        {
            if (inArray == null)
            {
                throw new ArgumentNullException();
            }
            if (length == 0)
            {
                return "";
            }
            if ((offset + length) > inArray.Length)
            {
                throw new ArgumentOutOfRangeException();
            }
            int inArrayLen = length;
            int outArrayLen = GetBase64EncodedLength(inArrayLen);
            char[] outArray = new char[outArrayLen];
            int iInputEnd = offset + (((outArrayLen / 4) - 1) * 3);
            int iInput = offset;
            int iOutput = 0;
            byte uc0 = 0;
            byte uc1 = 0;
            byte uc2 = 0;
            while (iInput < iInputEnd)
            {
                uc0 = inArray[iInput];
                uc1 = inArray[iInput + 1];
                uc2 = inArray[iInput + 2];
                outArray[iOutput] = s_rgchBase64Encoding[uc0 >> 2];
                outArray[iOutput + 1] = s_rgchBase64Encoding[((uc0 << 4) & 0x30) | ((uc1 >> 4) & 15)];
                outArray[iOutput + 2] = s_rgchBase64Encoding[((uc1 << 2) & 60) | ((uc2 >> 6) & 3)];
                outArray[iOutput + 3] = s_rgchBase64Encoding[uc2 & 0x3f];
                iInput += 3;
                iOutput += 4;
            }
            uc0 = inArray[iInput];
            uc1 = ((iInput + 1) < (offset + inArrayLen)) ? inArray[iInput + 1] : ((byte) 0);
            uc2 = ((iInput + 2) < (offset + inArrayLen)) ? inArray[iInput + 2] : ((byte) 0);
            outArray[iOutput] = s_rgchBase64Encoding[uc0 >> 2];
            outArray[iOutput + 1] = s_rgchBase64Encoding[((uc0 << 4) & 0x30) | ((uc1 >> 4) & 15)];
            outArray[iOutput + 2] = s_rgchBase64Encoding[((uc1 << 2) & 60) | ((uc2 >> 6) & 3)];
            outArray[iOutput + 3] = s_rgchBase64Encoding[uc2 & 0x3f];
            switch ((inArrayLen % 3))
            {
                case 1:
                    outArray[outArrayLen - 2] = '=';
                    break;

                case 2:
                    break;

                default:
                    goto Label_016E;
            }
            outArray[outArrayLen - 1] = '=';
        Label_016E:
            return new string(outArray);
        }

        #endregion
    }
}

