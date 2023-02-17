using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace Nodelist
{
    public class NodelistReader : MonoBehaviour
    {
        public static void InitializeNodelist(string content, string saveName)
        {
            NodelistContent NLC = new NodelistContent(saveName);
            string[] lines = content.Split(Environment.NewLine.ToCharArray());
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("node:"))
                {
                    Node newNode = new Node(lines[i].Split(':')[1]);
                    i++;
                    for (;;i++)
                    {
                        if (lines[i] == "endNode")
                        {
                            NLC.nodes.Add(newNode);
                            break;
                        }
                        else if (lines[i].Trim() != "")
                        {
                            newNode.lines.Add(lines[i]);
                        }
                    }
                }
            }
            storedNodelistFiles.Add(NLC);
        }

        public static List<NodelistContent> storedNodelistFiles = new List<NodelistContent>();

        public static NodelistContent GetNodelistFile(string fileName)
        {
            return storedNodelistFiles.Find(x => x.fileName == fileName);
        }
    }

    public class NodelistContent
    {
        public List<Node> nodes = new List<Node>();

        public string fileName;

        public NodelistContent(string name)
        {
            this.fileName = name;
        }

        public Node GetNode(string nodeName)
        {
            return nodes.Find(x => x.name == nodeName);
        }
    }

    public class Node
    {
        public string name;

        public List<string> lines = new List<string>();

        public Node(string nodeName)
        {
            this.name = nodeName;
        }
    }
}
