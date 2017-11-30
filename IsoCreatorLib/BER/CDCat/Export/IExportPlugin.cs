using System;

namespace BER.CDCat.Export
{
    public class FinishEventArgs : EventArgs
    {
        public FinishEventArgs(string message) => Message = message;

        public string Message { get; set; }
    }

    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(int current = -1, int maximum = -1, string action = null)
        {
            Action = action;
            Current = current;
            Maximum = maximum;
        }

        public int Current { get; set; }

        public int Maximum { get; set; }

        public string Action { get; set; }
    }

    public class AbortEventArgs : EventArgs
    {
        public AbortEventArgs(string message) => Message = message;

        public string Message { get; set; }
    }

    public delegate void FinishDelegate(object sender, FinishEventArgs e);

    public delegate void ProgressDelegate(object sender, ProgressEventArgs e);

    public delegate void AbortDelegate(object sender, AbortEventArgs e);

    public interface IExportPlugin
    {
        event ProgressDelegate Progress;

        event FinishDelegate Finished;

        event AbortDelegate Abort;

        string ID { get; }

        string Description { get; }

        string Extension { get; }

        TreeNode Volume { get; set; }

        string FileName { get; set; }

        void DoExport(TreeNode volume, string volumeDescription);

        void DoExport();
    }
}