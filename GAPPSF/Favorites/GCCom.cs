using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPSF.Favorites
{
    public class GCCom
    {
        public async Task GetAllYourFavoriteGeocachesAsync(Core.Storage.Database db, bool importMissing)
        {
            using (Utils.DataUpdater upd = new Utils.DataUpdater(db))
            {
                await Task.Run(() =>
                {
                    try
                    {
                        List<string> gcList = null;
                        using (var api = new LiveAPI.GeocachingLiveV6())
                        {
                            var respC = api.Client.GetCacheIdsFavoritedByUser(api.Token);
                            if (respC.Status.StatusCode == 0)
                            {
                                gcList = (from s in respC.CacheCodes select s).ToList();
                                Manager.Instance.AddFavoritedGeocaches(gcList);
                            }
                            else
                            {
                                Core.ApplicationData.Instance.Logger.AddLog(this, new Exception(respC.Status.StatusMessage));
                            }
                        }
                        if (gcList != null && gcList.Count > 0 && importMissing)
                        {
                            List<string> missingList = (from a in gcList where db.GeocacheCollection.GetGeocache(a) == null select a).ToList();
                            LiveAPI.Import.ImportGeocaches(db, gcList);
                        }
                    }
                    catch (Exception e)
                    {
                        Core.ApplicationData.Instance.Logger.AddLog(this, e);
                    }
                });
            }
        }

        public async Task AddFavoriteGeocacheAsync(Core.Data.Geocache gc)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var api = new LiveAPI.GeocachingLiveV6())
                    {
                        var resp = api.Client.AddFavoritePointToCache(api.Token, gc.Code);
                        if (resp.Status.StatusCode == 0)
                        {
                            Manager.Instance.AddFavoritedGeocache(gc.Code);
                        }
                        else
                        {
                            Core.ApplicationData.Instance.Logger.AddLog(this, new Exception(resp.Status.StatusMessage));
                        }
                    }
                }
                catch (Exception e)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, e);
                }
            });
        }

        public async Task RemoveFavoriteGeocacheAsync(Core.Data.Geocache gc)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var api = new LiveAPI.GeocachingLiveV6())
                    {
                        var resp = api.Client.RemoveFavoritePointFromCache(api.Token, gc.Code);
                        if (resp.Status.StatusCode == 0)
                        {
                            Manager.Instance.RemoveFavoritedGeocache(gc.Code);
                        }
                        else
                        {
                            Core.ApplicationData.Instance.Logger.AddLog(this, new Exception(resp.Status.StatusMessage));
                        }
                    }
                }
                catch (Exception e)
                {
                    Core.ApplicationData.Instance.Logger.AddLog(this, e);
                }
            });
        }

    }

}
