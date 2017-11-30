using IsoCreatorLib;
using System;

namespace ISO9660.PrimitiveTypes
{
    internal class VolumeDescriptor
    {
        // [1]
        /// <summary>
        ///  1, 2, 255
        /// </summary>
        public byte VolumeDescType;

        // [2-7]
        /// <summary>
        ///  67, 68, 48, 48, 49 ( that is "CD001" ) and 1, respectively
        ///  (same as Volume Descriptor Set Terminator)
        /// </summary>
        public byte[] StandardIdentifier = new byte[6] { 67, 68, 48, 48, 49, 1 };

        // [8]
        /// <summary>
        /// 0
        /// </summary>
        public byte Reserved1 = 0;

        // [9-40]
        /// <summary>
        /// system identifier
        /// </summary>
        public byte[] SystemId = IsoAlgorithm.MemSet(IsoAlgorithm.SystemIdLength, IsoAlgorithm.AsciiBlank);

        // [41-72]
        /// <summary>
        /// volume identifier
        /// </summary>
        public byte[] VolumeId = IsoAlgorithm.MemSet(IsoAlgorithm.VolumeIdLength, IsoAlgorithm.AsciiBlank);

        // [73-80]
        /// <summary>
        /// 0
        /// </summary>
        public UInt64 Reserved2 = 0;

        // [81-88]
        /// <summary>
        /// total number of sectors, as a both endian double word
        /// </summary>
        public UInt64 VolumeSpaceSize;

        // [89-91]
        /// <summary>
        /// Constant
        /// </summary>
        public byte[] Reserved3_1 = new byte[3] { 37, 47, 69 };

        // [92-120]
        /// <summary>
        /// Zeros
        /// </summary>
        public byte[] Reserved3_2 = new byte[29];

        // [121-124]
        /// <summary>
        ///  1, as a both endian word [volume set size]
        /// </summary>
        public UInt32 VolumeSetSize = 0x01000001;

        // [125-128]
        /// <summary>
        /// 1, as a both endian word [volume sequence number]
        /// </summary>
        public UInt32 VolumeSequenceNumber = 0x01000001;

        // [129-132]
        /// <summary>
        /// 2048 (the sector size), as a both endian word
        /// </summary>
        public UInt32 SectorkSize = 0x00080800;

        // [133-140]
        /// <summary>
        /// path table length in bytes, as a both endian double word
        /// </summary>
        public UInt64 PathTableSize;

        // [141-144]
        /// <summary>
        /// number of first sector in first little endian path table,
        /// as a little endian double word
        /// </summary>
        public UInt32 TypeLPathTable;

        // [145-148]
        /// <summary>
        /// number of first sector in second little endian path table,
        /// as a little endian double word, or zero if there is no
        /// second little endian path table
        /// </summary>
        public UInt32 OptionalTypeLPathTable = 0;

        // [149-152]
        /// <summary>
        /// number of first sector in first big endian path table,
        /// as a big endian double word
        /// </summary>
        public UInt32 TypeMPathTable; /* a inverser */

        // [153-156]
        /// <summary>
        /// number of first sector in second big endian path table,
        /// as a big endian double word, or zero if there is no
        /// second big endian path table
        /// </summary>
        public UInt32 OptionalTypeMPathTable = 0; /* a inverser */

        // [157-190]
        /// <summary>
        /// root directory record
        /// </summary>
        public DirectoryRecord RootDirRecord = new DirectoryRecord();

        // [191-318]
        /// <summary>
        /// volume set identifier
        /// </summary>
        public byte[] VolumeSetId = IsoAlgorithm.MemSet(IsoAlgorithm.VolumeSetIdLength, IsoAlgorithm.AsciiBlank);

        // [319-446]
        /// <summary>
        ///  publisher identifier
        /// </summary>
        public byte[] PublisherId = IsoAlgorithm.MemSet(IsoAlgorithm.PublisherIdLength, IsoAlgorithm.AsciiBlank);

        // [447-574]
        /// <summary>
        /// data preparer identifier
        /// </summary>
        public byte[] PreparerId = IsoAlgorithm.MemSet(IsoAlgorithm.PreparerIdLength, IsoAlgorithm.AsciiBlank);

        // [575-702]
        /// <summary>
        /// application identifier
        /// </summary>
        public byte[] ApplicationId = IsoAlgorithm.MemSet(IsoAlgorithm.ApplicationIdLength, IsoAlgorithm.AsciiBlank);

        // [703-739]
        /// <summary>
        /// copyright file identifier
        /// </summary>
        public byte[] CopyrightFileId = IsoAlgorithm.MemSet(IsoAlgorithm.CopyrightFileIdLength, IsoAlgorithm.AsciiBlank);

        // [740-776]
        /// <summary>
        /// abstract file identifier
        /// </summary>
        public byte[] AbstractFileId = IsoAlgorithm.MemSet(IsoAlgorithm.AbstractFileIdLength, IsoAlgorithm.AsciiBlank);

        // [777-813]
        /// <summary>
        /// bibliographical file identifier
        /// </summary>
        public byte[] BibliographicFileId = IsoAlgorithm.MemSet(IsoAlgorithm.BibliographicFileIdLength, IsoAlgorithm.AsciiBlank);

        //

        // [814-830]
        /// <summary>
        /// date and time of volume creation
        /// </summary>
        public AsciiDateRecord CreationDate = new AsciiDateRecord();

        // [831-847]
        /// <summary>
        /// date and time of most recent modification
        /// </summary>
        public AsciiDateRecord ModificationDate = new AsciiDateRecord();

        // [848-864]
        /// <summary>
        /// date and time when volume expires
        /// </summary>
        public AsciiDateRecord ExpirationDate = new AsciiDateRecord();

        // [865-881]
        /// <summary>
        /// date and time when volume is effective
        /// </summary>
        public AsciiDateRecord EffectiveDate = new AsciiDateRecord();

        // [882]
        /// <summary>
        /// 1
        /// </summary>
        public byte FileStructureVersion = 1;

        // [883]
        /// <summary>
        /// 0
        /// </summary>
        public byte Reserved4 = 0;

        // [884-1395]
        /// <summary>
        /// reserved for application use (usually zeros)
        /// </summary>
        public byte[] ApplicationData = new byte[512];

        // [1396-2048]
        /// <summary>
        /// zeros (TOTAL 2048 bytes)
        /// </summary>
        public byte[] Reserved5 = new byte[653];
    }
}