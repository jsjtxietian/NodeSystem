using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DialogueSystem
{
    public class DialogNode : Node
    {
        public string Text;
        public string GUID;
        public bool EntyPoint = false;
    }
}