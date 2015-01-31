using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GlobalcachingApplication.Plugins.APILimits
{
    public partial class APIInfoForm : Form
    {
        public APIInfoForm()
        {
            InitializeComponent();
        }
    }

    public class LiveAPIInfo : Utils.BasePlugin.Plugin
    {
        public const string ACTION_SHOW = "Live API Info";

        public async override Task<bool> InitializeAsync(Framework.Interfaces.ICore core)
        {
            AddAction(ACTION_SHOW);

            return await base.InitializeAsync(core);
        }

        public override Framework.PluginType PluginType
        {
            get
            {
                return Framework.PluginType.LiveAPI;
            }
        }

        public override string DefaultAction
        {
            get
            {
                return ACTION_SHOW;
            }
        }


        public override bool Action(string action)
        {
            bool result = base.Action(action);
            if (result && action == ACTION_SHOW)
            {
                if (Utils.API.GeocachingLiveV6.CheckAPIAccessAvailable(Core, false))
                {
                    StringBuilder sb = new StringBuilder();
                    try
                    {
                        using (var api = new Utils.API.GeocachingLiveV6(Core))
                        {
                            var resp = api.Client.GetAPILimits(api.Token);
                            sb.AppendLine("CacheLimits: ");
                            foreach(var cl in resp.Limits.CacheLimits)
                            {
                                sb.AppendLine(string.Format("CacheLimit: {0}, InMinutes: {1}", cl.CacheLimit, cl.InMinutes));
                            }
                            sb.AppendLine("LightCacheLimits: ");
                            foreach (var cl in resp.Limits.LiteCacheLimits)
                            {
                                sb.AppendLine(string.Format("LiteCacheLimits: {0}, InMinutes: {1}", cl.CacheLimit, cl.InMinutes));
                            }
                            sb.AppendLine(string.Format("EnforceCacheLimits: {0}", resp.Limits.EnforceCacheLimits));
                            sb.AppendLine(string.Format("EnforceLiteCacheLimits: {0}", resp.Limits.EnforceLiteCacheLimits));
                            sb.AppendLine(string.Format("EnforceMethodLimits: {0}", resp.Limits.EnforceMethodLimits));
                            sb.AppendLine(string.Format("ForMembershipType: {0}", resp.Limits.ForMembershipType));
                            sb.AppendLine(string.Format("MaxCallsbyIPIn1Minute: {0}", resp.Limits.MaxCallsbyIPIn1Minute));
                            sb.AppendLine(string.Format("RestrictbyIP: {0}", resp.Limits.RestrictbyIP));
                            sb.AppendLine(string.Format("ValidateIPCounts: {0}", resp.Limits.ValidateIPCounts));
                            sb.AppendLine("MethodLimits: ");
                            foreach (var cl in resp.Limits.MethodLimits)
                            {
                                sb.AppendLine(string.Format("MethodName: {0}, MaxCalls: {1}, InMinutes: {2}, PartnerMethod = {3}", cl.MethodName, cl.MaxCalls, cl.InMinutes, cl.PartnerMethod));
                            }
                        }
                    }
                    catch
                    {
                        sb.AppendLine();
                        sb.AppendLine("ERROR");
                    }
                    using (APIInfoForm dlg = new APIInfoForm())
                    {
                        dlg.textBox1.Text = sb.ToString();
                        dlg.ShowDialog();
                    }
                }
            }
            return result;
        }
    }
}
