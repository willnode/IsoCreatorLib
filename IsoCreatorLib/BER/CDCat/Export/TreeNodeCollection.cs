using System.Collections;

namespace BER.CDCat.Export
{
    public class TreeNodeCollection : CollectionBase
    {
        public int Add(TreeNode node) => InnerList.Add(node);

        public void AddRange(TreeNodeCollection collection) => InnerList.AddRange(collection);

        public void Remove(TreeNode node) => InnerList.Remove(node);

        public TreeNode this[int index]
        {
            get => (TreeNode)InnerList[index];
            set => InnerList[index] = value;
        }

        public TreeNode[] ToArray() => (TreeNode[])InnerList.ToArray(typeof(TreeNode));
    }
}