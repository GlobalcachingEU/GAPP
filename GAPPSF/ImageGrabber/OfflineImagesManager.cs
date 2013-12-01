using GAPPSF.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.ImageGrabber
{
    public class OfflineImagesManager
    {
        private const string DATABASE_FILENAME = "grabbedimg.db3";
        private const string IMG_SUBFOLDER = "GrabbedImages";

        private static OfflineImagesManager _uniqueInstance = null;
        private static object _lockObject = new object();

        private string _imageFolder = null;
        private DBCon _dbcon = null;

        private OfflineImagesManager()
        {
            try
            {
                _imageFolder = Path.Combine(Core.Settings.Default.SettingsFolder, "OfflineImages");
                if (!System.IO.Directory.Exists(_imageFolder))
                {
                    System.IO.Directory.CreateDirectory(_imageFolder);
                }
            }
            catch
            {
            }
        }

        public OfflineImagesManager Instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    lock (_lockObject)
                    {
                        if (_uniqueInstance==null)
                        {
                            _uniqueInstance = new OfflineImagesManager();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }

        private void CloseDatabase()
        {
            if (_dbcon != null)
            {
                _dbcon.Dispose();
                _dbcon = null;
            }
        }

        private string DatabaseFilename
        {
            get { return System.IO.Path.Combine(_imageFolder, DATABASE_FILENAME); }
        }

        private bool InitDatabase()
        {
            CloseDatabase();
            bool result = false;
            try
            {
                _dbcon = new DBConComSqlite(DatabaseFilename);
                object o = _dbcon.ExecuteScalar("SELECT name FROM sqlite_master WHERE type='table' AND name='images'");
                if (o == null || o.GetType() == typeof(DBNull))
                {
                    _dbcon.ExecuteNonQuery("create table 'images' (org_url text, gccode text, local_file text)");
                    _dbcon.ExecuteNonQuery("create index idx_images on images (org_url_idx)");
                    _dbcon.ExecuteNonQuery("create index idx_gccodes on images (gcode_idx)");
                }
                result = true;
            }
            catch
            {
            }
            return result;
        }

    }
}
