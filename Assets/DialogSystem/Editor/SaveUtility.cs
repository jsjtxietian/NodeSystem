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
using BlueprintSystem;
using UnityEngine.UIElements;

namespace BlueprintSystem
{
    public class SaveUtility
    {
        private List<Edge> Edges => _graphView.edges.ToList();
        private List<BlueprintNode> Nodes => _graphView.nodes.ToList().Cast<BlueprintNode>().ToList();
        private BlueprintGraphView _graphView;
        private SerializeDataContainer _SerializeDataContainer;

        public static SaveUtility GetInstance(BlueprintGraphView graphView)
        {
            return new SaveUtility
            {
                _graphView = graphView
            };
        }

        public void SaveGraph(string fileName)
        {
            if (!Edges.Any())
                return;

            var SerializeDataContainerCenter = ScriptableObject.CreateInstance<SerializeDataContainer>();
            var connectedPorts = Edges.Where(x => x.input.node != null).ToArray();
            for (int i = 0; i < connectedPorts.Length; i++)
            {
                BlueprintNode output = connectedPorts[i].output.node as BlueprintNode;
                BlueprintNode input = connectedPorts[i].input.node as BlueprintNode;

                SerializeDataContainerCenter.NodeLinks.Add(new NodeLinkData
                {
                    BaseNodeGUID = output.GUID,
                    PortName = connectedPorts[i].output.portName,
                    TargetNodeGUID = input.GUID
                });
            }

            foreach (var node in Nodes.Where(node => !node.EntyPoint))
            {
                SerializeDataContainerCenter.BlueprintNodeData.Add(new BlueprintNodeData
                {
                    NodeGUID = node.GUID,
                    Content = node.Text,
                    Position = node.GetPosition().position
                });
            }

            SaveExposedProperties(SerializeDataContainerCenter);

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");

            AssetDatabase.CreateAsset(SerializeDataContainerCenter, $"Assets/Resources/{fileName}.asset");
            AssetDatabase.SaveAssets();

        }

        public void LoadGraph(string fileName)
        {
            _SerializeDataContainer = Resources.Load<SerializeDataContainer>(fileName);
            if (_SerializeDataContainer == null)
            {
                EditorUtility.DisplayDialog("File Not Found", "Target Data does not exist!", "OK");
                return;
            }

            ClearGraph();
            CreateNodes();
            ConnectNodes();
            AddExposedProperties();
        }

        private void AddExposedProperties()
        {
            _graphView.ClearBlackBoardAndExposedProperties();
            foreach (var exposedProperty in _SerializeDataContainer.ExposedProperties)
            {
                _graphView.AddPropertyToBlackBoard(exposedProperty);
            }
        }

        private void ClearGraph()
        {
            Nodes.Find(x => x.EntyPoint).GUID = _SerializeDataContainer.NodeLinks[0].BaseNodeGUID;
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
            foreach (var perNode in _SerializeDataContainer.BlueprintNodeData)
            {
                var tempNode = _graphView.CreateDialogNode(perNode.Content, Vector2.zero);
                tempNode.GUID = perNode.NodeGUID;
                _graphView.AddElement(tempNode);

                var nodePorts = _SerializeDataContainer.NodeLinks.Where(x => x.BaseNodeGUID == perNode.NodeGUID).ToList();
                nodePorts.ForEach(x => _graphView.AddChoicePort(tempNode, x.PortName));
            }
        }


        private void ConnectNodes()
        {
            for (var i = 0; i < Nodes.Count; i++)
            {
                var k = i; //Prevent access to modified closure
                var connections = _SerializeDataContainer.NodeLinks.Where(x => x.BaseNodeGUID == Nodes[k].GUID).ToList();
                for (var j = 0; j < connections.Count(); j++)
                {
                    var targetNodeGUID = connections[j].TargetNodeGUID;
                    var targetNode = Nodes.First(x => x.GUID == targetNodeGUID);
                    LinkNodesTogether(Nodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);

                    targetNode.SetPosition(new Rect(
                        _SerializeDataContainer.BlueprintNodeData.First(x => x.NodeGUID == targetNodeGUID).Position,
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

        private void SaveExposedProperties(SerializeDataContainer SerializeDataContainer)
        {
            SerializeDataContainer.ExposedProperties.Clear();
            SerializeDataContainer.ExposedProperties.AddRange(_graphView.ExposedProperties);
        }

    }

}