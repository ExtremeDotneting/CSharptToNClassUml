using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CSharptToNClassUml
{
    //Не судите меня по-этому коду. Он написан на скорую руку, я осознанно игнорировал правила написания кода.
    //Do not judge me in this code.It is written in a hurry, I deliberately ignored the rules of writing code.
    static class Parser
    {
        public static XElement CodeToXElement(string inputCodePage)
        {
            return CodeToXElement(new string[] { inputCodePage });
        }
        public static XElement CodeToXElement(IEnumerable<string> inputCodePages)
        {
            List<string> nodesList = new List<string>();
            foreach (string item in inputCodePages)
            {
                string inputCode = item.Trim();
                inputCode = RemoveAllBetween("[Dll", "]", inputCode, false);
                inputCode = RemoveAllBetween("//", "\n", inputCode, false);
                inputCode = RemoveAllBetween("/*", "*/", inputCode, false);
                if (inputCode.IndexOf("namespace ") > 0)
                {
                    int startBracket = inputCode.IndexOf('{');
                    inputCode = inputCode.Remove(inputCode.Length - 1);
                    inputCode = inputCode.Substring(startBracket + 1);

                }

                //inputCode = inputCode.Replace("struct", "$%$struct").Replace("enum", "$%$enum").Replace("class", "$%$class").Replace("interface", "$%$interface");
                string[] nodesArr = SplitNodes(inputCode).Split(new string[] { "$%$" }, StringSplitOptions.RemoveEmptyEntries);
                //System.Windows.Forms.MessageBox.Show(SplitNodes(inputCode));
                nodesList.AddRange(nodesArr);
            }

            XElement entities = new XElement("Entities");
            foreach (XElement item in NodesToXElement(nodesList))
                entities.Add(item);

            XElement res = XElement.Parse("<Project><Name>GeneratedDiagram</Name><ProjectItem type = \"NClass.DiagramEditor.ClassDiagram.Diagram\" assembly = \"NClass.DiagramEditor, Version=2.4.1823.0, Culture=neutral, PublicKeyToken=null\"><Name>first</Name><Language>CSharp</Language></ProjectItem ></Project>");
            res.Element("ProjectItem").Add(entities);
            return res;


        }

        static string RemoveAllBetween(string s1, string s2, string str, bool enableDimensions=true)
        {
            str = str + "";
            while (true)
            {
                //System.Windows.Forms.MessageBox.Show(members);
                int startIndex = str.IndexOf(s1);
                if (startIndex < 0)
                    break;
                int endIndex;
                int removeEnd = 0;
                int i = startIndex;
                while (true)
                {
                    i++;

                    if (str.Substring(i, s1.Length) == s1)
                        removeEnd++;
                    if (str.Substring(i, s2.Length)==s2)
                    {
                        if (removeEnd == 0 || !enableDimensions)
                        {
                            endIndex = i;
                            break;
                        }
                        else
                            removeEnd--;
                    }
                }
                str = str.Remove(startIndex, endIndex - startIndex+1);
            }
            return str;
        }
        static string SplitNodes(string inputCode)
        {
            int lastIndex = 0;
            while (true)
            {
                int startIndex = inputCode.IndexOf('{', lastIndex);
                if (startIndex < 0)
                    return inputCode;
                int endIndex = 100000;
                int removeEnd = 0;
                int i = startIndex;
                while (true)
                {
                    
                    i++;
                    if (inputCode[i] == '{')
                        removeEnd++;
                    if (inputCode[i] == '}')
                    {
                        if (removeEnd == 0)
                        {
                            endIndex = i;
                            lastIndex = endIndex;
                            break;
                        }
                        else
                            removeEnd--;
                    }  
                }

                inputCode = inputCode.Insert(lastIndex+1, "$%$");
            }
        }
        static List<XElement> NodesToXElement(IEnumerable<string> inputCode)
        {
            List<XElement> res = new List<XElement>();
            int left = 1, top = 1;
            foreach (string item in inputCode)
            {
                try
                {
                    res.Add(NodeToXElement(item, left, top));
                    left += 300;
                }
                catch { }
            }
            return res;
        }
        static XElement NodeToXElement(string node, int left, int top)
        {
            node = node.Trim();
            int startBracket = node.IndexOf('{');
            string cutedOnStart = node.Substring(0, startBracket);
            if (cutedOnStart.IndexOf(":") > 0)
            {
                cutedOnStart=cutedOnStart.Remove(cutedOnStart.IndexOf(":"));
            }
            string members = node.Substring(startBracket+1);
            members = members.Remove(members.Length - 1);
            string nodeName,nodeType,nodeAccess= "Public";
            if (cutedOnStart.IndexOf(@"struct") >= 0)
            {
                nodeName = cutedOnStart.Substring(cutedOnStart.IndexOf(@"struct") +6);
                nodeType = "Structure";
            }
            else if (cutedOnStart.IndexOf(@"class") >= 0)
            {
                nodeName = cutedOnStart.Substring(cutedOnStart.IndexOf(@"class") + 5);
                nodeType = "Class";
            }
            else if (cutedOnStart.IndexOf(@"interface") >= 0)
            {
                nodeName = cutedOnStart.Substring(cutedOnStart.IndexOf(@"interface") + 9);
                nodeType = "Interface";
            }
            else if (cutedOnStart.IndexOf(@"enum") >= 0)
            {
                nodeName = cutedOnStart.Substring(cutedOnStart.IndexOf(@"enum") + 4);
                nodeType = "Enum";
            }
            else
                return null;
            nodeName = nodeName.Trim();


            int width=90, height=90;

            XElement xel = new XElement("Entity");

            xel.SetAttributeValue("type", nodeType);
            xel.SetElementValue("Name", nodeName);
            xel.SetElementValue("Access", nodeAccess);
            xel.SetElementValue("Collapsed", "False");
            XElement location = new XElement("Location");
            location.SetAttributeValue("left", left);
            location.SetAttributeValue("top", top);
            XElement size = new XElement("Size");
            size.SetAttributeValue("width", width);
            size.SetAttributeValue("height", height);
            xel.Add(location);
            xel.Add(size);


            if (nodeType == "Enum")
            {
                string[] membersArr = members.Split(',');
                foreach (string item in membersArr)
                {
                    XElement member = new XElement("Value");
                    string val = item.Trim();
                    if(!string.IsNullOrWhiteSpace(val))
                        member.Value = val;
                    xel.Add(member);
                }
            }
            else
            {
                while (true)
                {
                    //System.Windows.Forms.MessageBox.Show(members);
                    int startIndex = members.IndexOf('{');
                    if (startIndex < 0)
                        break;
                    int endIndex;
                    int removeEnd = 0;
                    int i = startIndex;
                    while (true)
                    {
                        i++;
                        if (members[i] == '{')
                            removeEnd++;
                        if (members[i] == '}')
                        {
                            if (removeEnd == 0)
                            {
                                endIndex = i;
                                break;
                            }
                            else
                                removeEnd--;
                        }
                    }
                    members = members.Remove(startIndex, endIndex - startIndex);

                }
                members = RemoveAllBetween("{", "}", members);
                members = members.Replace("\n", "").Replace("\t", "").Replace("\r", "").Replace("  ", "").Replace(")}", ");").Replace("}", "{};");
                //members = members.Replace("}", "{ get; set; }").Replace("\n", "").Replace("\t", "").Replace("\r", "").Replace("  ", "").Replace("){}", ")");
                //while (true)
                //{
                //    int startIndex = members.IndexOf('=');
                //    if (startIndex < 0)
                //        break;
                //    int endIndex = members.IndexOf(';', startIndex);
                //    members = members.Remove(startIndex, endIndex - startIndex);
                //}
                //members=members.Replace(";", ";\n");
                //Console.WriteLine(members + "\n\n\n\n");
                //System.Windows.Forms.MessageBox.Show(members);
                string[] membersArr = members.Split(';');
                foreach (string item in membersArr)
                {
                    MemberAddToXElement(item, nodeName, xel);
                }
            }
            return xel;
        }

        static void MemberAddToXElement(string memberStr, string nodeName, XElement xel)
        {
            XElement member = new XElement("Member");
            string codeNodeEl = memberStr.Trim();
            if (codeNodeEl.IndexOf(" extern ") >= 0)
                return;
            if (string.IsNullOrWhiteSpace(codeNodeEl))
                return;
            string memberType = "";
            string cuted = codeNodeEl.Replace("\n", "").Replace("\t", "").Replace("\r", "").Replace(" ", "");
            string fuckIt = RemoveAllBetween("(", ")", cuted.Replace("{}", "")).Trim();
            codeNodeEl = codeNodeEl.Replace(" static", " static ");
            if (fuckIt.IndexOf(nodeName) == fuckIt.Length- nodeName.Length && fuckIt.IndexOf(nodeName)>0)
            {
                memberType = "Constructor";
                codeNodeEl = codeNodeEl.Replace("{}", "");
            }
            else if (cuted.IndexOf(")") > 0 && codeNodeEl.IndexOf("=") < 0)
            {
                memberType = "Method";
                codeNodeEl = codeNodeEl.Replace("{}", "");
            }
            else if (cuted.IndexOf("{}") > 0)
            {
                codeNodeEl = codeNodeEl.Replace("{}", "{ get; set; }");
                memberType = "Property";
            }
            else
            {
                codeNodeEl = RemoveAllBetween("(", ")", codeNodeEl);
                memberType = "Field";
                string[] fieldArr = codeNodeEl.Split(',');
                if (fieldArr[0].IndexOf("=") >= 0)
                    codeNodeEl=fieldArr[0] = fieldArr[0].Remove(fieldArr[0].IndexOf("=")).TrimEnd();
                if (fieldArr.Length > 1)
                {
                    
                    string prefix = fieldArr[0].Remove(fieldArr[0].LastIndexOf(" ")).TrimEnd();
                    for (int i = 0; i < fieldArr.Length; i++)
                    {
                        if (fieldArr[i].IndexOf("=") >= 0)
                            fieldArr[i]=fieldArr[i].Remove(fieldArr[i].IndexOf("="));
                        if (i > 0)
                            fieldArr[i] = prefix + " " + fieldArr[i];
                        MemberAddToXElement(fieldArr[i], nodeName, xel);
                    }

                    return;
                }

            }
            member.SetAttributeValue("type", memberType);
            member.Value = codeNodeEl;
            xel.Add(member);
            return;
        }
    }
}
