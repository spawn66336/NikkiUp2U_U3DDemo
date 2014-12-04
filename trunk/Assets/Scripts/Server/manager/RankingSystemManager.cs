using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RankingSystemManager : IDataManager
{
    static RankingSystemManager _instance;
    public static RankingSystemManager getInstance()
    {
        if (_instance == null)
        {
            _instance = new RankingSystemManager();
        }
        return _instance;
    }
    public override void init()
    {
        XMLTools.readXMlGeneral("attribute.xml");
    }
    public override void destroy()
    {
        base.destroy();
    }

    public double getFinalScore(Level level, List<int> dressIdList)
    {
        if (level == null)
        {
            return -1;
        }
        if (dressIdList == null || dressIdList.Count <= 0)
        {
            return -1;
        }
        List<int> copyDressIdList = new List<int>();
        copyDressIdList.AddRange(dressIdList);
        double foundationScore = 0, attributeScore = 0, themeScore = 0;
        foreach (int id in dressIdList)
        {
            GameItemDataBaseBean dress = GameDataBaseManager.getInstance().getGameItemBean(id);
            // 1.基础分
            if (rarenessRatio.Count <= dress.rareness)
            {
                // TODO 数据错误
                continue;
            }
            if (bodyPartRatio.Count <= (int)dress.dressType)
            {
                // TODO 数据错误
                continue;
            }
            double baseScore = dress.score * (1 + rarenessRatio[dress.rareness])
                    * bodyPartRatio[(int)dress.dressType];
            double styleScore = 0;
            foreach (Style style in level.Thm.Stl)
            {
                if (dress.styleList.Count <= style.Id)
                {
                    // TODO 数据错误
                    continue;
                }
                
                styleScore += dress.styleList[style.Id]*style.Ratio;
            }
            foundationScore += baseScore * styleScore;
            // 2.特征匹配
            copyDressIdList.Remove(id);
            foreach (int copyId in copyDressIdList)
            {
                GameItemDataBaseBean copyDress = GameDataBaseManager.getInstance().getGameItemBean(copyId);
                if (dress.attStyleList!= null && dress.attStyleList.Count > 0
                        && copyDress.attStyleList != null && copyDress.attStyleList.Count > 0)
                {
                    foreach (Attribute attStyle in dress.attStyleList)
                    {
                        if (attributeStyle.attributeType_style.Count <= attStyle.id)
                        {
                            // TODO 数据错误
                            continue;
                        }
                        foreach (Attribute copyAttStyle in copyDress.attStyleList)
                        {
                            if (attributeStyle.attributeType_style.Count <= copyAttStyle.id)
                            {
                                // TODO 数据错误
                                continue;
                            }
                            
                            attributeScore += attributeStyle.attributeType_style[attStyle.id][copyAttStyle.id];
                        }
                    }
                }
                if (dress.attMatrialList != null && dress.attMatrialList.Count > 0
                        && copyDress.attMatrialList != null && copyDress.attMatrialList.Count > 0)
                {
                    foreach (Attribute attMatrial in dress.attMatrialList)
                    {
                        if (attributeMaterial.attributeType_material.Count <= attMatrial.id)
                        {
                            // TODO 数据错误
                            continue;
                        }
                        foreach (Attribute copyAttMatrial in copyDress.attMatrialList)
                        {
                            if (attributeMaterial.attributeType_material.Count <= copyAttMatrial.id)
                            {
                                // TODO 数据错误
                                continue;
                            }
                            
                            attributeScore += attributeMaterial.attributeType_material[attMatrial.id][copyAttMatrial.id];
                        }
                    }
                }
            }
            // 3.风格与主题匹配
            if (level.Thm.AttStyleList != null && level.Thm.AttStyleList.Count > 0
                    && dress.attStyleList != null && dress.attStyleList.Count > 0)
            {
                foreach (Atribute attStyleTheme in level.Thm.AttStyleList)
                {
                    foreach (Attribute attStyleDress in dress.attStyleList)
                    {
                        if (attStyleTheme.Id == attStyleDress.id)
                        {
                            themeScore += attStyleTheme.Ratio;
                            break;
                        }
                    }
                }
            }
            if (level.Thm.AttMatrialList != null && level.Thm.AttMatrialList.Count > 0
                    && dress.attMatrialList != null && dress.attMatrialList.Count > 0)
            {
                foreach (Atribute attMartialTheme in level.Thm.AttMatrialList)
                {
                    foreach (Attribute attMatrialDress in dress.attMatrialList)
                    {
                        if (attMartialTheme.Id == attMatrialDress.id)
                        {
                            themeScore += attMartialTheme.Ratio;
                            break;
                        }
                    }
                }
            }
        }

        return foundationScore * (1 + attributeScore) * (1 + themeScore);
    }
    
    public Attribute_Style attributeStyle = new Attribute_Style();
    public Attribute_Material attributeMaterial = new Attribute_Material();
    public List<double> bodyPartRatio = new List<double>();
    public List<double> rarenessRatio = new List<double>();
}
public enum AttribyteType
{
    Attribute_Style=0,
    Attribute_Material
}
public class Attribute_Style
{
    public virtual int id
    {
        get
        {
            return (int)AttribyteType.Attribute_Style;
        }
    }

    public Dictionary<int, List<double>> attributeType_style = new Dictionary<int, List<double>>();
}
public class Attribute_Material
{
    public virtual int id
    {
        get
        {
            return (int)AttribyteType.Attribute_Material;
        }
    }
    public Dictionary<int, List<double>> attributeType_material = new Dictionary<int, List<double>>();
}

public class StyleRatio
{
    public int id;
    public float ratio;
}