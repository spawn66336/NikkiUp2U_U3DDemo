using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Xml;
using ExcelClientBridge;

namespace testconvert
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private bool addSheet(ExcelBookRef tb,string sheetname)
        {
            int sheetcount = tb.GetSheetCout();
            tb.addSheet(sheetname);
            int sheetcount1 = tb.GetSheetCout();

            if ((sheetcount1 - sheetcount) == 1)
            {
                return true;
            }

            return false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (!File.Exists("ErrorLog.log"))
                {
                    FileStream fs = new FileStream("ErrorLog.log", FileMode.OpenOrCreate);
                    fs.Close();
                }

                int ret = 1;

                //打开日志文件
                StreamWriter log = new StreamWriter(Application.StartupPath + "\\ErrorLog.log", false, Encoding.Default);

                ExcelBookRef tb = new ExcelBookRef();
                string file = textBox1.Text;
                if (file.ToLower().Contains(".xlsx"))
                {
                    tb.xlCreateXMLBookInit();
                }
                else if (file.ToLower().Contains(".xls"))
                {
                    tb.xlCreateBookInit();
                }
                else
                {
                    log.WriteLine(textBox1.Text + " 文件类型错误！");
                    log.Flush();
                    log.Close();
                    tb.Realease();
                    return;
                }
              
                if (tb.load(file))
                {

                    ExcelSheet levelsheet = new ExcelSheet();
                    ExcelSheet issuesheet = new ExcelSheet();
                    ExcelSheet goodssheet = new ExcelSheet();
                    ExcelSheet clothsheet = new ExcelSheet();
                    try
                    {
                        for (int i = 0; i < tb.GetSheetCout(); i++)
                        {
                            SheetRef sheet = tb.GetSheet(i);

                            if (sheet.getName() == "关卡")
                            {
                                loadsheet(sheet, levelsheet,log);
                            }

                            if (sheet.getName() == "题目")
                            {
                                loadsheet(sheet, issuesheet, log);
                            }

                            if (sheet.getName() == "物品")
                            {
                                loadsheet(sheet, goodssheet, log);
                            }

                            if (sheet.getName() == "服装")
                            {
                                loadsheet(sheet, clothsheet, log);
                            }
                        }

                    }
                    catch(Exception ee)
                    {
                        MessageBox.Show("解析" + textBox1.Text + " 失败！\n" + ee.ToString());
                        return;
                    }

                    if (saveAreamapXML(textBox2.Text + "\\areamap.xml", levelsheet, issuesheet, log) == 0)
                    {
                        ret = 0;
                        log.WriteLine("areamap.xml保存失败！");
                    }
                    else if (saveGameItemXML(textBox2.Text + "\\gameItemData.xml", goodssheet, clothsheet, log) == 0)
                    {
                        ret = 0;
                        log.WriteLine("gameItemData.xml保存失败！");
                    }
                }
                    
                //tb.deleteSheet(0);
                //tb.save(file);

                
                log.Flush();
                log.Close();
                if (ret == 0)
                {
                    System.Diagnostics.Process.Start("ErrorLog.log");
                }
                else
                {
                    MessageBox.Show("完成");
                }
            }
            catch(Exception ee)
            {
                MessageBox.Show(ee.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = ".\\";
            openFileDialog1.Filter = "Excel files (*.xls;*.xlsx)|*.xls;*.xlsx";
            //openFileDialog1.RestoreDirectory = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
            }
           
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = dialog.SelectedPath;
            }
        }

        int saveGameItemXML(string file, ExcelSheet goodssheet, ExcelSheet clothsheet, StreamWriter log)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "GB2312", null);
            doc.AppendChild(dec);

            XmlElement root = doc.CreateElement("ArrayOfGameItemDataBaseBean");
            doc.AppendChild(root);
            for (int i = 0; i < goodssheet.getRowCount(); i++)
            {
                XmlElement ginode = doc.CreateElement("GameItemDataBaseBean");
                root.AppendChild(ginode);
                int gdid = int.Parse(goodssheet.getProp(i, 0));
                ginode.SetAttribute("ItemId", gdid.ToString());
                ginode.SetAttribute("ItemName", goodssheet.getProp(i, 1));

                int ItemType = int.Parse(goodssheet.getProp(i, 2));
                if (ItemType > 1 || ItemType <0)
                {
                    log.WriteLine(textBox1.Text + "中物品表格第 " + (i + 1).ToString() + " 行，第 3 列，数据超出有效范围！");
                    return 0;
                }
                ginode.SetAttribute("ItemType", ItemType.ToString());

                int Rareness = int.Parse(goodssheet.getProp(i, 4));
                if (Rareness > 4 || Rareness < 0)
                {
                    log.WriteLine(textBox1.Text + "中物品表格第 " + (i + 1).ToString() + " 行，第 5 列，数据超出有效范围！");
                    return 0;
                }
                ginode.SetAttribute("Rareness", Rareness.ToString());

                ginode.SetAttribute("Desc", goodssheet.getProp(i, 5));

                int DressType = int.Parse(goodssheet.getProp(i, 3));
                if (DressType > 15 || DressType < 0)
                {
                    log.WriteLine(textBox1.Text + "中物品表格第 " + (i + 1).ToString() + " 行，第 4 列，数据超出有效范围！");
                    return 0;
                }
                ginode.SetAttribute("DressType", DressType.ToString());


                ginode.SetAttribute("ShowPoition", "(" + goodssheet.getProp(i, 6) + "," + goodssheet.getProp(i, 7) + ")");

                int PriceGold = int.Parse(goodssheet.getProp(i, 3));
                if (PriceGold < 0)
                {
                    log.WriteLine(textBox1.Text + "中物品表格第 " + (i + 1).ToString() + " 行，第 9 列，数据超出有效范围！");
                    return 0;
                }
                ginode.SetAttribute("PriceGold", goodssheet.getProp(i, 8));

                int PriceDiamond = int.Parse(goodssheet.getProp(i, 3));
                if (PriceDiamond < 0)
                {
                    log.WriteLine(textBox1.Text + "中物品表格第 " + (i + 1).ToString() + " 行，第 10 列，数据超出有效范围！");
                    return 0;
                }
                ginode.SetAttribute("PriceDiamond", goodssheet.getProp(i, 9));

                XmlElement stlnode = doc.CreateElement("StyleList");
                ginode.AppendChild(stlnode);
                int colid = int.Parse(clothsheet.getProp(i, 0));
                if (gdid != colid)
                {
                    log.WriteLine(textBox1.Text + "中物品表格与衣服表格第 " + (i + 1).ToString() + " 行，物品id不对应！");
                    return 0;
                }
                for (int ist = 0; ist < 26; ist++)
                {
                    float Ratio = float.Parse(clothsheet.getProp(i, ist + 2));
                    if (Ratio > 1.0 || Ratio < -1.0)
                    {
                        log.WriteLine(textBox1.Text + "中服装表格第 " + (i + 1).ToString() + " 行，第 " + (ist + 1).ToString() + " 列，数据超出有效范围！");
                        return 0;
                    }

                    XmlElement rtnode = doc.CreateElement("ratio");
                    stlnode.AppendChild(rtnode);
                    rtnode.InnerText = Ratio.ToString();
                }
                XmlElement attrlnode = doc.CreateElement("AttributeList");
                ginode.AppendChild(attrlnode);

                XmlElement attrnode0 = doc.CreateElement("Attribute");
                attrlnode.AppendChild(attrnode0);

                attrnode0.SetAttribute("Type", "0");

                int TypeId = int.Parse(clothsheet.getProp(i, 1));
                if (TypeId < 0 || TypeId > 63)
                {
                    log.WriteLine(textBox1.Text + "中衣服表格第 " + (i + 1).ToString() + " 行，第 2 列，数据超出有效范围！");
                    return 0;
                }
                attrnode0.SetAttribute("Id", clothsheet.getProp(i, 1));
            }


            doc.Save(file);

            return 1;
        }

        int saveAreamapXML(string file, ExcelSheet levelsheet, ExcelSheet issuesheet, StreamWriter log)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "GB2312", null);
            doc.AppendChild(dec);

            XmlElement root = doc.CreateElement("AreaMaps");
            doc.AppendChild(root);

            int mapid = 0;

            XmlElement mapnode = null;

            for (int i = 0; i < levelsheet.getRowCount(); i++)
            {
                if (mapid != int.Parse(levelsheet.getProp(i, 0)))
                {
                    if(mapnode != null)
                    {
                        root.AppendChild(mapnode);
                    }
                    mapnode = doc.CreateElement("Map");
                    mapid = int.Parse(levelsheet.getProp(i, 0));
                    mapnode.SetAttribute("Id", levelsheet.getProp(i, 0));
                    mapnode.SetAttribute("Name", levelsheet.getProp(i, 1));
                    mapnode.SetAttribute("IconId", "iconid");
                    XmlElement munode = doc.CreateElement("MapUnlockCondition");
                    mapnode.AppendChild(munode);
                    munode.SetAttribute("Type", "1");
                    munode.SetAttribute("Value", "1");
                    XmlElement imlistnode = doc.CreateElement("ImageList");
                    mapnode.AppendChild(imlistnode);

                    XmlElement imnode = doc.CreateElement("Image");
                    imlistnode.AppendChild(imnode);
                    imnode.InnerText = "image_name";
                }

                XmlElement levelnode = doc.CreateElement("Level");
                mapnode.AppendChild(levelnode);
                int levelid = int.Parse(levelsheet.getProp(i, 2));
                levelnode.SetAttribute("Id", levelid.ToString());
                levelnode.SetAttribute("Name", levelsheet.getProp(i, 3));

                int FinishGrade = int.Parse(levelsheet.getProp(i, 4));
                if (FinishGrade < 0 || FinishGrade > 7)
                {
                    log.WriteLine(textBox1.Text + "中关卡表格第 " + (i + 1).ToString() + " 行，第 5 列，数据超出有效范围！");
                    return 0;
                }
                levelnode.SetAttribute("FinishGrade", levelsheet.getProp(i, 4));

                int LimiteTime = int.Parse(levelsheet.getProp(i, 4));
                if (LimiteTime < 0 || LimiteTime > 7)
                {
                    log.WriteLine(textBox1.Text + "中关卡表格第 " + (i + 1).ToString() + " 行，第 6 列，数据超出有效范围！");
                    return 0;
                }
                levelnode.SetAttribute("LimiteTime", levelsheet.getProp(i, 5));
                levelnode.SetAttribute("RewardId", levelsheet.getProp(i, 2));
                levelnode.SetAttribute("DialogId", levelsheet.getProp(i, 2));

                XmlElement ratingRulenode = doc.CreateElement("RatingRule");
                levelnode.AppendChild(ratingRulenode);
                for (int ir = 0; ir < 8; ir++)
                {
                    XmlElement gradeInfonode = doc.CreateElement("GradeInfo");
                    ratingRulenode.AppendChild(gradeInfonode);
                    gradeInfonode.SetAttribute("Grade", ir.ToString());
                    gradeInfonode.SetAttribute("MaxScore", levelsheet.getProp(i, 8+ir*2));
                    gradeInfonode.SetAttribute("UsePower", levelsheet.getProp(i, 9+ir*2));
                }
                XmlElement ulcnode = doc.CreateElement("UnLockCondition");
                levelnode.AppendChild(ulcnode);
                ulcnode.SetAttribute("Type", levelsheet.getProp(i, 6));
                ulcnode.SetAttribute("Value", levelsheet.getProp(i, 7));

                XmlElement rfnode = doc.CreateElement("RatingFactor");
                levelnode.AppendChild(rfnode);
                int issueid = int.Parse(issuesheet.getProp(i, 0));
                if(issueid != levelid)
                {
                    log.WriteLine(textBox1.Text + "中关卡表格与题目表格第 " + (i + 1).ToString() + " 行，关卡id不对应！");
                    return 0;
                }
                for (int ist = 0; ist < 26; ist++ )
                {
                    float Ratio = float.Parse(issuesheet.getProp(i, ist + 1));
                    if (Ratio == 0.0)
                    {
                        continue;
                    }

                    if (Ratio > 1.0 || Ratio < -1.0)
                    {
                        log.WriteLine(textBox1.Text + "中题目表格第 " + (i + 1).ToString() + " 行，第 " + (ist + 1).ToString() + " 列，数据超出有效范围！");
                        return 0;
                    }
                    XmlElement stnode = doc.CreateElement("Style");
                    rfnode.AppendChild(stnode);
                    stnode.SetAttribute("Id", ist.ToString());
                    stnode.SetAttribute("Ratio", Ratio.ToString());
                }
            }

            if (mapnode != null)
            {
                root.AppendChild(mapnode);
            }

            doc.Save(file);
            return 1;
        }


        int loadsheet(SheetRef sheet, ExcelSheet es, StreamWriter log)
        {
            try
            {
                int firstRow = sheet.FirtRow();
                int lastRow = sheet.LastRow();
                int firstCol = sheet.FirtCol();
                int lastCol = sheet.LastCol();
                //if (lastRow <= 0 || lastCol <= 0)//若分页为空
                //{
                //    return;
                //}

                for (int i = 0; i < lastCol; i++)
                {
                    string propname = sheet.getDate(firstRow, i);
                    es.addPropName(propname);
                }

                for (int ir = 1; ir < lastRow; ir++ )
                {
                    List<string> props = new List<string>();
                    for (int ic = 0; ic < lastCol; ic++)
                    {
                        string prop = sheet.getDate(ir, ic);
                        props.Add(prop);
                    }

                    es.addProps(props);
                }

            }
            catch (Exception ee)
            {
                log.WriteLine(sheet.getName() + "解析失败！\n" + ee.ToString());
                return 0;
            }

            return 1;
        }
    }

    class ExcelSheet 
    {
        List<string> propname;
        List<List<string>> propdata;

        public ExcelSheet()
        {
            this.propname = new List<string>();
            this.propdata = new List<List<string>>();
        }

        public bool addPropName(string pName)
        {
            this.propname.Add(pName);
            return true;
        }

        int getcol(string name)
        {
            int col = this.propname.FindIndex(delegate(string s) { return s == name; });
            return col;
        }

        public bool addProps(List<string> props)
        {
            this.propdata.Add(props);
            return true;
        }

        public int getRowCount()
        {
            return this.propdata.Count();
        }

        public string getProp(int row, string name)
        {
            if (row >= this.propdata.Count())
            {
                return "";
            }
            int col = getcol(name);

            if (col < 0)
            {
                return "";
            }
            string ret = this.propdata[row][col];

            return ret;
        }

        public string getProp(int row, int col)
        {
            if (row >= this.getRowCount())
            {
                return "";
            }

            if (col >= this.propname.Count())
            {
                return "";
            }
            string ret = this.propdata[row][col];

            return ret;
        }
    }
}
