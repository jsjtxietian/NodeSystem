using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using DialogueSystem;
using UnityEngine.UIElements;

namespace DialogueSystem
{
    public class GraphSaveUtility
    {
        private List<Edge> Edges => _graphView.edges.ToList();
        private List<DialogNode> Nodes => _graphView.nodes.ToList().Cast<DialogNode>().ToList();
        private DialogGraphView _graphView;
        private DialogContainer _dialogContainer;

        public static GraphSaveUtility GetInstance(DialogGraphView graphView)
        {
            return new GraphSaveUtility
            {
                _graphView = graphView
            };
        }

        public void SaveGraph(string fileName)
        {
            if (!Edges.Any())
                return;

            var dialogContainerCenter = ScriptableObject.CreateInstance<DialogContainer>();
            var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();
            for (int i = 0; i < connectedPorts.Length; i++)
            {
                DialogNode output = connectedPorts[i].output.node as DialogNode;
                DialogNode input = connectedPorts[i].input.node as DialogNode;

                dialogContainerCenter.NodeLinks.Add(new NodeLinkData
                {
                    BaseNodeGUID = output.GUID,
                    PortName = connectedPorts[i].output.name,
                    TargetNodeGUID = input.GUID
                });
            }

            foreach (var node in Nodes.Where(node => !node.EntyPoint))
            {
                dialogContainerCenter.DialogNodeData.Add(new DialogNodeData
                {
                    NodeGUID = node.GUID,
                    DialogueText = node.Text,
                    Position = node.GetPosition().position
                });
            }

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");

            AssetDatabase.CreateAsset(dialogContainerCenter, $"Assets/Resources/{fileName}.asset");
            AssetDatabase.SaveAssets();

        }

        public void LoadGraph(string fileName)
        {
            _dialogContainer = Resources.Load<DialogContainer>(fileName);
            if (_dialogContainer == null)
            {
                EditorUtility.DisplayDialog("File Not Found", "Target Data does not exist!", "OK");
                return;
            }

            ClearGraph();
            CreateNodes();
            ConnectNodes();
        }

        private void ClearGraph()
        {
            Nodes.Find(x => x.EntyPoint).GUID = _dialogContainer.NodeLinks[0].BaseNodeGUID;
            foreach (var perNode in Nodes)
            {
                if (perNode.EntyPoint) continue;
                Edges.Where(x => x.input.node == perNode).ToList()
                    .ForEach(edge => _graphView.RemoveElement(edge));
                _graphView.RemoveElement(perNode);
            }
        }

        private void CreateNodes()
        {
            foreach (var perNode in _dialogContainer.DialogNodeData)
            {
                var tempNode = _graphView.CreateDialogNode(perNode.DialogueText, Vector2.zero);
                tempNode.GUID = perNode.NodeGUID;
                _graphView.AddElement(tempNode);

                var nodePorts = _dialogContainer.NodeLinks.Where(x => x.BaseNodeGUID == perNode.NodeGUID).ToList();
                nodePorts.ForEach(x => _graphView.AddChoicePort(tempNode, x.PortName));
            }
        }


        private void ConnectNodes()
        {
            for (var i = 0; i < Nodes.Count; i++)
            {
                var k = i; //Prevent access to modified closure
                var connections = _dialogContainer.NodeLinks.Where(x => x.BaseNodeGUID == Nodes[k].GUID).ToList();
                for (var j = 0; j < connections.Count(); j++)
                {
                    var targetNodeGUID = connections[j].TargetNodeGUID;
                    var targetNode = Nodes.First(x => x.GUID == targetNodeGUID);
                    LinkNodesTogether(Nodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);

                    targetNode.SetPosition(new Rect(
                        _dialogContainer.DialogNodeData.First(x => x.NodeGUID == targetNodeGUID).Position,
                        _graphView.defaultNodeSize));
                }
            }
        }

        private void LinkNodesTogether(Port outputSocket, Port inputSocket)
        {
            var tempEdge = new Edge()
            {
                output = outputSocket,
                input = inputSocket
            };
            tempEdge?.input.Connect(tempEdge);
            tempEdge?.output.Connect(tempEdge);
            _graphView.Add(tempEdge);
        }

    }

}