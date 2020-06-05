using Exiled.API.Interfaces;
using Exiled.Loader;

namespace AdminTools
{
    public class Config : IConfig
    {
        public void Reload()
        {
            GodTuts = PluginManager.YamlConfig.GetBool("admin_god_tuts", true);
        }

        public bool GodTuts { get; set; }
        public bool IsEnabled { get; set; }
        public string Prefix { get; } = "at";
    }
}