using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace GAPPSF.KML
{
    public class Export
    {
        private int _indexDone;
        private DateTime _nextUpdate;
        private List<Core.Data.Geocache> _gcList;
        private string _filename;

        public async Task PerformExportAsync(List<Core.Data.Geocache> gcList)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = ""; // Default file name
            dlg.DefaultExt = ".kmz"; // Default file extension
            dlg.Filter = "KMZ files (.kmz)|*.kmz"; // Filter files by extension 

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                _filename = dlg.FileName;
                await Task.Run(() => { PerformExport(dlg.FileName, gcList); });
            }

        }

        public void PerformExport(string targetFile, List<Core.Data.Geocache> gcList)
        {
            _gcList = gcList;
            try
            {
                _nextUpdate = DateTime.Now.AddSeconds(1);
                using (Utils.ProgressBlock progress = new Utils.ProgressBlock("ExportingKML", "CreatingFile", _gcList.Count, 0, true))
                {
                    bool cancel = false;
                    _indexDone = 0;

                    if (File.Exists(_filename))
                    {
                        File.Delete(_filename);
                    }

                    //create temp folder / delete files
                    string tempFolder = System.IO.Path.Combine(Core.Settings.Default.SettingsFolder, "KmlExport");
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
                    XmlDeclaration pi = doc.CreateXmlDeclaration("1.0", "utf-8", "yes");
                    doc.InsertBefore(pi, doc.DocumentElement);
                    XmlElement root = doc.CreateElement("kml");
                    doc.AppendChild(root);
                    XmlAttribute attr = doc.CreateAttribute("creator");
                    XmlText txt = doc.CreateTextNode("GAPPSF, Globalcaching Application");
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

                    List<Core.Data.GeocacheType> typeList = new List<Core.Data.GeocacheType>();
                    List<Core.Data.Geocache> gs;
                    List<string> fileList = new List<string>();
                    foreach (Core.Data.GeocacheType gt in Core.ApplicationData.Instance.GeocacheTypes)
                    {
                        gs = (from g in _gcList where g.GeocacheType == gt && !g.Found && !g.IsOwn select g).ToList();
                        if (gs.Count > 0)
                        {
                            if (!addGeocaches(progress, gs, typeList, doc, rootDoc, Localization.TranslationManager.Instance.Translate(gt.Name) as string, tempImgFolder, fileList))
                            {
                                cancel = true;
                                break;
                            }
                        }
                    }
                    gs = (from g in _gcList where g.Found select g).ToList();
                    if (!cancel && gs.Count > 0)
                    {
                        if (!addGeocaches(progress, gs, typeList, doc, rootDoc, Localization.TranslationManager.Instance.Translate("Found") as string, tempImgFolder, fileList))
                        {
                            cancel = true;
                        }
                    }

                    gs = (from g in _gcList where g.IsOwn select g).ToList();
                    if (!cancel && gs.Count > 0)
                    {
                        if (!addGeocaches(progress, gs, typeList, doc, rootDoc, Localization.TranslationManager.Instance.Translate("Yours") as string, tempImgFolder, fileList))
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
            catch
            {
            }
        }

        private bool addGeocaches(Utils.ProgressBlock progress, List<Core.Data.Geocache> gcList, List<Core.Data.GeocacheType> typeList, XmlDocument doc, XmlElement rootDoc, string folderName, string tempFolder, List<string> fileList)
        {
            bool result = true;

            //
            //folder
            XmlElement folder = doc.CreateElement("Folder");
            rootDoc.AppendChild(folder);

            XmlElement el = doc.CreateElement("name");
            XmlText txt = doc.CreateTextNode(Localization.TranslationManager.Instance.Translate(folderName) as string);
            el.AppendChild(txt);
            folder.AppendChild(el);

            foreach (Core.Data.Geocache g in gcList)
            {
                //check if style exists
                Core.Data.GeocacheType gt = g.GeocacheType;
                string imgIcon = string.Format("{0}.gif",gt.ID);
                string destImg = Path.Combine(tempFolder, imgIcon);

                if (!typeList.Contains(gt))
                {
                    typeList.Add(gt);
                    string mapIcon = string.Format("{0}.png", gt.ID);
                    string destFile = Path.Combine(tempFolder, mapIcon);
                    string mapIconC = string.Format("c{0}.png", gt.ID);
                    string destFileC = Path.Combine(tempFolder, mapIconC);
                    Utils.ResourceHelper.SaveToFile(string.Format("/Resources/CacheTypes/Map/{0}", mapIcon), destFile, false);
                    Utils.ResourceHelper.SaveToFile(string.Format("/Resources/CacheTypes/Map/{0}", mapIconC), destFileC, false);
                    Utils.ResourceHelper.SaveToFile(string.Format("/Resources/CacheTypes/{0}", imgIcon), destImg, false);

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
                    txt = doc.CreateTextNode(string.Format("Images/{0}", Path.GetFileName(mapIcon)));
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
                    txt = doc.CreateTextNode(string.Format("Images/{0}", Path.GetFileName(mapIconC)));
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
                string descr = string.Format("<table width='250'><tr><td align='center'><a href='{0}'>{1}</a><br><font size='4'><b>{2}</b></font><br /><i>{3} {4}</i><br /><br /><img src='Images/{5}' /> <br /><br />{6}: {7}<br />{8}: {9}<br />{10}: {11}<br /><br />({12})<br><br></td></tr></table>", g.Url, g.Code, g.Name ?? "", HttpUtility.HtmlEncode(Localization.TranslationManager.Instance.Translate("By")), g.Owner ?? "", Path.GetFileName(destImg), HttpUtility.HtmlEncode(Localization.TranslationManager.Instance.Translate("Difficulty")), g.Difficulty.ToString("0.#"), HttpUtility.HtmlEncode(Localization.TranslationManager.Instance.Translate("Terrain")), g.Terrain.ToString("0.#"), HttpUtility.HtmlEncode(Localization.TranslationManager.Instance.Translate("Container")), HttpUtility.HtmlEncode(Localization.TranslationManager.Instance.Translate(g.Container.Name)), Utils.Conversion.GetCoordinatesPresentation(g.Lat, g.Lon));
                txt = doc.CreateTextNode(descr);
                el.AppendChild(txt);
                placemark.AppendChild(el);

                el = doc.CreateElement("Snippet");
                txt = doc.CreateTextNode(string.Format("<a href='{0}'>{1}</a> {2}<br>{3} {4}", g.Url, g.Code, g.Name ?? "", Localization.TranslationManager.Instance.Translate("By"), g.Owner ?? ""));
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
                if (DateTime.Now >= _nextUpdate)
                {
                    if (!progress.Update("CreatingFile", _gcList.Count, _indexDone))
                    {
                        result = false;
                        break;
                    }
                    _nextUpdate = DateTime.Now.AddSeconds(1);
                }

            }
            return result;
        }

    }
}
