using System;
using System.Collections;

namespace IsoCreatorLib.DirectoryTree
{
    /// <summary>
    /// This class represents a collection of folder elements (files and folders) which can be sorted by name.
    /// </summary>
    internal class FolderElementList : CollectionBase
    {

        public class DirEntryComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                string nameX = ((IsoFolderElement)x).LongName;
                string nameY = ((IsoFolderElement)y).LongName;

                return String.Compare(nameX, nameY, false);
            }
        }

        public void Add(IsoFolderElement value) => InnerList.Add(value);


        public void Sort() => InnerList.Sort(new DirEntryComparer());

        public IsoFolderElement this[int index]
        {
            get => (IsoFolderElement)InnerList[index];
            set => InnerList[index] = value;
        }
    }
}
