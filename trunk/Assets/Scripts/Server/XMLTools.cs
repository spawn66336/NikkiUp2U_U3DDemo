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
        string filepath = Application.dataPath + "/../LevelConfig.xml";
        if (File.Exists(filepath))
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filepath);
            XmlNodeList nodeList = xmlDoc.SelectSingleNode("LevelConfig").ChildNodes;
//            GetLevelDate.GetInstance().levelConfigDateList = new List<LevelConfigDate>();
//            LevelConfigDate mLevelConfigDate;
//            LevelReadDate mLevelReadDate;
//            Entity mEntity=new Entity();
//            //遍历每一个节点，拿节点的属性以及节点的内容
//            foreach (XmlElement xe in nodeList)
//            {
//               // Debug.Log("Attribute :" + xe.GetAttribute("level_id"));
//              //  Debug.Log("level_id :" + xe.Name);  
//                mLevelConfigDate = new LevelConfigDate();
//                mLevelConfigDate.level_ID = int.Parse(xe.GetAttribute("level_id"));
//                mLevelConfigDate.levels = new List<LevelReadDate>();
//                foreach (XmlElement xchild in xe.ChildNodes)
//                {
//                    mLevelReadDate = new LevelReadDate();
//                    mLevelReadDate.sublevel_ID = int.Parse(xchild.GetAttribute("sublevel_id"));
//                    mLevelReadDate.subLevel_Time = float.Parse(xchild.GetAttribute("sublevel_time"));                    
//                    mLevelReadDate.entitys = new List<Entity>();
//                    foreach (XmlElement entitychild in xchild.ChildNodes)
//                    {
//                        mEntity.entity_ID = int.Parse(entitychild.GetAttribute("id"));
//                        mEntity.entity_X = int.Parse(entitychild.GetAttribute("x"));
//                        mEntity.entity_Y = int.Parse(entitychild.GetAttribute("y"));
//                        mEntity.entity_insTime = float.Parse(entitychild.GetAttribute("insTime"));
//                        mLevelReadDate.entitys.Add(mEntity);
////                        Debug.Log("  " + mEntity.entity_ID + "  mEntity.entity_X " + mEntity.entity_X);
//                    }                    
//                    mLevelConfigDate.levels.Add(mLevelReadDate);
//                  //  Debug.Log(" mLevelConfigDate.levels  " + mLevelConfigDate.levels.Count);
//                }
//                GetLevelDate.GetInstance().levelConfigDateList.Add(mLevelConfigDate);               
            }

           // Debug.Log("all = " + xmlDoc.OuterXml);
        }
    

    public static List<string> paths = new List<string>();
}

