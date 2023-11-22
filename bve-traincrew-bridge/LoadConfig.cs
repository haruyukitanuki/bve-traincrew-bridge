using IniParser;
using IniParser.Model;

namespace bve_traincrew_bridge;

public class TASCConfigObject
{
    public int tascenabled { get; set; }
    public int tascmonitor { get; set; }
    public int tascbrake { get; set; }
    public int tascposition { get; set; }
    public int inching { get; set; }
}

public static class Config
{
    public static bool RestApiEnable { get; set; } = false;
    public static int RestApiPort { get; set; } = 56001;
    public static TASCConfigObject TASCConfig { get; set; } = new();
    
    public static void LoadConfig()
    {
        // Todo: ロギング用のライブラリを使う
        var parser = new FileIniDataParser();
        IniData appConfig = new();
        IniData autoPilotConfig = new();
        
        // Bridge config
        try
        {
            appConfig = parser.ReadFile("bridge_config.ini");
        }
        catch (Exception)
        {
            Console.WriteLine("設定ファイルが見つかりませんでした。デフォルト設定で起動します。");
        }

        // TASC, Autopilot config
        try
        {
            autoPilotConfig = parser.ReadFile("autopilot.ini");
        }
        catch (Exception)
        {
            Console.WriteLine("自動運転プラグインの設定ファイルが見つかりませんでした。デフォルト設定で起動します。");
            throw;
        }
        
        
        const string tanudenModApiSectionName = "tanuden_mod_api";
        // Only if the key exists, set it
        if (appConfig[tanudenModApiSectionName].ContainsKey("enable"))
        {
            RestApiEnable = bool.Parse(appConfig[tanudenModApiSectionName]["enable"]);
        }
        
        if (appConfig[tanudenModApiSectionName].ContainsKey("port"))
        {
            RestApiPort = int.Parse(appConfig[tanudenModApiSectionName]["port"]);
        }
        
        
        // Set for autopilot
        const string panelSectionName = "panel";
        if (!RestApiEnable) return;
        if (autoPilotConfig[panelSectionName].ContainsKey("tascenabled"))
        {
            TASCConfig.tascenabled = int.Parse(autoPilotConfig[panelSectionName]["tascenabled"]);
        }
        if (autoPilotConfig[panelSectionName].ContainsKey("tascmonitor"))
        {
            TASCConfig.tascmonitor = int.Parse(autoPilotConfig[panelSectionName]["tascmonitor"]);
        }
        if (autoPilotConfig[panelSectionName].ContainsKey("tascbrake"))
        {
            TASCConfig.tascbrake = int.Parse(autoPilotConfig[panelSectionName]["tascbrake"]);
        }
        if (autoPilotConfig[panelSectionName].ContainsKey("tascposition"))
        {
            TASCConfig.tascposition = int.Parse(autoPilotConfig[panelSectionName]["tascposition"]);
        }
        if (autoPilotConfig[panelSectionName].ContainsKey("inching"))
        {
            TASCConfig.inching = int.Parse(autoPilotConfig[panelSectionName]["inching"]);
        }
    }
}