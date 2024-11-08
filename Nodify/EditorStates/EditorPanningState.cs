using System.Windows;
using System.Windows.Input;

namespace Nodify
{
    /// <summary>The panning state of the editor.</summary>
    public class EditorPanningState : EditorState
    {
        private Point _initialMousePosition;
        private Point _previousMousePosition;
        private Point _currentMousePosition;

        /// <summary>Constructs an instance of the <see cref="EditorPanningState"/> state.</summary>
        /// <param name="editor">The owner of the state.</param>
        public EditorPanningState(NodifyEditor editor) : base(editor)
        {
        }

        /// <inheritdoc />
        public override void Exit()
            => Editor.IsPanning = false;

        /// <inheritdoc />
        public override void Enter(EditorState? from)
        {
            _initialMousePosition = Mouse.GetPosition(Editor);
            _previousMousePosition = _initialMousePosition;
            _currentMousePosition = _initialMousePosition;
            Editor.IsPanning = true;
        }

        /// <inheritdoc />
        public override void HandleMouseMove(MouseEventArgs e)
        {
            _currentMousePosition = e.GetPosition(Editor);
            Editor.ViewportLocation -= (_currentMousePosition - _previousMousePosition) / Editor.ViewportZoom;
            _previousMousePosition = _currentMousePosition;
        }

        /// <inheritdoc />
        public override void HandleMouseUp(MouseButtonEventArgs e)
        {
            EditorGestures.NodifyEditorGestures gestures = EditorGestures.Mappings.Editor;
            if (gestures.Pan.Matches(e.Source, e))
            {
                // Handle right click if panning and moved the mouse more than threshold so context menu doesn't open
                if (e.ChangedButton == MouseButton.Right)
                {
                    double contextMenuTreshold = NodifyEditor.HandleRightClickAfterPanningThreshold * NodifyEditor.HandleRightClickAfterPanningThreshold;
                    if ((_currentMousePosition - _initialMousePosition).LengthSquared > contextMenuTreshold)
                    {
                        e.Handled = true;
                    }
                }

                PopState();
            }
            else if (gestures.Selection.Select.Matches(e.Source, e) && Editor.IsSelecting)
            {
                PopState();
                // Cancel selection and continue panning
                if (Editor.State is EditorSelectingState && !Editor.DisablePanning)
                {
                    PopState();
                    PushState(new EditorPanningState(Editor));
                }
            }
        }
    }

    public class EditorVerticalPanningState : EditorState
    {
        private readonly float _delta;
        private readonly float _speed = 0.5f;
        /// <summary>Constructs an instance of the <see cref="EditorPanningState"/> state.</summary>
        /// <param name="editor">The owner of the state.</param>
        public EditorVerticalPanningState(NodifyEditor editor, float delta) : base(editor)
        {
            _delta = delta;
        }

        /// <inheritdoc />
        public override void Exit()
            => Editor.IsPanning = false;

        /// <inheritdoc />
        public override void Enter(EditorState? from)
        {
            Editor.IsPanning = true;
            Editor.ViewportLocation = new Point(Editor.ViewportLocation.X, Editor.ViewportLocation.Y + (_speed * -_delta) / Editor.ViewportZoom);
            PopState();
        }

        // XXX : Could also handle key down / up to allow arrow key panning (using a DispatcherTimer ?)
        // XXX : Would require to add a direction information to the gesture to know in which direction the input is going (similar to wheel delta)
    }

    public class EditorHorizontalPanningState : EditorState
    {
        private readonly float _delta;
        private readonly float _speed = 0.5f;
        /// <summary>Constructs an instance of the <see cref="EditorPanningState"/> state.</summary>
        /// <param name="editor">The owner of the state.</param>
        public EditorHorizontalPanningState(NodifyEditor editor, float delta) : base(editor)
        {
            _delta = delta;
        }

        /// <inheritdoc />
        public override void Exit()
            => Editor.IsPanning = false;

        /// <inheritdoc />
        public override void Enter(EditorState? from)
        {
            Editor.IsPanning = true;
            Editor.ViewportLocation = new Point(Editor.ViewportLocation.X + (_speed * -_delta) / Editor.ViewportZoom, Editor.ViewportLocation.Y);
            PopState();
        }
    }
}
