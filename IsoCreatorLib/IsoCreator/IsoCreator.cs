using System;
using System.IO;
using System.Collections;
using BER.CDCat.Export;
using ISO9660.Enums;
using IsoCreatorLib.DirectoryTree;
using System.Diagnostics;

namespace IsoCreatorLib
{
    public partial class IsoCreator
    {

        #region Folder to ISO

        /// <summary>
        /// Writes an ISO with the contains of the folder given as a parameter.`
        /// </summary>
        /// <param name="rootDirectoryInfo">The folder to be turned into an iso.</param>
        /// <param name="writer">A binary writer to write the data.</param>
        /// <param name="volumeName">The name of the volume created.</param>
        private void Folder2Iso(DirectoryInfo rootDirectoryInfo, BinaryWriter writer, string volumeName)
        {

            ArrayList dirList;
            IsoDirectory[] dirArray;

            OnProgress("Initializing ISO root directory...", 0, 1);

            IsoDirectory root = new IsoDirectory(rootDirectoryInfo, 1, "0", Progress);

            //
            // Folder structure and path tables corresponding to the Primary Volume Descriptor:
            //

            OnProgress("Preparing first set of directory extents...", 0, 1);

            dirList = new ArrayList { root };

            // Set all extents corresponding to the primary volume descriptor;
            // Memorize the SORTED directories in the dirList list.
            // The first extent (corresponding to the root) should be at the 19th sector 
            // (which is the first available sector: 0-15 are empty and the next 3 (16-18) 
            // are occupied by the volume descriptors).
            IsoDirectory.SetExtent1(dirList, 0, 19);

            OnProgress(1);

            OnProgress("Calculating directory numbers...", 0, 1);

            dirArray = new IsoDirectory[dirList.Count];
            dirList.ToArray().CopyTo(dirArray, 0);      // Copy to an array the sorted directory list.

            SetDirectoryNumbers(dirArray);         // Set the directory numbers, used in the path tables.

            OnProgress(1);

            OnProgress("Preparing first set of path tables...", 0, 2);

            // Create a memory stream where to temporarily save the path tables. 
            // (We can't write them directly to the file, because we first have to write - by convention - 
            // the directories. For now, we cannot do that, since we don't know the files' extents.
            // Those will be calculated later, when we know the actual size of the path tables, because
            // the files come at the end of the file, after the path tables.)
            // I used this algorihm, although a little backword, since this is the algorithm NERO uses,
            // and I gave them credit for choosing the best one ;)
            MemoryStream memory1 = new MemoryStream();
            BinaryWriter memoryWriter1 = new BinaryWriter(memory1);

            // Calculate the position of the first little endian path table, which comes right after the last directory.
            IsoDirectory lastDir = dirArray[dirArray.Length - 1];
            UInt32 typeLPathTable1 = lastDir.Extent1 + lastDir.Size1 / IsoAlgorithm.SectorSize;

            WritePathTable(memoryWriter1, dirArray, Endian.LittleEndian, VolumeType.Primary);

            OnProgress(1);

            // Calculate the position of the first big endian path table.
            UInt32 typeMPathTable1 = typeLPathTable1 + (UInt32)(memory1.Length) / IsoAlgorithm.SectorSize;

            UInt32 pathTableSize1 = (UInt32)WritePathTable(memoryWriter1, dirArray, Endian.BigEndian, VolumeType.Primary);

            OnProgress(2);

            //
            // end
            //

            //
            // Folder structure and path tables corresponding to the Suplementary Volume Descriptor:
            //

            OnProgress("Preparing second set of directory extents...", 0, 1);

            dirList = new ArrayList { root };

            UInt32 currentExtent = typeLPathTable1 + (UInt32)(memory1.Length) / IsoAlgorithm.SectorSize;

            IsoDirectory.SetExtent2(dirList, 0, currentExtent);

            dirArray = new IsoDirectory[dirList.Count];
            dirList.ToArray().CopyTo(dirArray, 0);

            OnProgress(1);

            OnProgress("Preparing second set of path tables...", 0, 2);

            MemoryStream memory2 = new MemoryStream();
            BinaryWriter memoryWriter2 = new BinaryWriter(memory2);

            lastDir = dirArray[dirArray.Length - 1];
            UInt32 typeLPathTable2 = lastDir.Extent2 + lastDir.Size2 / IsoAlgorithm.SectorSize;

            WritePathTable(memoryWriter2, dirArray, Endian.LittleEndian, VolumeType.Suplementary);

            OnProgress(1);

            UInt32 typeMPathTable2 = typeLPathTable2 + (UInt32)(memory2.Length) / IsoAlgorithm.SectorSize;

            UInt32 pathTableSize2 = (UInt32)WritePathTable(memoryWriter2, dirArray, Endian.BigEndian, VolumeType.Suplementary);

            OnProgress(2);

            //
            // end
            //

            OnProgress("Initializing...", 0, 1);

            // Now that we know the extents and sizes of all directories and path tables, 
            // all that remains is to calculate files extent:
            currentExtent = typeLPathTable2 + (UInt32)(memory2.Length) / IsoAlgorithm.SectorSize;
            root.SetFilesExtent(ref currentExtent);

            // Calculate the total size in sectors of the file to be made.
            UInt32 volumeSpaceSize = 19;
            volumeSpaceSize += root.TotalSize;

            volumeSpaceSize += (UInt32)memory1.Length / IsoAlgorithm.SectorSize;
            volumeSpaceSize += (UInt32)memory2.Length / IsoAlgorithm.SectorSize;

            // Prepare the buffers for the path tables.
            byte[] pathTableBuffer1 = memory1.GetBuffer();
            Array.Resize(ref pathTableBuffer1, (int)memory1.Length);

            byte[] pathTableBuffer2 = memory2.GetBuffer();
            Array.Resize(ref pathTableBuffer2, (int)memory2.Length);

            // Close the memory streams.
            memory1.Close();
            memory2.Close();
            memoryWriter1.Close();
            memoryWriter2.Close();

            OnProgress(1);

            //
            // Now all we have to do is to write all information to the ISO:
            //

            OnProgress("Writing data to file...", 0, (int)volumeSpaceSize);

            // First, write the 16 empty sectors.
            WriteFirst16EmptySectors(writer);

            OnProgress((int)(writer.BaseStream.Length / IsoAlgorithm.SectorSize));

            // Write the three volume descriptors.
            WriteVolumeDescriptors(
                writer, volumeName, root,
                volumeSpaceSize,
                pathTableSize1, pathTableSize2,
                typeLPathTable1, typeMPathTable1,
                typeLPathTable2, typeMPathTable2);

            OnProgress((int)(writer.BaseStream.Length / IsoAlgorithm.SectorSize));

            // Write the directories in a manner corresponding to the Primary Volume Descriptor.
            WriteDirectories(writer, dirArray, VolumeType.Primary);

            // Write the first two path tables.
            writer.Write(pathTableBuffer1);

            OnProgress((int)(writer.BaseStream.Length / IsoAlgorithm.SectorSize));

            // Write the directories in a manner corresponding to the Suplementary Volume Descriptor.
            WriteDirectories(writer, dirArray, VolumeType.Suplementary);

            // Write the other two path tables.
            writer.Write(pathTableBuffer2);

            OnProgress((int)(writer.BaseStream.Length / IsoAlgorithm.SectorSize));

            // Write the files.
            root.WriteFiles(writer, Progress);

            // That's it ;)
        }

