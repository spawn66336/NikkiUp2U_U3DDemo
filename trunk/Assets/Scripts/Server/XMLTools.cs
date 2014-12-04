using System;
using UnityEngine;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;


public class XMLTools
{
    public static System.Object readXml(String fileName,Type type)
    {
        StreamReader sr = null;
        try
        { 
            XmlSerializer ser = new XmlSerializer(type);
            string filePath = GameUtil.GetServerConfigFilePath(fileName);
            Debug.Log(filePath);
            paths.Add(filePath);

            sr = new StreamReader(filePath); 
            return ser.Deserialize(sr);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            Console.WriteLine(e);
        }
        finally
        {
            if (sr != null)
            {
                sr.Close();
            }
        }
        return null;
    }

    public static void writeXml(String fileName,System.Object obj)
    {
        StreamWriter sw = null;
        try
        {
            XmlSerializer ser = new XmlSerializer(obj.GetType());
            string filePath = GameUtil.GetServerConfigFilePath(fileName); 
            Debug.Log(filePath);
            sw = new StreamWriter(File.OpenWrite(filePath));
            ser.Serialize(sw, obj);
        }
        catch (Exception e)
        {
            Debug.Log(e);
            Console.WriteLine(e);
        }
        finally
        {
            if (sw != null)
            {
                sw.Flush();
                sw.Close();
            }
        }
    }
    public static void readXMlGeneral(String fileName)
    {
        string filepath = GameUtil.GetServerConfigFilePath(fileName); 
        if (File.Exists(filepath))
        {
            StreamReader sr2 = new StreamReader(filepath, Encoding.UTF8);
            StreamReader sr = File.OpenText(filepath);
            string nextLine;
            int flag=-1;
            string attribyte = "";
            string bodypart="";
            string rareness = "";
            while ((nextLine = sr.ReadLine()) != null)
            {
                if (nextLine.Contains("<AttributeList>"))
                {
                    flag = 1;

                }
                else if (nextLine.Contains("<BodyPartList>"))
                {
                    flag = 2;
                }
                else if (nextLine.Contains("<RarenessList>"))
                {
                    flag = 3;
                }
                else if (nextLine.Contains("<StyleList>"))
                {
                    break;
                }
                if (flag == 1)
                {
                    attribyte += nextLine + "\n";
                }else if(flag==2){
                    bodypart += nextLine + "\n";
                }
                else if (flag == 3)
                {
                    rareness += nextLine + "\n";
                }
                
            }
            sr.Close();
            // 读取attribute
            XmlDocument xmlDocAtt = new XmlDocument();
            xmlDocAtt.LoadXml(attribyte);
            XmlNodeList nodeListAtt = xmlDocAtt.SelectSingleNode("AttributeList").ChildNodes;
            foreach (XmlElement xe in nodeListAtt)
            {
                int id = int.Parse(xe.GetAttribute("id").ToString());
                XmlNodeList nodeList1 = xe.ChildNodes;
                foreach (XmlElement xe1 in nodeList1)
                {
                    int id1 = int.Parse(xe1.GetAttribute("id").ToString());
                    string str = xe1.InnerText;
                    string[] strs = str.Split(' ');
                    List<double> list = new List<double>();
                    foreach(string strr in strs){
                        list.Add(double.Parse(strr));
                    }
                    if (id == 0)
                    {
                        Attribute_Style style = RankingSystemManager.getInstance().attributeStyle;
                        Dictionary<int, List<double>> dic = style.attributeType_style;
                        if (dic.ContainsKey(id1))
                        {
                            continue;
                        }
                        dic.Add(id1, list);
                    }
                    else if (id == 1)
                    {
                        Attribute_Material material = RankingSystemManager.getInstance().attributeMaterial;
                        Dictionary<int, List<double>> dic = material.attributeType_material;
                        if (dic.ContainsKey(id1))
                        {
                            continue;
                        }
                        dic.Add(id1, list);
                    }
                }
               }
            // 读取bodypart
            XmlDocument xmlDocBody = new XmlDocument();
            xmlDocBody.LoadXml(bodypart);
            XmlNodeList nodeListBody = xmlDocBody.SelectSingleNode("BodyPartList").ChildNodes;
            foreach (XmlElement xe in nodeListBody)
            {
                int index = int.Parse(xe.GetAttribute("appear_index").ToString());
                double ratio = double.Parse(xe.GetAttribute("ratio").ToString());
                RankingSystemManager.getInstance().bodyPartRatio.Add(ratio);
                
            }

            // 读取rareness
            XmlDocument xmlDocRareness = new XmlDocument();
            xmlDocRareness.LoadXml(rareness);
            XmlNodeList nodeListRareness = xmlDocRareness.SelectSingleNode("RarenessList").ChildNodes;
            foreach (XmlElement xe in nodeListRareness)
            {
                int level = int.Parse(xe.GetAttribute("level").ToString());
                double ratio = double.Parse(xe.GetAttribute("ratio").ToString());
                RankingSystemManager.getInstance().rarenessRatio.Add(ratio);
            }
        }
    }
    

    public static List<string> paths = new List<string>();
}

