using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Verse;

namespace ProjectRimFactory.SAL3.PatchOperations
{
    public class PatchOperationMakeRecipeReaderRecipeDef : PatchOperation
    {
        public PatchOperationMakeRecipeReaderRecipeDef()
        {
        }
        protected override bool ApplyWorker(XmlDocument xml)
        {
            XmlNodeList nodes = xml.SelectNodes("*/RecipeDef[not(@Abstract = \"True\") and defName]");
            foreach (XmlNode node in nodes)
            {
                XmlNode newNode = node.Clone(); ;
                if (newNode.Attributes != null)
                {
                    XmlAttribute item = newNode.Attributes["Name"];
                    if (item != null)
                    {
                        newNode.Attributes.Remove(item);
                    }
                }
                for (int j = 0; j < newNode.ChildNodes.Count; j++)
                {
                    XmlNode childNode = newNode.ChildNodes[j];
                    switch (childNode.Name.ToLower())
                    {
                        case "defname":
                            childNode.InnerXml = "SmartAssemblerRecipe_" + childNode.InnerXml;
                            break;
                        case "label":
                            childNode.InnerXml = "Make schematic for " + childNode.InnerXml;
                            break;
                        case "skillrequirements":
                        case "researchprerequisite":
                            newNode.RemoveChild(childNode);
                            break;
                        default:
                            break;
                    }
                }
                node.ParentNode.PrependChild(newNode);
            }
            return true;
        }
    }
}
