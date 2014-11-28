using System;
using UnityEngine;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


public class XMLTools
{
    public static System.Object readXml(String fileName,Type type)
    {
        TextReader sr=null;
        try
        {
            String finalFileName = fileName.Remove(fileName.Length - 4, 4);
            XmlSerializer ser = new XmlSerializer(type);
            String filePath = Application.dataPath + "/Resources/Config/Server/" + fileName;
            Debug.Log(filePath);
            paths.Add(filePath);

            TextAsset asset = Resources.Load("Config/Server/" + finalFileName) as TextAsset;
            sr = new StringReader(asset.text); 
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
            String filePath = Application.dataPath + "/Resources/Config/Server/" + fileName;
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

    public static List<string> paths = new List<string>();
}