        /// <summary>
        /// Writes an ISO with the contains of the folder given as a parameter.
        /// </summary>
        /// <param name="folderPath">The path of the folder to be turned into an iso.</param>
        /// <param name="isoPath">The path of the iso file.</param>
        /// <param name="volumeName">The name of the volume to be created.</param>
        public void Folder2Iso(string folderPath, string isoPath, string volumeName)
        {
            try
            {
                FileStream isoFileStream = new FileStream(isoPath, FileMode.Create);
                BinaryWriter writer = new BinaryWriter(isoFileStream);
                DirectoryInfo rootDirectoryInfo = new DirectoryInfo(folderPath);

                try
                {
                    Folder2Iso(rootDirectoryInfo, writer, volumeName); 
                    OnFinished("ISO writing process finished succesfully");
                }
                catch (Exception ex) { throw ex; }

                writer.Close();
                isoFileStream.Close();

            }
            catch (System.Threading.ThreadAbortException ex)
            {
                Debug.WriteLine(ex.Message);
                OnAbort("Aborted by user");
            }
            catch (Exception ex)
            {
                OnAbort(ex.Message);
            }
        }

        /// <summary>
        /// Writes an ISO with the speciffications contained in the IsoCreatorArgs object given as parameter.
        /// </summary>
        /// <param name="data">An IsoCreatorFolderArgs object.</param>
        public void Folder2Iso(object data)
        {
            if (data is IsoCreatorFolderArgs args)
                Folder2Iso(args.FolderPath, args.IsoPath, args.VolumeName);
        }

        #endregion

        #region Tree to ISO

