using BER.CDCat.Export;
using ISO9660.Enums;
using IsoCreatorLib.IsoWrappers;
using System;
using System.Collections;
using System.IO;

namespace IsoCreatorLib.DirectoryTree
{
    /// <summary>
    /// The directory (folder element).
    /// </summary>
    internal class IsoDirectory : IsoFolderElement
    {
        // The directory memorizes two sizes:
        private UInt32 m_size1;             // The size in sectors occupied by all the short-ascii-named children.

        private UInt32 m_size2;             // The size in sectors occupied by all the full-unicode-named children.

        #region Constructor for Real directories

        private void CalculateSize()
        {
            // Calculate the size of the current directory in sectors
            m_size1 = m_size2 = 1;

            UInt32 position1 = 2U * IsoAlgorithm.DefaultDirectoryRecordLength;
            UInt32 position2 = 2U * IsoAlgorithm.DefaultDirectoryRecordLength;

            foreach (IsoFolderElement child in Children)
            {
                UInt32 size1;
                if (child.IsDirectory)
                    // A directory record of a sub-dir has the size equal to 33 + lengthOf(<sub-dir name>)
                    size1 = (UInt32)(child.ShortName.Length + IsoAlgorithm.DefaultDirectoryRecordLength - 1);
                else
                    // A directory record of a file has the size equal to
                    // 33 + lengthOf(<file name>) + lengthOf(";1") - the standard suffix for file names in ISO9660
                    size1 = (UInt32)(child.ShortName.Length + 2 + IsoAlgorithm.DefaultDirectoryRecordLength - 1);

                if (size1 % 2 == 1) size1++;

                // If a directory record is bigger than the space left in the sector, it will be written in a new sector.
                if (position1 + size1 > IsoAlgorithm.SectorSize)
                {
                    // Increment the size of the current directory:
                    m_size1++;
                    position1 = size1;
                }
                else
                    position1 += size1;

                UInt32 size2;
                // The normal name length is multiplied by 2, since the names are memorized in unicode characters,
                // which ocupy 2 bytes instead of one.
                if (child.IsDirectory)
                    size2 = (UInt32)(2 * child.LongName.Length + IsoAlgorithm.DefaultDirectoryRecordLength - 1);
                else
                    size2 = (UInt32)(2 * (child.LongName.Length + 2) + IsoAlgorithm.DefaultDirectoryRecordLength - 1);

                if (size2 % 2 == 1)
                    size2++;

                if (position2 + size2 > IsoAlgorithm.SectorSize)
                {
                    m_size2++;
                    position2 = size2;
                }
                else
                    position2 += size2;
            }
        }

        private void Initialize(DirectoryInfo directory, UInt32 level, ProgressDelegate Progress)
        {
            Level = level;

            FileSystemInfo[] children = directory.GetFileSystemInfos();

            if (children != null)
            {
                Progress?.Invoke(this, new ProgressEventArgs(0, children.Length));

                int childNumberLength = children.Length.ToString().Length;
                for (int i = 0; i < children.Length; i++)
                {
                    string childNumber = String.Format("{0:D" + childNumberLength.ToString() + "}", i);

                    Children.Add(children[i] is DirectoryInfo ? (IsoFolderElement)
                      new IsoDirectory(this, (DirectoryInfo)children[i], level + 1, childNumber) :
                      new IsoFile((FileInfo)children[i], childNumber));

                    Progress?.Invoke(this, new ProgressEventArgs(i));
                }
            }

            Children.Sort();

            CalculateSize();
        }

        /// <summary>
        /// For ROOT Only !!
        /// </summary>
        public IsoDirectory(DirectoryInfo directory, UInt32 level, string childNumber, ProgressDelegate Progress) : base(directory, true, childNumber)
        {
            Parent = this;

            Initialize(directory, level, Progress);
        }

        public IsoDirectory(IsoDirectory parent, DirectoryInfo directory, UInt32 level, string childNumber) : base(directory, false, childNumber)
        {
            Parent = parent;

            Initialize(directory, level, null);
        }

        #endregion Constructor for Real directories

        #region Constructor for Virtual directories (TreeNode)

