using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace BlueprintSystem
{
    public class BlueprintGraph : EditorWindow
    {
        private BlueprintGraphView _graphView;
        private string _fileName = "New Blueprint";

        [MenuItem("Blueprint/New Blueprint")]
        public static void OpenBlueprintGraphWindow()
        {
            var window = GetWindow<BlueprintGraph>();
            window.titleContent = new GUIContent("Blueprint");
        }

        private void OnEnable()
        {
            ConstrcutGraph();
            GenerateToolbar();
            GenerateMiniMap();
            GenerateBlackBoard();

        }

        private void GenerateBlackBoard()
        {
            var blackboard = new Blackboard(_graphView);
            blackboard.Add(new BlackboardSection { title = "Exposed Variables" });
            blackboard.addItemRequested = _blackboard =>
            {
                _graphView.AddPropertyToBlackBoard(ExposedProperty.CreateInstance(), false);
            };
            blackboard.editTextRequested = (_blackboard, element, newValue) =>
            {
                var oldPropertyName = ((BlackboardField)element).text;
                if (_graphView.ExposedProperties.Any(x => x.PropertyName == newValue))
                {
                    EditorUtility.DisplayDialog("Error", "This property name already exists, please chose another one.",
                        "OK");
                    return;
                }

                var targetIndex = _graphView.ExposedProperties.FindIndex(x => x.PropertyName == oldPropertyName);
                _graphView.ExposedProperties[targetIndex].PropertyName = newValue;
                ((BlackboardField)element).text = newValue;
            };
            blackboard.SetPosition(new Rect(10, 30, 200, 300));
            _graphView.Add(blackboard);
            _graphView.Blackboard = blackboard;
        }

        private void GenerateMiniMap()
        {
            var miniMap = new MiniMap { anchored = true };
            var cords = _graphView.contentViewContainer.WorldToLocal(new Vector2(this.maxSize.x - 10, 30));
            miniMap.SetPosition(new Rect(cords.x, cords.y, 200, 140));
            _graphView.Add(miniMap);
        }

        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();

            var fileNameTextField = new TextField("File Name");
            fileNameTextField.SetValueWithoutNotify(_fileName);
            fileNameTextField.MarkDirtyRepaint();
            fileNameTextField.RegisterValueChangedCallback(evt => _fileName = evt.newValue);
            toolbar.Add(fileNameTextField);

            toolbar.Add(new Button(() => RequestDataOperation(true)) { text = "Save Data" });
            toolbar.Add(new Button(() => RequestDataOperation(false)) { text = "Load Data" });

            var nodeCreateButton = new Button(() => { _graphView.CreateNode("New Node", Vector2.zero); });
            nodeCreateButton.text = "Create Node";
            toolbar.Add(nodeCreateButton);
            rootVisualElement.Add(toolbar);
        }
        private void ConstrcutGraph()
        {
            _graphView = new BlueprintGraphView(this)
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

        private void RequestDataOperation(bool save)
        {
            if (string.IsNullOrEmpty(_fileName))
            {
                EditorUtility.DisplayDialog("Invalid file name!", "Please Enter a valid file name", "OK");
            }
            var graphSaveUtility = SaveUtility.GetInstance(_graphView);
            if (save)
            {
                graphSaveUtility.SaveGraph(_fileName);
            }
            else
            {
                graphSaveUtility.LoadGraph(_fileName);
            }
        }

    }

}
