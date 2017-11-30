using BER.CDCat.Export;
using ISO9660.Enums;
using IsoCreatorLib.DirectoryTree;
using IsoCreatorLib.IsoWrappers;
using System;
using System.IO;

namespace IsoCreatorLib
{
    public partial class IsoCreator
    {
        /// <summary>
        /// Used for sending parameters to ParameterizedThreadStart delegate function.
        /// Contains the natural arguments for Folder2Iso function.
        /// </summary>
        public class IsoCreatorFolderArgs
        {
            public string FolderPath { get; private set; }

            public string IsoPath { get; private set; }

            public string VolumeName { get; private set; }

            public IsoCreatorFolderArgs(string folderPath, string isoPath, string volumeName)
            {
                FolderPath = folderPath;
                IsoPath = isoPath;
                VolumeName = volumeName;
            }
        }

        /// <summary>
        /// Used for sending parameters to ParameterizedThreadStart delegate function.
        /// Contains the natural arguments for Folder2Iso function.
        /// </summary>
        public class IsoCreatorTreeArgs
        {
            public TreeNode Volume { get; }

            public string IsoPath { get; }

            public IsoCreatorTreeArgs(TreeNode volume, string isoPath)
            {
                Volume = volume;
                IsoPath = isoPath;
            }
        }

        #region Writing Methods

        /// <summary>
        /// Sets the directory numbers according to the ISO 9660 standard, so that Path Tables could be built. (root=1, first child=2, etc.)
        /// The order of the directories is as following:
        /// 1. If two directories are on different levels, then the one on the lowest level comes first;
        /// 2. If the directories are on the same level, but have different parents, then they are ordered in the same order as their parents.
        /// 3. If the directories have the same parent, then they are sorted according to their name (lexicographic).
        /// </summary>
        /// <param name="dirArray">An array of SORTED IsoDirectories according to the ISO 9660 standard.</param>
        private void SetDirectoryNumbers(IsoDirectory[] dirArray)
        {
            if (dirArray == null) return;

            for (int i = 0; i < dirArray.Length; i++)
                dirArray[i].Number = (UInt16)(i + 1);
        }

        /// <summary>
        /// Writes the first 16 empty sectors of an ISO image.
        /// </summary>
        /// <param name="writer">A binary writer to write the data.</param>
        private void WriteFirst16EmptySectors(BinaryWriter writer)
        {
            writer.Write(new byte[IsoAlgorithm.SectorSize * 16]);
        }

        /// <summary>
        /// Writes three volume descriptors speciffic to the ISO 9660 Joliet:
        /// 1. Primary volume descriptor;
        /// 2. Suplementary volume descriptor;
        /// 3. Volume descriptor set terminator.
        /// </summary>
        /// <param name="writer">A binary writer to write the data.</param>
        /// <param name="volumeName">A normal string representing the desired name of the volume.
        /// (the maximum standard length for this string is 16 for Joliet, so if the name is larger
        /// than 16 characters, it is truncated.)</param>
        /// <param name="root">The root IsoDirectory, representing the root directory for the volume.</param>
        /// <param name="volumeSpaceSize">The ISO total space size IN SECTORS.
        /// (For example, if the ISO space size is 1,427,456 bytes, then the volumeSpaceSize will be 697)</param>
        /// <param name="pathTableSize1">The first path table size (for the primary volume) IN BYTES.</param>
        /// <param name="pathTableSize2">The second path table size (for the suplementary volume) IN BYTES.</param>
        /// <param name="typeLPathTable1">The location (sector) of the first LITTLE ENDIAN path table.</param>
        /// <param name="typeMPathTable1">The location (sector) of the first BIG ENDIAN path table.</param>
        /// <param name="typeLPathTable2">The location (sector) of the second LITTLE ENDIAN path table.</param>
        /// <param name="typeMPathTable2">The location (sector) of the second BIG ENDIAN path table.</param>
        private void WriteVolumeDescriptors(BinaryWriter writer,
                                             string volumeName,
                                             IsoDirectory root,
                                             UInt32 volumeSpaceSize,
                                             UInt32 pathTableSize1, UInt32 pathTableSize2,
                                             UInt32 typeLPathTable1, UInt32 typeMPathTable1,
                                             UInt32 typeLPathTable2, UInt32 typeMPathTable2)
        {
            // Throughout this program I have respected the convention of refering to the root as ".";
            // However, one should not confuse the root with the current directory, also known as "." (along with the parent directory, "..").

            // Primary Volume Descriptor:

            // Create a Directory Record of the root and the volume descriptor.
            DirectoryRecordWrapper rootRecord = new DirectoryRecordWrapper(root.Extent1, root.Size1, root.Date, root.IsDirectory, ".");
            VolumeDescriptorWrapper volumeDescriptor = new VolumeDescriptorWrapper(volumeName, volumeSpaceSize, pathTableSize1, typeLPathTable1, typeMPathTable1, rootRecord, DateTime.Now, DateTime.Now, 8)
            {
                VolumeDescriptorType = VolumeType.Primary
            };
            volumeDescriptor.Write(writer);

            // Suplementary volume descriptor:

            rootRecord = new DirectoryRecordWrapper(root.Extent2, root.Size2, root.Date, root.IsDirectory, ".");
            volumeDescriptor = new VolumeDescriptorWrapper(volumeName, volumeSpaceSize, pathTableSize2, typeLPathTable2, typeMPathTable2, rootRecord, DateTime.Now, DateTime.Now, 8)
            {
                VolumeDescriptorType = VolumeType.Suplementary
            };
            volumeDescriptor.Write(writer);

            // Volume descriptor set terminator:

            volumeDescriptor.VolumeDescriptorType = VolumeType.SetTerminator;
            volumeDescriptor.Write(writer);
        }

