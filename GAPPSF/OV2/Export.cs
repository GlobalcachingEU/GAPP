using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.OV2
{
    public class Export
    {
        public static void ExportToFile(List<Core.Data.Geocache> gcList, string filename)
        {
            using (Utils.ProgressBlock progress = new Utils.ProgressBlock("ExportingOV2", "CreatingFile", gcList.Count, 0))
            {
                using (System.IO.FileStream fs = System.IO.File.Create(filename))
                {
                    DateTime nextUpdate = DateTime.Now.AddSeconds(1);
                    int index = 0;
                    foreach (Core.Data.Geocache gc in gcList)
                    {
                        StringBuilder sb = new StringBuilder();
                        if (Core.Settings.Default.OV2FieldCoord)
                        {
                            if (sb.Length > 0) sb.Append(",");
                            sb.Append(Utils.Conversion.GetCoordinatesPresentation(gc.Lat, gc.Lon));
                        }
                        if (Core.Settings.Default.OV2FieldCode)
                        {
                            if (sb.Length > 0) sb.Append(",");
                            sb.Append(gc.Code ?? "");
                        }
                        if (Core.Settings.Default.OV2FieldCacheType)
                        {
                            if (sb.Length > 0) sb.Append(",");
                            sb.Append(gc.GeocacheType.Name);
                        }
                        if (Core.Settings.Default.OV2FieldName)
                        {
                            if (sb.Length > 0) sb.Append(",");
                            sb.Append(gc.Name ?? "");
                        }
                        if (Core.Settings.Default.OV2FieldContainer)
                        {
                            if (sb.Length > 0) sb.Append(",");
                            sb.Append(gc.Container.Name);
                        }
                        if (Core.Settings.Default.OV2FieldHint)
                        {
                            if (sb.Length > 0) sb.Append(",");
                            sb.Append(gc.EncodedHints ?? "");
                        }
                        if (Core.Settings.Default.OV2FieldFavorites)
                        {
                            if (sb.Length > 0) sb.Append(",");
                            sb.Append(gc.Favorites.ToString());
                        }
                        if (Core.Settings.Default.OV2FieldOwner)
                        {
                            if (sb.Length > 0) sb.Append(",");
                            sb.Append(gc.Owner ?? "");
                        }
                        if (Core.Settings.Default.OV2FieldDifficulty)
                        {
                            if (sb.Length > 0) sb.Append(",");
                            sb.Append(gc.Difficulty.ToString("0.#"));
                        }
                        if (Core.Settings.Default.OV2FieldTerrain)
                        {
                            if (sb.Length > 0) sb.Append(",");
                            sb.Append(gc.Terrain.ToString("0.#"));
                        }

                        byte[] data;
                        string s = sb.ToString();
                        data = new byte[s.Length];
                        for (int i = 0; i < s.Length; i++)
                        {
                            data[i] = BitConverter.GetBytes(s[i])[0];
                        }
                        //data = System.Text.UnicodeEncoding.UTF8.GetBytes(sb.ToString());
                        fs.WriteByte(2);
                        Int32 len = 13 + data.Length + 1;
                        byte[] arr;
                        arr = BitConverter.GetBytes(len);
                        fs.Write(arr, 0, 4);
                        Int32 x;
                        Int32 y;
                        if (gc.CustomLat != null && gc.CustomLon != null)
                        {
                            x = (Int32)(gc.CustomLon * 100000.0);
                            y = (Int32)(gc.CustomLat * 100000.0);
                        }
                        else
                        {
                            x = (Int32)(gc.Lon * 100000.0);
                            y = (Int32)(gc.Lat * 100000.0);
                        }
                        fs.Write(BitConverter.GetBytes(x), 0, 4);
                        fs.Write(BitConverter.GetBytes(y), 0, 4);
                        fs.Write(data, 0, data.Length);
                        fs.WriteByte(0);

                        index++;
                        if (DateTime.Now >= nextUpdate)
                        {
                            progress.Update("CreatingFile", gcList.Count, index);
                            nextUpdate = DateTime.Now.AddSeconds(1);
                        }
                    }
                }
            }
        }
    }
}
