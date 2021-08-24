using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BlueprintSystem
{
    public class BlueprintParser : MonoBehaviour
    {
        [SerializeField] private SerializeDataContainer dataContainer;

        private void Start()
        {
            var data = dataContainer.NodeLinks.First(); //Entrypoint node
            Proceed(data.TargetNodeGUID);
        }

        private void Proceed(string dataGUID)
        {
            var text = dataContainer.BlueprintNodeData.Find(x => x.NodeGUID == dataGUID).Content;
            var choices = dataContainer.NodeLinks.Where(x => x.BaseNodeGUID == dataGUID);
            text = ProcessProperties(text);
            Debug.Log(text);

            foreach (var choice in choices)
            {
                Debug.Log(ProcessProperties(choice.PortName));
            }
        }

        private string ProcessProperties(string text)
        {
            foreach (var exposedProperty in dataContainer.ExposedProperties)
            {
                text = text.Replace($"[{exposedProperty.PropertyName}]", exposedProperty.PropertyValue);
            }
            return text;
        }
    }
}