        /// <summary>
        /// Writes an ISO with the contains of the tree given as a parameter.
        /// This is a "virtual" ISO, which means that you will find on it only a directory structure;
        /// files will actually not ocupy any space on it. (For a better picture of what happens here,
        /// run the VirtualIsoCreator form in Forms namespace. There is a demo. Also, if you have CDCat 
        /// installed on your PC, you should know by now the effect of the method below. Within CDCat, 
        /// this method is used through the ExportIso class in BER.CDCat.Export namespace)
        /// </summary>
        /// <param name="volume">The directory structure to be turned into an iso.</param>
        /// <param name="writer">A binary writer to write the data.</param>
        private void Tree2Iso(TreeNode volume, BinaryWriter writer)
        {

            ArrayList dirList;
            IsoDirectory[] dirArray;

            OnProgress("Initializing ISO root directory...", 0, 1);

            IsoDirectory root = new IsoDirectory(volume, 1, "0", Progress);

            //
            // Folder structure and path tables corresponding to the Primary Volume Descriptor:
            //

            OnProgress("Preparing first set of directory extents...", 0, 1);

            dirList = new ArrayList { root };

            // Set all extents corresponding to the primary volume descriptor;
            // Memorize the SORTED directories in the dirList list.
            // The first extent (corresponding to the root) should be at the 19th sector 
            // (which is the first available sector: 0-15 are empty and the next 3 (16-18) 
            // are occupied by the volume descriptors).
            IsoDirectory.SetExtent1(dirList, 0, 19);

            OnProgress(1);

            OnProgress("Calculating directory numbers...", 0, 1);

            dirArray = new IsoDirectory[dirList.Count];
            dirList.ToArray().CopyTo(dirArray, 0);      // Copy to an array the sorted directory list.

            SetDirectoryNumbers(dirArray);         // Set the directory numbers, used in the path tables.

            OnProgress(1);

            OnProgress("Preparing first set of path tables...", 0, 2);

            // Create a memory stream where to temporarily save the path tables. 
            // (We can't write them directly to the file, because we first have to write - by convention - 
            // the directories. For now, we cannot do that, since we don't know the files' extents.
            // Those will be calculated later, when we know the actual size of the path tables, because
            // the files come at the end of the file, after the path tables.)
            // I used this algorihm, although a little backword, since this is the algorithm NERO uses,
            // and I gave them credit for choosing the best one ;)
            MemoryStream memory1 = new MemoryStream();
            BinaryWriter memoryWriter1 = new BinaryWriter(memory1);

            // Calculate the position of the first little endian path table, which comes right after the last directory.
            IsoDirectory lastDir = dirArray[dirArray.Length - 1];
            UInt32 typeLPathTable1 = lastDir.Extent1 + lastDir.Size1 / IsoAlgorithm.SectorSize;

            WritePathTable(memoryWriter1, dirArray, Endian.LittleEndian, VolumeType.Primary);

            OnProgress(1);

            // Calculate the position of the first big endian path table.
            UInt32 typeMPathTable1 = typeLPathTable1 + (UInt32)(memory1.Length) / IsoAlgorithm.SectorSize;

            UInt32 pathTableSize1 = (UInt32)WritePathTable(memoryWriter1, dirArray, Endian.BigEndian, VolumeType.Primary);

            OnProgress(2);

            //
            // end
            //

            //
            // Folder structure and path tables corresponding to the Suplementary Volume Descriptor:
            //

            OnProgress("Preparing second set of directory extents...", 0, 1);

            dirList = new ArrayList { root };

            UInt32 currentExtent = typeLPathTable1 + (UInt32)(memory1.Length) / IsoAlgorithm.SectorSize;

            IsoDirectory.SetExtent2(dirList, 0, currentExtent);

            dirArray = new IsoDirectory[dirList.Count];
            dirList.ToArray().CopyTo(dirArray, 0);

            OnProgress(1);

            OnProgress("Preparing second set of path tables...", 0, 2);

            MemoryStream memory2 = new MemoryStream();
            BinaryWriter memoryWriter2 = new BinaryWriter(memory2);

            lastDir = dirArray[dirArray.Length - 1];
            UInt32 typeLPathTable2 = lastDir.Extent2 + lastDir.Size2 / IsoAlgorithm.SectorSize;

            WritePathTable(memoryWriter2, dirArray, Endian.LittleEndian, VolumeType.Suplementary);

            OnProgress(1);

            UInt32 typeMPathTable2 = typeLPathTable2 + (UInt32)(memory2.Length) / IsoAlgorithm.SectorSize;

            UInt32 pathTableSize2 = (UInt32)WritePathTable(memoryWriter2, dirArray, Endian.BigEndian, VolumeType.Suplementary);

            OnProgress(2);

            //
            // end
            //

            OnProgress("Initializing...", 0, 1);

            // Now that we know the extents and sizes of all directories and path tables, 
            // all that remains is to calculate files extent. However, this being a virtual ISO,
            // it won't memorize real files, but only images of files, which will apear to have a real size,
            // but in fact, won't occupy any more space. So we will leave all the files' extents null (0).

            // Calculate the total size in sectors of the file to be made.
            UInt32 volumeSpaceSize = 19;
            volumeSpaceSize += root.TotalDirSize;   // This only calculates the size of the directories, without the files.
            volumeSpaceSize += (UInt32)memory1.Length / IsoAlgorithm.SectorSize;
            volumeSpaceSize += (UInt32)memory2.Length / IsoAlgorithm.SectorSize;

            // Prepare the buffers for the path tables.
            byte[] pathTableBuffer1 = memory1.GetBuffer();
            Array.Resize(ref pathTableBuffer1, (int)memory1.Length);

            byte[] pathTableBuffer2 = memory2.GetBuffer();
            Array.Resize(ref pathTableBuffer2, (int)memory2.Length);

            // Close the memory streams.
            memory1.Close();
            memory2.Close();
            memoryWriter1.Close();
            memoryWriter2.Close();

            OnProgress(1);

            //
            // Now all we have to do is to write all information to the ISO:
            //

            OnProgress("Writing data to file...", 0, (int)volumeSpaceSize);

            // First, write the 16 empty sectors.
            WriteFirst16EmptySectors(writer);

            OnProgress((int)(writer.BaseStream.Length / IsoAlgorithm.SectorSize));

            // Write the three volume descriptors.
            WriteVolumeDescriptors(
                writer, volume.Name, root,
                volumeSpaceSize,
                pathTableSize1, pathTableSize2,
                typeLPathTable1, typeMPathTable1,
                typeLPathTable2, typeMPathTable2);

            OnProgress((int)(writer.BaseStream.Length / IsoAlgorithm.SectorSize));

            // Write the directories in a manner corresponding to the Primary Volume Descriptor.
            WriteDirectories(writer, dirArray, VolumeType.Primary);

            // Write the first two path tables.
            writer.Write(pathTableBuffer1);

            OnProgress((int)(writer.BaseStream.Length / IsoAlgorithm.SectorSize));

            // Write the directories in a manner corresponding to the Suplementary Volume Descriptor.
            WriteDirectories(writer, dirArray, VolumeType.Suplementary);

            // Write the other two path tables.
            writer.Write(pathTableBuffer2);

            OnProgress((int)(writer.BaseStream.Length / IsoAlgorithm.SectorSize));

            // If this were an ISO with real files, this is the part where we would write the files.

            // That's it ;)
        }

