using System;
using Microsoft.SPOT;

namespace uPLibrary.Networking.Smtp
{
    /// <summary>
    /// Represents an Smtp reply
    /// </summary>
    public class SmtpReply
    {
        // length of reply code in Smtp protocol
        public const int REPLY_CODE_LENGTH = 3;
        // end of each reply message
        public const string CRLF = "\r\n";

        /// <summary>
        /// Reply code for Smtp procotol
        /// </summary>
        public SmtpReplyCode ReplyCode { get; internal set; }

        /// <summary>
        /// Text in the Smtp response
        /// </summary>
        public string Text { get; internal set; }

        /// <summary>
        /// Parse an Smtp response text into SmtpReply
        /// </summary>
        /// <param name="textReply">Smtp response text</param>
        /// <returns>SmtpReply parsed from text</returns>
        public static SmtpReply Parse(string textReply)
        {
            SmtpReply smtpReply = new SmtpReply();

            // extract reply code (first 3 digits)
            smtpReply.ReplyCode = (SmtpReplyCode)Convert.ToInt32(textReply.Substring(0, SmtpReply.REPLY_CODE_LENGTH));
            // extract the text after reply code
            smtpReply.Text = textReply.Substring(3, textReply.IndexOf(SmtpReply.CRLF) - 3).Trim();

            return smtpReply;
        }

    }

    /// <summary>
    /// Reply codes for Smtp protocol
    /// </summary>
    public enum SmtpReplyCode
    {
        /// <summary>
        /// System status, or system help reply
        /// </summary>
        SystemHelpReply = 211,

        /// <summary>
        /// Help message
        /// </summary>
        HelpMessage = 214,

        /// <summary>
        /// "domain" Service ready
        /// </summary>
        DomainServiceReady = 220,

        /// <summary>
        /// "domain" Service closing transmission channel
        /// </summary>
        DomainServiceClosingTransmissionChannel = 221,

        /// <summary>
        /// Requested mail action okay, completed
        /// </summary>
        RequestedMailActionOk = 250,

        /// <summary>
        /// User not local; will forward to "forward-path"
        /// </summary>
        UserNotLocal = 251,

        /// <summary>
        /// Start mail input; end with "CRLF"."CRLF"
        /// </summary>
        StartMailInput = 354,

        /// <summary>
        /// "domain" Service not available
        /// </summary>
        DomainServiceNotAvailable = 421,

        /// <summary>
        /// Requested mail action not taken: mailbox unavailable
        /// </summary>
        RequestedMailActionNotTaken = 450,

        /// <summary>
        /// Requested action aborted: local error in processing
        /// </summary>
        RequestedActionAborted = 451,

        /// <summary>
        /// Requested action not taken: insufficient system storage
        /// </summary>
        RequestedActionNotTaken = 452,

        /// <summary>
        /// Syntax error, command unrecognized
        /// </summary>
        SyntaxError = 500,

        /// <summary>
        /// Syntax error in parameters or arguments
        /// </summary>
        SyntaxErrorParameters = 501,

        /// <summary>
        /// Command not implemented
        /// </summary>
        CommandNotImplemented = 502,

        /// <summary>
        /// Bad sequence of commands
        /// </summary>
        BadSequenceOfCommands = 503,

        /// <summary>
        /// Command parameter not implemented
        /// </summary>
        CommandParameterNotImplemented = 504,

        /// <summary>
        /// Requested action not taken: mailbox unavailable
        /// </summary>
        RequestedActionNotTaken2 = 550,

        /// <summary>
        /// User not local; please try "forward-path"
        /// </summary>
        UserNotLocal2 = 551,

        /// <summary>
        /// Requested mail action aborted: exceeded storage allocation
        /// </summary>
        RequestedMailActionAborted = 552,

        /// <summary>
        /// Requested action not taken: mailbox name not allowed
        /// </summary>
        RequestedActionNotTaken3 = 553,

        /// <summary>
        /// Transaction failed
        /// </summary>
        TransactionFailed = 554
    }
}