        /// <summary>
        /// Writes the containings of each directory
        /// </summary>
        /// <param name="writer">A binary writer to write the data.</param>
        /// <param name="dirArray">An array of IsoDirectories to be written.</param>
        /// <param name="type">The type of writing to be performed:
        /// Primary - corresponding to the Primary Volume Descriptor (DOS Speciffic - 8 letter ASCII upper case names)
        /// Suplementary - corresponding to the Suplementary Volume Descriptor (Windows speciffic - 101 letter Unicode names)</param>
        private void WriteDirectories(BinaryWriter writer, IsoDirectory[] dirArray, VolumeType type)
        {
            if (dirArray == null) return;

            for (int i = 0; i < dirArray.Length; i++)
            {
                dirArray[i].Write(writer, type);
                OnProgress((int)(writer.BaseStream.Length / IsoAlgorithm.SectorSize));
            }
        }

        /// <summary>
        /// Writes a path table corresponding to a given directory structure.
        /// The order of the directories is as following (ISO 9660 standard):
        /// 1. If two directories are on different levels, then the one on the lowest level comes first;
        /// 2. If the directories are on the same level, but have different parents, then they are ordered in the same order as their parents.
        /// 3. If the directories have the same parent, then they are sorted according to their name (lexicographic).
        /// </summary>
        /// <param name="writer">A binary writer to write the data.</param>
        /// <param name="dirArray">An array of IsoDirectories representing the directory structure.</param>
        /// <param name="endian">The byte order of numbers (little endian or big endian).</param>
        /// <param name="type">The type of writing to be performed:
        /// Primary - corresponding to the Primary Volume Descriptor (DOS Speciffic - 8 letter ASCII upper case names)
        /// Suplementary - corresponding to the Suplementary Volume Descriptor (Windows speciffic - 101 letter Unicode names)</param>
        /// <returns>An integer representing the total number of bytes written.</returns>
        private int WritePathTable(BinaryWriter writer, IsoDirectory[] dirArray, Endian endian, VolumeType type)
        {
            if (dirArray == null) return 0;

            int bytesWritten = 0;
            for (int i = 0; i < dirArray.Length; i++)
            {
                // The directory list is sorted according to the ISO 9660 standard, so the first one (0) should be the root.
                bytesWritten += dirArray[i].WritePathTable(writer, (i == 0), endian, type);
            }

            // A directory must ocupy a number of bytes multiple of 2048 (the sector size).
            writer.Write(new byte[IsoAlgorithm.SectorSize - (bytesWritten % IsoAlgorithm.SectorSize)]);

            return bytesWritten;
        }

        #endregion Writing Methods
    }
}