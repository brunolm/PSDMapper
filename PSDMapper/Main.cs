using System;
using System.Linq;
using System.Windows.Forms;
using PhotoshopFile;
using PSDMapper.Properties;
using System.Drawing;

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
                    var nameTokens = l.Name.Split(new char[] { '-' }, 2);
                    var sizeTokens = nameTokens[0].Split('x');

                    var size = new Size(Convert.ToInt32(sizeTokens[0]), Convert.ToInt32(sizeTokens[1]));
                    var name = nameTokens[1];

                    var x = l.Rect.X - Math.Round((size.Width - l.Rect.Width) / 2.0, MidpointRounding.AwayFromZero);
                    var y = l.Rect.Y - Math.Round((size.Height - l.Rect.Height) / 2.0, MidpointRounding.AwayFromZero);

                    output.AppendText(
                        String.Format(".{0} {{ width: {1}px; height: {2}px; background-position: {3}px {4}px; }}{5}",
                            name,
                            size.Width,
                            size.Height,
                            x,
                            y,
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
