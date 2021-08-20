using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace DialogueSystem.Editor
{
    public class DialogGraph : EditorWindow
    {
        private DialogGraphView _graphView;

        [MenuItem("Graph/Dialog Graph")]
        public static void OpenDialogGraphWindow()
        {
            var window = GetWindow<DialogGraph>();
            window.titleContent = new GUIContent("Dialog Graph");
        }

        private void OnEnable()
        {
            ConstrcutGraph();
            GenerateToolbar();
        }

        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();

            var nodeCreateButton = new Button(() => { _graphView.CreateNode("New Node"); });
            nodeCreateButton.text = "Create Node";
            toolbar.Add(nodeCreateButton);
            rootVisualElement.Add(toolbar);
        }
        private void ConstrcutGraph()
        {
            _graphView = new DialogGraphView
            {
                name = "Dialog Graph"
            };

            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
        }
    }

}