        /// <summary>
        /// Writes an ISO with the contains of the tree given as a parameter, to the specified path.
        /// </summary>
        /// <param name="volume">The directory structure to be turned into an iso.</param>
        /// <param name="isoPath">The path of the iso file to be created.</param>
        public void Tree2Iso(TreeNode volume, string isoPath)
        {
            try
            {
                FileStream isoFileStream = new FileStream(isoPath, FileMode.Create);
                BinaryWriter writer = new BinaryWriter(isoFileStream);
                try
                {
                    Tree2Iso(volume, writer);

                    writer.Close();
                    isoFileStream.Close();

                    OnFinished("ISO writing process finished succesfully");
                }
                catch (Exception ex)
                {
                    writer.Close();
                    isoFileStream.Close();
                    throw ex;
                }
            }
            catch (System.Threading.ThreadAbortException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                OnAbort("Aborted by user");
            }
            catch (Exception ex)
            {
                OnAbort(ex.Message);
            }
        }

        /// <summary>
        /// Writes an ISO with the speciffications contained in the IsoCreatorTreeArgs object given as parameter.
        /// </summary>
        /// <param name="data">An IsoCreatorTreeArgs object.</param>
        public void Tree2Iso(object data)
        {
            if (data.GetType() != typeof(IsoCreatorTreeArgs))
            {
                return;
            }

            IsoCreatorTreeArgs args = (IsoCreatorTreeArgs)data;
            Tree2Iso(args.Volume, args.IsoPath);
        }

        #endregion

        #region Events

        public event ProgressDelegate Progress;

        public event FinishDelegate Finish;

        public event AbortDelegate Abort;

        private void OnFinished(string message) => Finish?.Invoke(this, new FinishEventArgs(message));

        private void OnProgress(int current) => Progress?.Invoke(this, new ProgressEventArgs(current));

        private void OnProgress(int current, int maximum) => Progress?.Invoke(this, new ProgressEventArgs(current, maximum));

        private void OnProgress(string action, int current, int maximum) => Progress?.Invoke(this, new ProgressEventArgs(current, maximum, action));

        private void OnAbort(string message) => Abort?.Invoke(this, new AbortEventArgs(message));

        #endregion
    }
}
