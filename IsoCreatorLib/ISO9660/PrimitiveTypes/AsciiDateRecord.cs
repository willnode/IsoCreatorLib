namespace ISO9660.PrimitiveTypes
{
    internal class AsciiDateRecord
    {
        /// <summary>
        /// year, as four ASCII digits
        /// </summary>
        public byte[] Year = { 48, 48, 48, 48 };

        /// <summary>
        /// month, as two ASCII digits, where 01=January, 02=February, etc.
        /// </summary>
        public byte[] Month = { 48, 48 };

        /// <summary>
        /// day of month, as two ASCII digits, in the range
        /// from 01 to 31
        /// </summary>
        public byte[] DayOfMonth = { 48, 48 };

        /// <summary>
        /// hour, as two ASCII digits, in the range from 00 to 23
        /// </summary>
        public byte[] Hour = { 48, 48 };

        /// <summary>
        /// minute, as two ASCII digits, in the range from 00 to 59
        /// </summary>
        public byte[] Minute = { 48, 48 };

        /// <summary>
        /// second, as two ASCII digits, in the range from 00 to 59
        /// </summary>
        public byte[] Second = { 48, 48 };

        /// <summary>
        /// hundredths of a second, as two ASCII digits, in the range from 00 to 99
        /// </summary>
        public byte[] HundredthsOfSecond = { 48, 48 };

        /// <summary>
        /// offset from Greenwich Mean Time, in 15-minute intervals,
        /// as a twos complement signed number, positive for time
        /// zones east of Greenwich, and negative for time zones
        /// west of Greenwich
        /// </summary>
        public sbyte TimeZone;
    }
}