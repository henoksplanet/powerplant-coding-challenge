using LoadAPI;
using Newtonsoft.Json;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();




app.MapPost("/productionplan", async delegate (HttpContext context)
{
    using (StreamReader reader = new StreamReader(context.Request.Body, Encoding.UTF8))
    {
        string jsonstring = await reader.ReadToEndAsync();
        Root? deserializedPayload = JsonConvert.DeserializeObject<Root>(jsonstring);

        string? responsePayload = null;

        LoadBalancing loading = new LoadBalancing(deserializedPayload);

        loading.loadDistribution();
        if (!loading.validateAllTurbineLoad())
        {
            throw new Exception("some turbines violate allowed power range!");
        }

        responsePayload += "[\n";
        foreach (LoadBalancing.PowerplantExtended powerplant in loading.allTurbinesExtended)
        {
            PayloadResponse _payload = new PayloadResponse();
            _payload.name = powerplant.name;
            _payload.p = powerplant.p;

            responsePayload += JsonConvert.SerializeObject(_payload, Formatting.Indented);
            if(!powerplant.Equals(loading.allTurbinesExtended.Last()))
            {
                responsePayload += ",\n";
            }

        }
        responsePayload += "\n]";

        return responsePayload;
    }
});

app.Run();
