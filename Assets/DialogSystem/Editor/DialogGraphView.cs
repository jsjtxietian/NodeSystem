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

namespace DialogueSystem.Editor
{
    public class DialogGraphView : GraphView
    {
        private readonly Vector2 defaultNodeSize = new Vector2(150, 200);
        public DialogGraphView()
        {
            styleSheets.Add(Resources.Load<StyleSheet>("DialogGraph"));
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            AddElement(GenerateEntryPointNode());
        }

        private DialogueNode CreateDialogNode(string nodeName)
        {
            var node = new DialogueNode
            {
                title = nodeName,
                GUID = Guid.NewGuid().ToString(),
                EntyPoint = false
            };

            var inputPort = GeneratePort(node, Direction.Input, Port.Capacity.Multi);
            inputPort.portName = "Input";
            node.inputContainer.Add(inputPort);

            var button = new Button(() => { AddChoicePort(node); })
            {
                text = "Add Choice"
            };

            node.titleContainer.Add(button);

            node.RefreshExpandedState();
            node.RefreshPorts();
            node.SetPosition(new Rect(Vector2.zero, defaultNodeSize));

            return node;
        }

        private void AddChoicePort(DialogueNode node)
        {
            var port = GeneratePort(node, Direction.Output);
            int outputPortCount = node.outputContainer.Query("connector").ToList().Count;
            port.portName = $"Choice {outputPortCount}";

            node.outputContainer.Add(port);
            node.RefreshExpandedState();
            node.RefreshPorts();
        }

        public void CreateNode(string nodeName)
        {
            AddElement(CreateDialogNode(nodeName));
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

        private Port GeneratePort(DialogueNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
        {
            return node.InstantiatePort(
                Orientation.Horizontal,
                portDirection,
                capacity,
                typeof(float)
            );
        }

        private DialogueNode GenerateEntryPointNode()
        {
            var node = new DialogueNode
            {
                title = "START",
                Text = "GO",
                GUID = Guid.NewGuid().ToString(),
                EntyPoint = true
            };
            var port = GeneratePort(node, Direction.Output);
            port.portName = "Next";
            node.outputContainer.Add(port);

            node.RefreshExpandedState();
            node.RefreshPorts();

            node.SetPosition(new Rect(100, 200, 100, 150));
            return node;
        }
    }

}
