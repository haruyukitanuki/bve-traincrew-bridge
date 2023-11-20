using IniParser;
using IniParser.Model;

namespace bve_traincrew_bridge;

public class LoadConfig
{
    public bool RestApiEnable { get; set; } = false;
    public int RestApiPort { get; set; } = 56001;
    
    public LoadConfig()
    {
        var parser = new FileIniDataParser();
        IniData data = new();
        try
        {
            data = parser.ReadFile("bridge_config.ini");
        }
        catch (Exception)
        {
            Console.WriteLine("設定ファイルが見つけません。デフォルト設定で起動します。");
        }
        
        const string tanudenModApiSectionName = "tanuden_mod_api";

        // Only if the key exists, set it
        if (data[tanudenModApiSectionName].ContainsKey("enable"))
        {
            RestApiEnable = bool.Parse(data[tanudenModApiSectionName]["enable"]);
        }
        
        if (data[tanudenModApiSectionName].ContainsKey("port"))
        {
            RestApiPort = int.Parse(data[tanudenModApiSectionName]["port"]);
        }
    }
}