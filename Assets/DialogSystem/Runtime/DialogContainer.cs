using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem
{
    [Serializable]
    public class DialogContainer : ScriptableObject
    {
        public List<NodeLinkData> NodeLinks = new List<NodeLinkData>();
        public List<DialogNodeData> DialogNodeData = new List<DialogNodeData>();
        // public List<ExposedProperty> ExposedProperties = new List<ExposedProperty>();
        // public List<CommentBlockData> CommentBlockData = new List<CommentBlockData>();
    }
}