        private void Initialize(TreeNode directory, UInt32 level, ProgressDelegate Progress)
        {
            Level = level;

            TreeNode[] children = directory.GetAllChildren();

            if (children != null)
            {
                Progress?.Invoke(this, new ProgressEventArgs(0, children.Length));
                int childNumberLength = children.Length.ToString().Length;
                for (int i = 0; i < children.Length; i++)
                {
                    string childNumber = String.Format("{0:D" + childNumberLength.ToString() + "}", i);

                    Children.Add(children[i].IsDirectory ? (IsoFolderElement)
                      new IsoDirectory(this, children[i], level + 1, childNumber) :
                      new IsoFile(children[i], childNumber));

                    Progress?.Invoke(this, new ProgressEventArgs(i));
                }

                Children.Sort();
            }

            CalculateSize();
        }

        /// <summary>
        /// For ROOT Only !!
        /// </summary>
        public IsoDirectory(TreeNode directory, UInt32 level, string childNumber, ProgressDelegate Progress)
            : base(directory, true, childNumber)
        {
            Parent = this;

            Initialize(directory, level, Progress);
        }

        public IsoDirectory(IsoDirectory parent, TreeNode directory, UInt32 level, string childNumber)
            : base(directory, false, childNumber)
        {
            Parent = parent;

            Initialize(directory, level, null);
        }

        #endregion Constructor for Virtual directories (TreeNode)

        #region Properties

        /// <summary>
        /// Calculates the size of the current directory (including all subdirectories, but not files).
        /// </summary>
        public UInt32 TotalDirSize
        {
            get
            {
                UInt32 result = (Size1 + Size2) / IsoAlgorithm.SectorSize;

                foreach (IsoFolderElement child in Children)
                    if (child.IsDirectory)
                        result += ((IsoDirectory)child).TotalDirSize;

                return result;
            }
        }

        /// <summary>
        /// Calculates the size of the current directory (including files and folders).
        /// </summary>
        public UInt32 TotalSize
        {
            get
            {
                UInt32 result = (Size1 + Size2) / IsoAlgorithm.SectorSize;
                foreach (IsoFolderElement child in Children)
                {
                    if (!child.IsDirectory)
                    {
                        result += child.Size1 / IsoAlgorithm.SectorSize;
                        if (child.Size1 % IsoAlgorithm.SectorSize != 0)
                            result++;
                    }
                    else
                        result += ((IsoDirectory)child).TotalSize;
                }

                return result;
            }
        }

        /// <summary>
        /// The children (that is the files and subfolders) of the directory.
        /// </summary>
        public FolderElementList Children { get; private set; } = new FolderElementList();

        public IsoDirectory Parent { get; private set; }

        public UInt32 Level { get; private set; }

        /// <summary>
        /// The number needed in the path table as parent number.
        /// </summary>
        public UInt16 Number { get; set; }

        /// <summary>
        /// The number of the first sector occupied by the short-ascii-named directory.
        /// </summary>
        public override UInt32 Extent1 { get; set; }

        /// <summary>
        /// The number of the first sector occupied by the full-unicode-named directory.
        /// </summary>
        public override UInt32 Extent2 { get; set; }

        /// <summary>
        /// Size1 in bytes.
        /// </summary>
        public override UInt32 Size1 => m_size1 * IsoAlgorithm.SectorSize;

        /// <summary>
        /// Size2 in bytes.
        /// </summary>
        public override UInt32 Size2 => m_size2 * IsoAlgorithm.SectorSize;

        public override bool IsDirectory => true;

        #endregion Properties

        #region I/O Methods

        public void WriteFiles(BinaryWriter writer, ProgressDelegate Progress)
        {
            foreach (IsoFolderElement child in Children)
            {
                if (!child.IsDirectory)
                {
                    ((IsoFile)child).Write(writer, Progress);
                    Progress?.Invoke(this, new ProgressEventArgs((int)(writer.BaseStream.Length / IsoAlgorithm.SectorSize)));
                }
            }

            foreach (IsoFolderElement child in Children)
            {
                if (child.IsDirectory)
                {
                    ((IsoDirectory)child).WriteFiles(writer, Progress);
                    Progress?.Invoke(this, new ProgressEventArgs((int)(writer.BaseStream.Length / IsoAlgorithm.SectorSize)));
                }
            }
        }

