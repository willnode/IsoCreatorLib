using System;

namespace ISO9660.PrimitiveTypes
{
    internal class PathTableRecord
    {
        /// <summary>
        /// N, the identifier length (or 1 for the root directory)
        /// </summary>
		public byte Length;

        /// <summary>
        /// 0 [number of sectors in extended attribute record]
        /// </summary>
        public byte ExtendedLength = 0;

        /// <summary>
        /// number of the first sector in the directory, as a double word
        /// </summary>
        public UInt32 ExtentLocation;

        /// <summary>
        /// number of record for parent directory (or 1 for the root
        /// directory), as a word; the first record is number 1,
        /// the second record is number 2, etc.
        /// </summary>
        public UInt16 ParentNumber;

        /// <summary>
        /// identifier (or 0 for the root directory)
        /// </summary>
        public byte[] Identifier;

        // One record must have even number of bytes.
    }
}