using System;
using System.Linq;
using System.Windows.Forms;
using PhotoshopFile;
using PSDMapper.Properties;

namespace PSDMapper
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();

            TopMost = true;
            AllowDrop = true;
        }

        private void Main_Load(object sender, EventArgs ev)
        {
            #region Form location

            Location = Settings.Default.StartLocation;

            FormClosing += (s, e) =>
            {
                Settings.Default.StartLocation = Location;
                Settings.Default.Save();
            };

            #endregion

            DragEventHandler enter = (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = e.Data.GetData(DataFormats.FileDrop, false) as string[];
                    if (files.FirstOrDefault<string>().EndsWith(".psd", StringComparison.OrdinalIgnoreCase))
                    {
                        e.Effect = DragDropEffects.Copy;
                    }
                }
            };
            DragEventHandler drop = (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string file = (e.Data.GetData(DataFormats.FileDrop, false) as string[]).FirstOrDefault<string>();
                    PsdFile ps = new PsdFile();
                    ps.Load(file);
                    MapLayers(ps);
                }
            };

            base.DragEnter += enter;
            base.DragDrop += drop;
        }

        public void MapLayers(PsdFile ps)
        {
            output.Clear();

            ps.Layers.OrderBy(l => l.Rect.X).ThenBy(l => l.Rect.Y)
                .ToList()
                .ForEach(l =>
                {
                    output.AppendText(
                        String.Format(".{0} {{ width: {1}px; height: {2}px; background-position: {3}px {4}px; }}{5}",
                            l.Name,
                            l.Rect.Width,
                            l.Rect.Height,
                            l.Rect.X != 0 ? -l.Rect.X : 0,
                            l.Rect.Y != 0 ? -l.Rect.Y : 0,
                            Environment.NewLine
                        )
                    );
                });
        }

        private void pin_Click(object sender, EventArgs e)
        {
            TopMost = !TopMost;
        }

        private void copy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(output.Text);
        }
    }
}