        public void Write(BinaryWriter writer, VolumeType type)
        {
            DirectoryRecordWrapper record;

            UInt32 extent = (type == VolumeType.Primary) ? Extent1 : Extent2;
            UInt32 size = (type == VolumeType.Primary) ? Size1 : Size2;
            UInt32 parentExtent = (type == VolumeType.Primary) ? Parent.Extent1 : Parent.Extent2;
            UInt32 parentSize = (type == VolumeType.Primary) ? Parent.Size1 : Parent.Size2;

            // First write the current directory and the parent. ("." and "..")
            record = new DirectoryRecordWrapper(extent, size, Date, IsDirectory, ".");
            record.Write(writer);

            record = new DirectoryRecordWrapper(parentExtent, parentSize, Parent.Date, Parent.IsDirectory, "..");
            record.Write(writer);

            // Everything else is written after the first two sectors (current dir and parent).
            int position = 2 * IsoAlgorithm.DefaultDirectoryRecordLength;

            foreach (IsoFolderElement child in Children)
            {
                UInt32 childExtent = (type == VolumeType.Primary) ? child.Extent1 : child.Extent2;
                UInt32 childSize = (type == VolumeType.Primary) ? child.Size1 : child.Size2;
                string childName = (type == VolumeType.Primary) ? child.ShortName : child.LongName;

                record = new DirectoryRecordWrapper(childExtent, childSize, child.Date, child.IsDirectory, childName)
                {
                    VolumeDescriptorType = type
                };

                if (record.Length + position > IsoAlgorithm.SectorSize)
                {
                    writer.Write(new byte[IsoAlgorithm.SectorSize - position]);
                    position = 0;
                }
                position += record.Write(writer);
            }

            writer.Write(new byte[IsoAlgorithm.SectorSize - position]);
        }

        public int WritePathTable(BinaryWriter writer, bool isRoot, Endian endian, VolumeType type)
        {
            UInt32 extent = (type == VolumeType.Primary) ? Extent1 : Extent2;
            string name = (type == VolumeType.Primary) ? ShortName : LongName;

            var pathTableRecord = new PathTableRecordWrapper(extent, Parent.Number, isRoot ? "." : name)
            {
                VolumeDescriptorType = type,
                Endian = endian
            };

            return pathTableRecord.Write(writer);
        }

        #endregion I/O Methods

        #region Set Extent Methods

        public void SetFilesExtent(ref UInt32 currentExtent)
        {
            foreach (IsoFolderElement child in Children)
            {
                if (!child.IsDirectory)
                {
                    if (child.Size1 == 0)
                        child.Extent1 = 0;
                    else
                    {
                        child.Extent1 = currentExtent;
                        currentExtent += child.Size1 / IsoAlgorithm.SectorSize;
                        if (child.Size1 % IsoAlgorithm.SectorSize != 0)
                            currentExtent++;
                    }
                }
            }

            foreach (IsoFolderElement child in Children)
                if (child.IsDirectory)
                    ((IsoDirectory)child).SetFilesExtent(ref currentExtent);
        }

        public static void SetExtent1(ArrayList stack, int index, UInt32 currentExtent)
        {
            if (index >= stack.Count) return;

            IsoDirectory currentDir = (IsoDirectory)stack[index];
            currentDir.Extent1 = currentExtent;

            UInt32 newCurrentExtent = currentExtent + currentDir.Size1 / IsoAlgorithm.SectorSize;
            foreach (IsoFolderElement child in currentDir.Children)
                if (child.IsDirectory)
                    stack.Add(child);

            SetExtent1(stack, index + 1, newCurrentExtent);
        }

        public static void SetExtent2(ArrayList stack, int index, UInt32 currentExtent)
        {
            if (index >= stack.Count) return;

            IsoDirectory currentDir = (IsoDirectory)stack[index];
            currentDir.Extent2 = currentExtent;

            UInt32 newCurrentExtent = currentExtent + currentDir.Size2 / IsoAlgorithm.SectorSize;
            foreach (IsoFolderElement child in currentDir.Children)
                if (child.IsDirectory)
                    stack.Add(child);
            SetExtent2(stack, index + 1, newCurrentExtent);
        }

        #endregion Set Extent Methods
    }
}