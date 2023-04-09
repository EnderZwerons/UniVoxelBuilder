using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

//the nodelist namespace does not have more than the reader for now, but it may in the future.
namespace Nodelist
{
    //the purpose of the nodelist reader is to split a .nodelist file into nodes and store it into a list of .nodelist files for other scripts to use.
    //you can get a nodelist file (nodelistcontent) by name and then get nodes in that file by name to get the lines of text inside each node.

    public class NodelistReader
    {
        public static void InitializeNodelist(string content, string saveName)
        {
            //initialize new nodelistcontent and split the .nodelist file content into lines
            NodelistContent NLC = new NodelistContent(saveName);
            string[] lines = content.Split(Environment.NewLine.ToCharArray());
            
            //loop over all the lines of the .nodelist file content
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("node:"))
                {
                    //create new node from content after the node declaration.
                    Node newNode = new Node(lines[i].Split(':')[1]);

                    //increment the counter to skip to the node content
                    i++;

                    //run through the lines until the endnode is found
                    for (;;i++)
                    {
                        if (lines[i] == "endNode")
                        {
                            //add the node to the nodelistcontent and break out of the loop
                            NLC.nodes.Add(newNode);
                            break;
                        }
                        //if the line isn't empty, add it to the list of lines in the node.
                        else if (lines[i].Trim() != "")
                        {
                            newNode.lines.Add(lines[i]);
                        }
                    }
                }
            }

            //add the new nodelistcontent to the stored nodelist files.
            storedNodelistFiles.Add(NLC);
        }

        //parsed nodelist files.
        public static List<NodelistContent> storedNodelistFiles = new List<NodelistContent>();

        //find a parsed nodelist by name.
        public static NodelistContent GetNodelistFile(string fileName)
        {
            try
            {
                return storedNodelistFiles.Find(x => x.fileName == fileName);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }
    }

    //the nodelistcontent can be thought of as a parsed .nodelist file split into usable data.
    public class NodelistContent
    {
        //stored nodes inside the nodelist
        public List<Node> nodes = new List<Node>();

        //the filename of this nodelistcontent. used for finding the nodelistcontent.
        public string fileName;

        //simple constructor to declare the name
        public NodelistContent(string name)
        {
            this.fileName = name;
        }

        //find a node inside the nodelist by name
        public Node GetNode(string nodeName)
        {
            try
            {
                return nodes.Find(x => x.name == nodeName);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return null;
            }
        }
    }

    //the node class is used to store nodes by name in nodelistcontent objects. you can get the text stored inside by finding a node by name inside your desired nodelistcontent and using the lines variable
    public class Node
    {
        //name to find the node in the list of nodes
        public string name;

        //lines of text inside the node for use elsewhere
        public List<string> lines = new List<string>();

        //simple constructor to declare the name
        public Node(string nodeName)
        {
            this.name = nodeName;
        }
    }
}
