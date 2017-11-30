using IsoCreatorLib;

namespace BER.CDCat.Export
{
    /// <summary>
    /// This class implements the interface required by CDCat for its export plugins.
    /// It is used only when compiled as external library, and only by CDCat.
    /// </summary>
    public class ExportIso : IExportPlugin
    {
        public ExportIso()
        {
            Creator.Progress += delegate (object sender, ProgressEventArgs e) { Progress?.Invoke(sender, e); };
            Creator.Abort += delegate (object sender, AbortEventArgs e) { Abort?.Invoke(sender, e); };
            Creator.Finish += delegate (object sender, FinishEventArgs e) { Finished?.Invoke(sender, e); };
        }

        private IsoCreator Creator { get; } = new IsoCreator();

        public string ID => "ExportISO";

        public string Name => "ISO";

        public string Extension => "iso";

        public string Description => "CD image with virtual files";

        public TreeNode Volume { get; set; }

        public string FileName { get; set; }

        public void DoExport()
        {
            if (Volume != null && FileName != null)
                Creator.Tree2Iso(Volume, FileName);
        }

        public void DoExport(TreeNode volume, string fileName)
        {
            Volume = volume;
            FileName = fileName;
            Creator.Tree2Iso(Volume, FileName);
        }

        public event ProgressDelegate Progress;

        public event FinishDelegate Finished;

        public event AbortDelegate Abort;

        public override string ToString() => Name;
    }
}