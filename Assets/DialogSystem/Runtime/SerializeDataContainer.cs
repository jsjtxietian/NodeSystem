using System;
using System.Collections.Generic;
using UnityEngine;
namespace BlueprintSystem
{
    [Serializable]
    public class SerializeDataContainer : ScriptableObject
    {
        public List<NodeLinkData> NodeLinks = new List<NodeLinkData>();
        public List<BlueprintNodeData> BlueprintNodeData = new List<BlueprintNodeData>();
        public List<ExposedProperty> ExposedProperties = new List<ExposedProperty>();
    }
}

