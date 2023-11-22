using System.Reflection;
using System.Text.Json;
using WatsonWebserver;
using WatsonWebserver.Core;

namespace bve_traincrew_bridge;

public class TascObject
{
    public bool? Power { get; set; }
    public bool? Monitor { get; set; }
    public int? Brake { get; set; }
    public int? Position { get; set; }
    public bool? Inching { get; set; }
}

// Create a webserver using WatsonWebserver and respond with the TASC data

public interface RESTApi
{
    public void SetTascObject(TascObject tascObject);
}

public class EmptyRestApi : RESTApi
{
    public void SetTascObject(TascObject tascObject){}
}

public class IRestApi: RESTApi
{
    private TascObject _tascObject;
    public IRestApi(int port)
    {
        _tascObject = new TascObject();
        var serverSettings = new WebserverSettings()
        {
            Hostname = "127.0.0.1",
            Port = port
        };
        var server = new Webserver(serverSettings, DefaultRoute);

        server.Routes.PreAuthentication.Static.Add(WatsonWebserver.Core.HttpMethod.GET, "/data", GetDataRoute);
        
        Console.WriteLine($"タヌ電TIMSの改造APIサーバーを起動しました。（ポート{port}）");
        server.Start();
    }

    public void SetTascObject(TascObject tascObject)
    {
        _tascObject = tascObject;
    }
    
    private async Task DefaultRoute(HttpContextBase ctx)
    {
        var assemblyName = Assembly.GetExecutingAssembly().GetName();
        
        // create an object with "serviceName" and "owner" properties
        var serviceInfo = new
        {
            serviceName = assemblyName.Name,
            timestamp = DateTime.Now
        };
        
        var json = JsonSerializer.Serialize(serviceInfo);
        ctx.Response.ContentType = "application/json";
        await ctx.Response.Send(json);
    }
    
    private async Task GetDataRoute(HttpContextBase ctx)
    {
        // Wrap in Tanuden API Standard
        var data = new
        {
            tanudenTIMSExtData = new
            {
                trainState = new
                {
                    lamps = new
                    {
                        tasc = new
                        {
                            power = _tascObject.Power,
                            monitor = _tascObject.Monitor,
                            brake = _tascObject.Brake,
                            position = _tascObject.Position,
                            inching = _tascObject.Inching
                        }
                    }
                }
            }
        };
        
        var json = JsonSerializer.Serialize(data);
        ctx.Response.ContentType = "application/json";
        await ctx.Response.Send(json);
    }
}
