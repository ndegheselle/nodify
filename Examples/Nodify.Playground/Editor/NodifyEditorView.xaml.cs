using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace Nodify.Playground
{
    public partial class NodifyEditorView : UserControl
    {
        public NodifyEditor EditorInstance => Editor;

        public Lazy<Cursor> CursorGrab { get; private set; } = new Lazy<Cursor>(
            () =>
            {
                using (var stream = App.GetResourceStream(new Uri("pack://application:,,,/Nodify.Playground;component/Resources/Cursors/grab.cur")).Stream)
                {
                    return new Cursor(stream);
                }
            });

        public Lazy<Cursor> CursorGrabbing { get; private set; } = new Lazy<Cursor>(
            () =>
            {
                using (var stream = App.GetResourceStream(new Uri("pack://application:,,,/Nodify.Playground;component/Resources/Cursors/grabbing.cur")).Stream)
                {
                    return new Cursor(stream);
                }
            });

        public NodifyEditorView()
        {
            InitializeComponent();
        }

        private void Minimap_Zoom(object sender, ZoomEventArgs e)
        {
            EditorInstance.ZoomAtPosition(e.Zoom, e.Location);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                this.Cursor = CursorGrab.Value;
            }
            base.OnKeyDown(e);
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                this.Cursor = null;
            }
            base.OnKeyDown(e);
        }
    }
}
