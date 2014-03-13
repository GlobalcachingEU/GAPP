using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.ImageGrabber
{
    public class Export
    {
        public async Task DeleteImagesFromFolder(List<Core.Data.Geocache> gcList, string folder)
        {
            await Task.Run(() =>
            {
                try
                {
                    DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                    using (Utils.ProgressBlock progress = new Utils.ProgressBlock("DeletingImages", "DeletingImages", gcList.Count, 0, true))
                    {
                        string imgFolder;
                        string checkFolder = Path.Combine(folder, "GeocachePhotos");
                        if (Directory.Exists(checkFolder))
                        {
                            imgFolder = checkFolder;
                        }
                        else
                        {
                            imgFolder = folder;
                        }

                        int index = 0;
                        foreach (var gc in gcList)
                        {
                            string cacheFolder = Path.Combine(imgFolder, gc.Code[gc.Code.Length - 1].ToString());
                            if (Directory.Exists(cacheFolder))
                            {
                                cacheFolder = Path.Combine(cacheFolder, gc.Code[gc.Code.Length - 2].ToString());
                                if (Directory.Exists(cacheFolder))
                                {
                                    cacheFolder = Path.Combine(cacheFolder, gc.Code);
                                    if (Directory.Exists(cacheFolder))
                                    {
                                        Directory.Delete(cacheFolder, true);
                                    }
                                }
                            }


                            index++;
                            if (DateTime.Now >= nextUpdate)
                            {
                                if (!progress.Update("DeletingImages", gcList.Count, index))
                                {
                                    break;
                                }
                                nextUpdate = DateTime.Now.AddSeconds(1);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, e);
                }
            });
        }


        public async Task CreateImageFolder(List<Core.Data.Geocache> gcList, string folder, bool download, bool NotDescrOnly, bool clearBeforeCopy)
        {
            if (download)
            {
                await OfflineImagesManager.Instance.DownloadImagesAsync(gcList, true);
            }
            await Task.Run(() =>
            {
                try
                {
                    DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                    using (Utils.ProgressBlock progress = new Utils.ProgressBlock("CreatingOfflineImageFolder", "CreatingOfflineImageFolder", gcList.Count, 0, true))
                    {
                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }
                        string imgFolder = Path.Combine(folder, "GeocachePhotos");
                        if (!Directory.Exists(imgFolder))
                        {
                            Directory.CreateDirectory(imgFolder);
                        }
                        if (clearBeforeCopy)
                        {
                            string[] fls = Directory.GetFiles(imgFolder);
                            if (fls != null)
                            {
                                foreach (string f in fls)
                                {
                                    File.Delete(f);
                                }
                            }
                            fls = Directory.GetDirectories(imgFolder);
                            if (fls != null)
                            {
                                foreach (string f in fls)
                                {
                                    Directory.Delete(f, true);
                                }
                            }
                        }
                        int index = 0;
                        foreach (Core.Data.Geocache gc in gcList)
                        {
                            int imgIndex = 1;
                            string cacheFolder = "";
                            List<string> urls = Utils.DataAccess.GetImagesOfGeocache(gc.Database, gc.Code, NotDescrOnly);
                            Dictionary<string, string> oimgs = OfflineImagesManager.Instance.GetImages(gc);
                            List<Core.Data.GeocacheImage> imgList = Utils.DataAccess.GetGeocacheImages(gc.Database, gc.Code);
                            foreach (var kp in oimgs)
                            {
                                if (urls.Contains(kp.Key))
                                {
                                    if (string.IsNullOrEmpty(cacheFolder))
                                    {
                                        cacheFolder = Path.Combine(imgFolder, gc.Code[gc.Code.Length - 1].ToString());
                                        if (!Directory.Exists(cacheFolder))
                                        {
                                            Directory.CreateDirectory(cacheFolder);
                                        }
                                        cacheFolder = Path.Combine(cacheFolder, gc.Code[gc.Code.Length - 2].ToString());
                                        if (!Directory.Exists(cacheFolder))
                                        {
                                            Directory.CreateDirectory(cacheFolder);
                                        }
                                        cacheFolder = Path.Combine(cacheFolder, gc.Code);
                                        if (!Directory.Exists(cacheFolder))
                                        {
                                            Directory.CreateDirectory(cacheFolder);
                                        }
                                    }
                                    string imgname = imgIndex.ToString();
                                    if (imgList != null)
                                    {
                                        Core.Data.GeocacheImage img = (from a in imgList where a.Url == kp.Key select a).FirstOrDefault();
                                        if (img != null)
                                        {
                                            imgname = string.Format("{1}{0}", imgIndex, getValidNameForFile(img.Name));
                                        }
                                    }
                                    string dst = Path.Combine(cacheFolder, string.Format("{0}{1}", imgname, Path.GetExtension(kp.Value)));
                                    if (!File.Exists(dst))
                                    {
                                        File.Copy(kp.Value, dst);
                                    }
                                }
                                imgIndex++;
                            }

                            index++;
                            if (DateTime.Now >= nextUpdate)
                            {
                                if (!progress.Update("CreatingOfflineImageFolder", gcList.Count, index))
                                {
                                    break;
                                }
                                nextUpdate = DateTime.Now.AddSeconds(1);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, e);
                }
            });
        }

        private string getValidNameForFile(string name)
        {
            string validChars = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                if (validChars.Contains(name[i]))
                {
                    sb.Append(name[i]);
                }
                else
                {
                    //nope
                }
            }
            return sb.ToString();
        }

    }
}
