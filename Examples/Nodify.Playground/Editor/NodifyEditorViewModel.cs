using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Nodify.Playground
{
    public class NodifyEditorViewModel : ObservableObject
    {
        public NodifyEditorViewModel()
        {
            Schema = new GraphSchema();

            PendingConnection = new PendingConnectionViewModel
            {
                Graph = this
            };

            DeleteSelectionCommand = new DelegateCommand(DeleteSelection, CanDeleteSelection);
            CommentSelectionCommand = new DelegateCommand(CommentSelection, CanCommentSelection);
            DisconnectConnectorCommand = new RequeryCommand<ConnectorViewModel>(DisconnectConnector);
            CreateConnectionCommand = new DelegateCommand<object>(CreateConnection, CanCreateConnection);

            Connections.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (ConnectionViewModel c in e.NewItems)
                    {
                        c.Graph = this;
                        c.Input.Connections.Add(c);
                        c.Output.Connections.Add(c);
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (ConnectionViewModel c in e.OldItems)
                    {
                        c.Input.Connections.Remove(c);
                        c.Output.Connections.Remove(c);
                    }
                }
            };

            Nodes.CollectionChanged += (s, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (NodeViewModel x in e.NewItems)
                    {
                        x.Graph = this;
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (NodeViewModel x in e.OldItems)
                    {
                        if (x is FlowNodeViewModel flow)
                        {
                            flow.Disconnect();
                        }
                        else if (x is KnotNodeViewModel knot)
                        {
                            knot.Connector.Disconnect();
                        }
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    Connections.Clear();
                }
            };
        }

        private NodifyObservableCollection<NodeViewModel> _nodes = new NodifyObservableCollection<NodeViewModel>();
        public NodifyObservableCollection<NodeViewModel> Nodes
        {
            get => _nodes;
            set => SetProperty(ref _nodes, value);
        }

        private NodifyObservableCollection<NodeViewModel> _selectedNodes = new NodifyObservableCollection<NodeViewModel>();
        public NodifyObservableCollection<NodeViewModel> SelectedNodes
        {
            get => _selectedNodes;
            set => SetProperty(ref _selectedNodes, value);
        }

        private NodifyObservableCollection<ConnectionViewModel> _connections = new NodifyObservableCollection<ConnectionViewModel>();
        public NodifyObservableCollection<ConnectionViewModel> Connections
        {
            get => _connections;
            set => SetProperty(ref _connections, value);
        }

        private Size _viewportSize;
        public Size ViewportSize
        {
            get => _viewportSize;
            set => SetProperty(ref _viewportSize, value);
        }

        public PendingConnectionViewModel PendingConnection { get; }
        public GraphSchema Schema { get; }

        public ICommand DeleteSelectionCommand { get; }
        public ICommand DisconnectConnectorCommand { get; }
        public ICommand CreateConnectionCommand { get; }
        public ICommand CommentSelectionCommand { get; }

        private bool CanDeleteSelection()
        {
            return !EditorSettings.Instance.IsReadOnly && SelectedNodes.Count > 0;
        }

        private void DeleteSelection()
        {
            var selected = SelectedNodes.ToList();

            for (int i = 0; i < selected.Count; i++)
            {
                Nodes.Remove(selected[i]);
            }
        }

        private bool CanCommentSelection()
        {
            return !EditorSettings.Instance.IsReadOnly && SelectedNodes.Count > 0;
        }

        private void CommentSelection()
        {
            Schema.AddCommentAroundNodes(SelectedNodes, "New comment");
        }

        private void DisconnectConnector(ConnectorViewModel c)
        {
            c.Disconnect();
        }

        private bool CanCreateConnection(object target)
        {
            return !EditorSettings.Instance.IsReadOnly && PendingConnection.Source != null && target != null;
        }

        private void CreateConnection(object target)
        {
            Schema.TryAddConnection(PendingConnection.Source!, target);
        }
    }
}
