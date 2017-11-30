namespace ISO9660.PrimitiveTypes
{
    internal class BinaryDateRecord
    {
        /// <summary>
        /// number of years since 1900
        /// </summary>
		public byte Year;

        /// <summary>
        /// month, where 1=January, 2=February, etc.
        /// </summary>
        public byte Month;

        /// <summary>
        /// day of month, in the range from 1 to 31
        /// </summary>
        public byte DayOfMonth;

        /// <summary>
        /// hour, in the range from 0 to 23
        /// </summary>
		public byte Hour;

        /// <summary>
        /// minute, in the range from 0 to 59
        /// </summary>
        public byte Minute;

        /// <summary>
        /// second, in the range from 0 to 59
        /// (for DOS this is always an even number)
        /// </summary>
        public byte Second;
    }
}