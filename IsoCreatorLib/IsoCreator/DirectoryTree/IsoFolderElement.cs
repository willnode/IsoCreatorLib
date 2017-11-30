using BER.CDCat.Export;
using System;
using System.IO;

namespace IsoCreatorLib.DirectoryTree
{
    /// <summary>
    /// Base class for all folder elements (files and subfolders).
    /// </summary>
    internal abstract class IsoFolderElement
    {
        public IsoFolderElement(FileSystemInfo folderElement, bool isRoot, string childNumber)
        {
            Date = folderElement.CreationTime;
            LongName = folderElement.Name;

            // If you need to use the short name, then you may want to change the naming method.
            if (isRoot)
            {
                ShortName = ".";
                LongName = ".";
            }
            else
            {
                if (LongName.Length > 8)
                {
                    ShortName = LongName.Substring(0, 8 - childNumber.Length).ToUpper().Replace(' ', '_').Replace('.', '_');
                    ShortName += childNumber;
                }
                else
                {
                    ShortName = LongName.ToUpper().Replace(' ', '_').Replace('.', '_');
                }
            }

            if (LongName.Length > IsoAlgorithm.FileNameMaxLength)
            {
                LongName = LongName.Substring(0, IsoAlgorithm.FileNameMaxLength - childNumber.Length) + childNumber;
            }
        }

        public IsoFolderElement(TreeNode folderElement, bool isRoot, string childNumber)
        {
            Date = folderElement.CreationTime;
            LongName = folderElement.Name;

            // If you need to use the short name, then you may want to change the naming method.
            if (isRoot)
            {
                ShortName = ".";
                LongName = ".";
            }
            else
            {
                if (LongName.Length > 8)
                {
                    ShortName = LongName.Substring(0, 8 - childNumber.Length).ToUpper().Replace(' ', '_').Replace('.', '_');
                    ShortName += childNumber;
                }
                else
                {
                    ShortName = LongName.ToUpper().Replace(' ', '_').Replace('.', '_');
                }
            }

            if (LongName.Length > IsoAlgorithm.FileNameMaxLength)
            {
                LongName = LongName.Substring(0, IsoAlgorithm.FileNameMaxLength - childNumber.Length) + childNumber;
            }
        }

        public abstract UInt32 Extent1 { get; set; }

        public abstract UInt32 Extent2 { get; set; }

        public abstract UInt32 Size1 { get; }

        public abstract UInt32 Size2 { get; }

        public abstract bool IsDirectory { get; }

        public DateTime Date { get; }

        /// <summary>
        /// The shortIdent is used for the DOS short-ascii-name. I haven't given too much
        /// effort into making it right. It isn't of much use these days.
        /// </summary>
		public string ShortName { get; set; }

        /// <summary>
        /// The original name. (unicode n stuff)
        /// </summary>
		public string LongName { get; }
    }
}