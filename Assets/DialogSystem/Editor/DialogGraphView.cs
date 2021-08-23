using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace DialogueSystem
{
    public class DialogGraphView : GraphView
    {
        public readonly Vector2 defaultNodeSize = new Vector2(150, 200);
        public DialogGraphView()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("DialogGraph"));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale * 2);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            AddElement(GenerateEntryPointNode());
        }

        public DialogNode CreateDialogNode(string nodeName, Vector2 position)
        {
            var node = new DialogNode
            {
                title = nodeName,
                Text = nodeName,
                GUID = Guid.NewGuid().ToString(),
                EntyPoint = false
            };

            node.styleSheets.Add(Resources.Load<StyleSheet>("Node"));

            var inputPort = GeneratePort(node, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Input";
            node.inputContainer.Add(inputPort);

            var textField = new TextField("");
            textField.RegisterValueChangedCallback(evt =>
            {
                node.Text = evt.newValue;
                node.title = evt.newValue;
            });
            textField.SetValueWithoutNotify(node.title);
            node.mainContainer.Add(textField);


            var button = new Button(() => { AddChoicePort(node); })
            {
                text = "Add Choice"
            };

            node.titleContainer.Add(button);

            node.RefreshExpandedState();
            node.RefreshPorts();
            node.SetPosition(new Rect(position, defaultNodeSize));

            return node;
        }

        public void AddChoicePort(DialogNode node, string portName = null)
        {
            var port = GeneratePort(node, Direction.Output);
            var portLabel = port.contentContainer.Q<Label>("type");
            port.contentContainer.Remove(portLabel);

            int outputPortCount = node.outputContainer.Query("connector").ToList().Count;
            var outputPortName = string.IsNullOrEmpty(portName)
                ? $"Option {outputPortCount + 1}"
                : portName;

            var textField = new TextField()
            {
                name = string.Empty,
                value = outputPortName
            };
            textField.RegisterValueChangedCallback(evt => port.portName = evt.newValue);
            port.contentContainer.Add(new Label("  "));
            port.contentContainer.Add(textField);
            var deleteButton = new Button(() => RemovePort(node, port))
            {
                text = "X"
            };
            port.contentContainer.Add(deleteButton);
            port.portName = outputPortName; ;

            node.outputContainer.Add(port);
            node.RefreshExpandedState();
            node.RefreshPorts();
        }
        private void RemovePort(Node node, Port socket)
        {
            var targetEdge = edges.ToList()
                .Where(x => x.output.portName == socket.portName && x.output.node == socket.node);
            if (targetEdge.Any())
            {
                var edge = targetEdge.First();
                edge.input.Disconnect(edge);
                RemoveElement(targetEdge.First());
            }

            node.outputContainer.Remove(socket);
            node.RefreshPorts();
            node.RefreshExpandedState();
        }

        public void CreateNode(string nodeName, Vector2 position)
        {
            AddElement(CreateDialogNode(nodeName, position));
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            var startPortView = startPort;

            ports.ForEach((port) =>
            {
                var portView = port;
                if (startPortView != portView && startPortView.node != portView.node)
                    compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        private Port GeneratePort(DialogNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(
                Orientation.Horizontal,
                portDirection,
                capacity,
                typeof(float)
            );
        }

        private DialogNode GenerateEntryPointNode()
        {
            var node = new DialogNode
            {
                title = "START",
                Text = "GO",
                GUID = Guid.NewGuid().ToString(),
                EntyPoint = true
            };
            var port = GeneratePort(node, Direction.Output);
            port.portName = "Next";
            node.outputContainer.Add(port);

            node.capabilities &= ~Capabilities.Movable;
            node.capabilities &= ~Capabilities.Deletable;

            node.RefreshExpandedState();
            node.RefreshPorts();

            node.SetPosition(new Rect(100, 200, 100, 150));
            return node;
        }
    }

}
