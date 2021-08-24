using System;
using System.Collections.Generic;
using UnityEngine;

namespace BlueprintSystem
{
    [Serializable]
    public class BlueprintNodeData
    {
        public string NodeGUID;
        public string DialogueText;
        public Vector2 Position;
    }

    [System.Serializable]
    public class ExposedProperty
    {
        public static ExposedProperty CreateInstance()
        {
            return new ExposedProperty();
        }

        public string PropertyName = "New String";
        public string PropertyValue = "New Value";
    }

    [Serializable]
    public class NodeLinkData
    {
        public string BaseNodeGUID;
        public string PortName;
        public string TargetNodeGUID;
    }

}