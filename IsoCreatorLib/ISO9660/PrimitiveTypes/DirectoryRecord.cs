using IsoCreatorLib;
using System;

namespace ISO9660.PrimitiveTypes
{
    internal class DirectoryRecord
    {
        // [1]
        /// <summary>
        /// 34, the number of bytes in the record (which must be even)
        /// </summary>
        public byte Length = IsoAlgorithm.DefaultDirectoryRecordLength;

        // [2]
        /// <summary>
        /// always 0 in DOS [number of sectors in extended attribute record]
        /// </summary>
        public byte ExtendedAttributeLength = 0;

        // [3-10]
        /// <summary>
        /// number of the first sector of file data or directory
        /// (zero for an empty file), as a both endian double word
        /// </summary>
        public UInt64 ExtentLocation;

        // [11-18]
        /// <summary>
        /// number of bytes of file data or length of directory,
        /// excluding the extended attribute record,
        /// as a both endian double word
        /// </summary>
        public UInt64 DataLength;

        // [19-24]
        /// <summary>
        /// Details in struct DateRecord
        /// </summary>
        public BinaryDateRecord Date = new BinaryDateRecord();

        // [25]
        /// <summary>
        /// offset from Greenwich Mean Time, in 15-minute intervals,
        /// as a twos complement signed number, positive for time
        /// zones east of Greenwich, and negative for time zones
        /// west of Greenwich (DOS ignores this field)
        /// </summary>
        public sbyte TimeZone;

        // [26]
        /// <summary>
        /// flags, with bits as follows:
        /// bit     value
        /// ------  ------------------------------------------
        /// 0 (LS)  0 for a normal file, 1 for a hidden file
        /// 1       0 for a file, 1 for a directory
        /// 2       0 [1 for an associated file]
        /// 3       0 [1 for record format specified]
        /// 4       0 [1 for permissions specified]
        /// 5       0
        /// 6       0
        /// 7 (MS)  0 [1 if not the final record for the file]
        /// </summary>
        public byte FileFlags;

        // [27]
        /// <summary>
        /// 0 [file unit size for an interleaved file]
        /// </summary>
        public byte FileUnitSize = 0;

        // [28]
        /// <summary>
        /// 0 [interleave gap size for an interleaved file]
        /// </summary>
        public byte InterleaveGapSize = 0;

        // [29-32]
        /// <summary>
        ///  1, as a both endian word [volume sequence number]
        /// </summary>
        public UInt32 VolumeSequnceNumber = 0x01000001;

        // [33]
        /// <summary>
        /// the identifier length
        /// </summary>
        public byte LengthOfFileIdentifier;

        // [34-...]
        /// <summary>
        /// identifier
        /// </summary>
        public byte[] FileIdentifier;

        // A directory record must have even number of bytes. (!!)
    }
}