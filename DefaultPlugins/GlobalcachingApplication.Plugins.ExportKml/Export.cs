using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using System.Xml;
using System.IO;
using System.Web;
using System.Threading.Tasks;

namespace GlobalcachingApplication.Plugins.ExportKml
{
    public class Export : Utils.BasePlugin.BaseExportFilter
    {
        public const string STR_NOGEOCACHESELECTED = "No geocache selected for export";
        public const string STR_ERROR = "Error";
        public const string STR_EXPORTINGGPX = "Exporting KML...";
        public const string STR_CREATINGFILE = "Creating file...";
        public const string STR_BY = "by";
        public const string STR_FOUND = "Found";
        public const string STR_DIFFICULTY = "Difficulty";
        public const string STR_TERRAIN = "Terrain";
        public const string STR_CONTAINER = "Container";
        public const string STR_YOURS = "Your own";

        public const string ACTION_EXPORT_ALL = "Export KML|All";
        public const string ACTION_EXPORT_SELECTED = "Export KML|Selected";
        public const string ACTION_EXPORT_ACTIVE = "Export KML|Active";

        private string _filename = null;
        private List<Framework.Data.Geocache> _gcList = null;
        private int _indexDone = 0;

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_EXPORT_ALL);
            AddAction(ACTION_EXPORT_SELECTED);
            AddAction(ACTION_EXPORT_ACTIVE);

            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_NOGEOCACHESELECTED));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_ERROR));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_EXPORTINGGPX));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_CREATINGFILE));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_BY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_FOUND));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_DIFFICULTY));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_TERRAIN));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_CONTAINER));
            core.LanguageItems.Add(new Framework.Data.LanguageItem(STR_YOURS));

            return await base.InitializeAsync(core);
        }

        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result)
            {
                if (action == ACTION_EXPORT_ALL || action == ACTION_EXPORT_SELECTED || action == ACTION_EXPORT_ACTIVE)
                {
                    if (action == ACTION_EXPORT_ALL)
                    {
                        _gcList = (from Framework.Data.Geocache a in Core.Geocaches select a).ToList();
                    }
                    else if (action == ACTION_EXPORT_SELECTED)
                    {
                        _gcList = Utils.DataAccess.GetSelectedGeocaches(Core.Geocaches);
                    }
                    else
                    {
                        if (Core.ActiveGeocache != null)
                        {
                            _gcList = new List<Framework.Data.Geocache>();
                            _gcList.Add(Core.ActiveGeocache);
                        }
                    }
                    if (_gcList == null || _gcList.Count == 0)
                    {
                        System.Windows.Forms.MessageBox.Show(Utils.LanguageSupport.Instance.GetTranslation(STR_NOGEOCACHESELECTED), Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR), System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                    }
                    else
                    {
                        using (System.Windows.Forms.SaveFileDialog dlg = new System.Windows.Forms.SaveFileDialog())
                        {
                            dlg.FileName = "";
                            dlg.Filter = "*.kmz|*.kmz";
                            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                            {
                                _filename = dlg.FileName;
                                PerformExport();
                            }
                        }
                    }
                }
            }
            return result;
        }

        protected override void ExportMethod()
        {
            try
            {
                using (Utils.ProgressBlock progress = new Utils.ProgressBlock(this, STR_EXPORTINGGPX, STR_CREATINGFILE, _gcList.Count, 0, true))
                {
                    bool cancel = false;
                    _indexDone = 0;

                    if (File.Exists(_filename))
                    {
                        File.Delete(_filename);
                    }

                    //create temp folder / delete files
                    string tempFolder = System.IO.Path.Combine(Core.PluginDataPath, "KmlExport" );
                    string tempImgFolder = System.IO.Path.Combine(tempFolder, "Images");
                    if (!Directory.Exists(tempFolder))
                    {
                        Directory.CreateDirectory(tempFolder);
                    }
                    else
                    {
                        string[] fls = Directory.GetFiles(tempFolder);
                        if (fls != null && fls.Length > 0)
                        {
                            foreach (string s in fls)
                            {
                                File.Delete(s);
                            }
                        }
                    }
                    if (!Directory.Exists(tempImgFolder))
                    {
                        Directory.CreateDirectory(tempImgFolder);
                    }
                    else
                    {
                        string[] fls = Directory.GetFiles(tempImgFolder);
                        if (fls != null && fls.Length > 0)
                        {
                            foreach (string s in fls)
                            {
                                File.Delete(s);
                            }
                        }
                    }

                    XmlDocument doc = new XmlDocument();
                    XmlDeclaration pi = doc.CreateXmlDeclaration("1.0","utf-8","yes");
                    doc.InsertBefore(pi, doc.DocumentElement);
                    XmlElement root = doc.CreateElement("kml");
                    doc.AppendChild(root);
                    XmlAttribute attr = doc.CreateAttribute("creator");
                    XmlText txt = doc.CreateTextNode("GAPP, Globalcaching Application");
                    attr.AppendChild(txt);
                    root.Attributes.Append(attr);
                    attr = doc.CreateAttribute("xmlns");
                    txt = doc.CreateTextNode("http://www.opengis.net/kml/2.2");
                    attr.AppendChild(txt);
                    root.Attributes.Append(attr);

                    XmlElement rootDoc = doc.CreateElement("Document");
                    root.AppendChild(rootDoc);

                    XmlElement el = doc.CreateElement("name");
                    txt = doc.CreateTextNode(Path.GetFileNameWithoutExtension(_filename));
                    el.AppendChild(txt);
                    rootDoc.AppendChild(el);

                    List<Framework.Data.GeocacheType> typeList = new List<Framework.Data.GeocacheType>();
                    List<Framework.Data.Geocache> gs;
                    List<string> fileList = new List<string>();
                    foreach (Framework.Data.GeocacheType gt in Core.GeocacheTypes)
                    {
                        gs = (from g in _gcList where g.GeocacheType==gt && !g.Found && !g.IsOwn select g).ToList();
                        if (gs.Count>0)
                        {
                            if (!addGeocaches(progress, gs, typeList, doc, rootDoc, Utils.LanguageSupport.Instance.GetTranslation(gt.Name), tempImgFolder, fileList))
                            {
                                cancel = true;
                                break;
                            }
                        }
                    }
                    gs = (from g in _gcList where g.Found select g).ToList();
                    if (!cancel && gs.Count > 0)
                    {
                        if (!addGeocaches(progress, gs, typeList, doc, rootDoc, Utils.LanguageSupport.Instance.GetTranslation(STR_FOUND), tempImgFolder, fileList))
                        {
                            cancel = true;
                        }
                    }

                    gs = (from g in _gcList where g.IsOwn select g).ToList();
                    if (!cancel && gs.Count > 0)
                    {
                        if (!addGeocaches(progress, gs, typeList, doc, rootDoc, Utils.LanguageSupport.Instance.GetTranslation(STR_YOURS), tempImgFolder, fileList))
                        {
                            cancel = true;
                        }
                    }

                    if (!cancel)
                    {
                        string kmlFilename = System.IO.Path.Combine(tempFolder, "doc.kml");
                        using (TextWriter sw = new StreamWriter(kmlFilename, false, Encoding.UTF8)) //Set encoding
                        {
                            doc.Save(sw);
                        }
                        fileList.Add(kmlFilename);

                        using (ZipOutputStream s = new ZipOutputStream(System.IO.File.Create(_filename)))
                        {
                            s.SetLevel(9); // 0-9, 9 being the highest compression

                            byte[] buffer = new byte[4096];
                            fileList.Reverse();
                            foreach (string file in fileList)
                            {

                                ZipEntry entry = new ZipEntry(System.IO.Path.GetFileName(file) == "doc.kml" ? System.IO.Path.GetFileName(file) : string.Format("Images/{0}", System.IO.Path.GetFileName(file)));

                                entry.DateTime = DateTime.Now;
                                FileInfo fi = new FileInfo(file);
                                entry.Size = fi.Length;
                                s.PutNextEntry(entry);

                                using (System.IO.FileStream fs = System.IO.File.OpenRead(file))
                                {
                                    int sourceBytes;
                                    do
                                    {
                                        sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                        s.Write(buffer, 0, sourceBytes);
                                    } while (sourceBytes > 0);
                                }
                            }
                            s.Finish();
                            s.Close();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message, Utils.LanguageSupport.Instance.GetTranslation(STR_ERROR));
            }
        }

        private bool addGeocaches(Utils.ProgressBlock progress, List<Framework.Data.Geocache> gcList, List<Framework.Data.GeocacheType> typeList, XmlDocument doc, XmlElement rootDoc, string folderName, string tempFolder, List<string> fileList)
        {
            bool result = true;

            //
            //folder
            XmlElement folder = doc.CreateElement("Folder");
            rootDoc.AppendChild(folder);

            XmlElement el = doc.CreateElement("name");
            XmlText txt = doc.CreateTextNode(Utils.LanguageSupport.Instance.GetTranslation(folderName));
            el.AppendChild(txt);
            folder.AppendChild(el);

            foreach (Framework.Data.Geocache g in gcList)
            {
                //check if style exists
                Framework.Data.GeocacheType gt = g.GeocacheType;
                string imgIcon = Utils.ImageSupport.Instance.GetImagePath(Core, Framework.Data.ImageSize.Default, gt);
                string destImg = Path.Combine(tempFolder, Path.GetFileName(imgIcon));
                
                if (!typeList.Contains(gt))
                {
                    typeList.Add(gt);
                    string mapIcon = Utils.ImageSupport.Instance.GetImagePath(Core, Framework.Data.ImageSize.Map, gt);
                    string destFile = Path.Combine(tempFolder, Path.GetFileName(mapIcon));
                    string mapIconC = Utils.ImageSupport.Instance.GetImagePath(Core, Framework.Data.ImageSize.Map, gt, true);
                    string destFileC = Path.Combine(tempFolder, Path.GetFileName(mapIconC));
                    if (!File.Exists(destFile))
                    {
                        File.Copy(mapIcon, destFile);
                    }
                    if (!File.Exists(destFileC))
                    {
                        File.Copy(mapIconC, destFileC);
                    }
                    if (!File.Exists(destImg))
                    {
                        File.Copy(imgIcon, destImg);
                    }

                    if (!fileList.Contains(destFile))
                    {
                        fileList.Add(destFile);
                    }
                    if (!fileList.Contains(destFileC))
                    {
                        fileList.Add(destFileC);
                    }
                    if (!fileList.Contains(destImg))
                    {
                        fileList.Add(destImg);
                    }
                    //adding style
                    /*    
                     * <Style id="TNA">
                     *  <IconStyle id="iconTNA">
                     *      <scale>1</scale>
                     *      <Icon>
                     *          <href>http://hulmgulm.de/gc/myGoogleEarth/mrk_traditional.png</href>
                     *      </Icon>
                     *  </IconStyle>
                     *  <LabelStyle id="labelTNA">
                     *      <scale>1</scale>
                     *  </LabelStyle>
                     * </Style>
                    */
                    XmlElement style = doc.CreateElement("Style");
                    rootDoc.AppendChild(style);
                    XmlAttribute attr = doc.CreateAttribute("id");
                    txt = doc.CreateTextNode(string.Format("id{0}", gt.ID));
                    attr.AppendChild(txt);
                    style.Attributes.Append(attr);

                    XmlElement iconstyle = doc.CreateElement("IconStyle");
                    style.AppendChild(iconstyle);
                    attr = doc.CreateAttribute("id");
                    txt = doc.CreateTextNode(string.Format("iconid{0}", gt.ID));
                    attr.AppendChild(txt);
                    iconstyle.Attributes.Append(attr);

                    el = doc.CreateElement("scale");
                    txt = doc.CreateTextNode("1");
                    el.AppendChild(txt);
                    iconstyle.AppendChild(el);

                    XmlElement icon = doc.CreateElement("Icon");
                    iconstyle.AppendChild(icon);
                    el = doc.CreateElement("href");
                    txt = doc.CreateTextNode(string.Format("Images/{0}",Path.GetFileName(mapIcon)));
                    el.AppendChild(txt);
                    icon.AppendChild(el);


                    XmlElement labelstyle = doc.CreateElement("LabelStyle");
                    style.AppendChild(labelstyle);
                    attr = doc.CreateAttribute("id");
                    txt = doc.CreateTextNode(string.Format("labelid{0}", gt.ID));
                    attr.AppendChild(txt);
                    labelstyle.Attributes.Append(attr);

                    el = doc.CreateElement("scale");
                    txt = doc.CreateTextNode("1");
                    el.AppendChild(txt);
                    labelstyle.AppendChild(el);

                    //
                    //corrected
                    //
                    style = doc.CreateElement("Style");
                    rootDoc.AppendChild(style);
                    attr = doc.CreateAttribute("id");
                    txt = doc.CreateTextNode(string.Format("idC{0}", gt.ID));
                    attr.AppendChild(txt);
                    style.Attributes.Append(attr);

                    iconstyle = doc.CreateElement("IconStyle");
                    style.AppendChild(iconstyle);
                    attr = doc.CreateAttribute("id");
                    txt = doc.CreateTextNode(string.Format("iconidC{0}", gt.ID));
                    attr.AppendChild(txt);
                    iconstyle.Attributes.Append(attr);

                    el = doc.CreateElement("scale");
                    txt = doc.CreateTextNode("1");
                    el.AppendChild(txt);
                    iconstyle.AppendChild(el);

                    icon = doc.CreateElement("Icon");
                    iconstyle.AppendChild(icon);
                    el = doc.CreateElement("href");
                    txt = doc.CreateTextNode(string.Format("Images/{0}",Path.GetFileName(mapIconC)));
                    el.AppendChild(txt);
                    icon.AppendChild(el);


                    labelstyle = doc.CreateElement("LabelStyle");
                    style.AppendChild(labelstyle);
                    attr = doc.CreateAttribute("id");
                    txt = doc.CreateTextNode(string.Format("labelidC{0}", gt.ID));
                    attr.AppendChild(txt);
                    labelstyle.Attributes.Append(attr);

                    el = doc.CreateElement("scale");
                    txt = doc.CreateTextNode("1");
                    el.AppendChild(txt);
                    labelstyle.AppendChild(el);
                }

                XmlElement placemark = doc.CreateElement("Placemark");
                folder.AppendChild(placemark);

                el = doc.CreateElement("name");
                txt = doc.CreateTextNode(g.Name ?? "");
                el.AppendChild(txt);
                placemark.AppendChild(el);

                el = doc.CreateElement("styleUrl");
                if (g.ContainsCustomLatLon)
                {
                    txt = doc.CreateTextNode(string.Format("#idC{0}", gt.ID));
                }
                else
                {
                    txt = doc.CreateTextNode(string.Format("#id{0}", gt.ID));
                }
                el.AppendChild(txt);
                placemark.AppendChild(el);

                el = doc.CreateElement("description");
                string descr = string.Format("<table width='250'><tr><td align='center'><a href='{0}'>{1}</a><br><font size='4'><b>{2}</b></font><br /><i>{3} {4}</i><br /><br /><img src='Images/{5}' /> <br /><br />{6}: {7}<br />{8}: {9}<br />{10}: {11}<br /><br />({12})<br><br></td></tr></table>", g.Url, g.Code, g.Name ?? "", HttpUtility.HtmlEncode(Utils.LanguageSupport.Instance.GetTranslation(STR_BY)), g.Owner ?? "", Path.GetFileName(destImg), HttpUtility.HtmlEncode(Utils.LanguageSupport.Instance.GetTranslation(STR_DIFFICULTY)), g.Difficulty.ToString("0.#"), HttpUtility.HtmlEncode(Utils.LanguageSupport.Instance.GetTranslation(STR_TERRAIN)), g.Terrain.ToString("0.#"), HttpUtility.HtmlEncode(Utils.LanguageSupport.Instance.GetTranslation(STR_CONTAINER)), HttpUtility.HtmlEncode(Utils.LanguageSupport.Instance.GetTranslation(g.Container.Name)), Utils.Conversion.GetCoordinatesPresentation(g.Lat, g.Lon));
                txt = doc.CreateTextNode(descr);
                el.AppendChild(txt);
                placemark.AppendChild(el);

                el = doc.CreateElement("Snippet");
                txt = doc.CreateTextNode(string.Format("<a href='{0}'>{1}</a> {2}<br>{3} {4}", g.Url, g.Code, g.Name ?? "", Utils.LanguageSupport.Instance.GetTranslation(STR_BY), g.Owner ?? ""));
                el.AppendChild(txt);
                placemark.AppendChild(el);

                XmlElement point = doc.CreateElement("Point");
                placemark.AppendChild(point);
                el = doc.CreateElement("coordinates");
                if (g.ContainsCustomLatLon)
                {
                    txt = doc.CreateTextNode(string.Format("{0},{1}", g.CustomLon.ToString().Replace(',', '.'), g.CustomLat.ToString().Replace(',', '.')));
                }
                else
                {
                    txt = doc.CreateTextNode(string.Format("{0},{1}", g.Lon.ToString().Replace(',', '.'), g.Lat.ToString().Replace(',', '.')));
                }
                el.AppendChild(txt);
                point.AppendChild(el);

                _indexDone++;
                if (_indexDone % 100 == 0)
                {
                    if (!progress.UpdateProgress(STR_EXPORTINGGPX, STR_CREATINGFILE, _gcList.Count, _indexDone))
                    {
                        result = false;
                        break;
                    }
                }
                
            }
            return result;
        }
    }
}
