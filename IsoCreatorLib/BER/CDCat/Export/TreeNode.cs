using System;
using System.Text;

namespace BER.CDCat.Export
{
    public class TreeNode
    {
        public TreeNodeCollection Files { get; } = new TreeNodeCollection();

        public TreeNodeCollection Directories { get; } = new TreeNodeCollection();

        public string Name { get; set; } = "";

        public string ShortName { get; set; } = "";

        public UInt32 Length { get; set; }

        public DateTime CreationTime { get; set; }

        public bool IsDirectory { get; set; }

        public string FullName { get; set; } = "";

        public TreeNode[] GetFiles() => Files.ToArray();

        public TreeNode[] GetDirectories() => Directories.ToArray();

        public TreeNode[] GetAllChildren()
        {
            TreeNodeCollection result = new TreeNodeCollection();
            result.AddRange(Directories);
            result.AddRange(Files);
            return result.ToArray();
        }

        /// <summary>
        /// Debug purposes: generates a rudimentary xml.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var str = new StringBuilder($"<node name=\"{ Name }\" dir=\"true\">");

            foreach (TreeNode dir in Directories)
                str.Append(dir.ToString());

            foreach (TreeNode file in Files)
                str.Append($"<node name=\"{ file.Name }\" dir=\"false\"/>");

            str.Append("</node>");

            return str.ToString();
        }
    }
}