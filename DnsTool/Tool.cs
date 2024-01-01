using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DnsTool;

internal class Tool
{
    public bool ShouldSetRecordId => configInfo != null && !configInfo.DnsRecordSet;

    private ConfigInfo? configInfo;
    private readonly JsonSerializerOptions serializerOptions;

    private readonly string currPath;
    private readonly string fullPath;

    private const string configName = "Config.json";

    public Tool()
    {
        currPath = Directory.GetCurrentDirectory();
        fullPath = Path.Combine(currPath, configName);

        serializerOptions = new JsonSerializerOptions()
        {
            WriteIndented = true
        };
    }

    public bool CheckConfig()
    {
        if (!File.Exists(fullPath))
        {
            string jsonText = JsonSerializer.Serialize(new ConfigInfo(), serializerOptions);
            File.WriteAllText(fullPath, jsonText);

            Console.WriteLine("a new config file was created... please start this tool again");
            return false;
        }

        string allText = File.ReadAllText(fullPath);

        if (JsonSerializer.Deserialize<ConfigInfo>(allText, serializerOptions) is ConfigInfo fromFile)
        {
            configInfo = fromFile;
        }

        return configInfo != null;
    }

    public void SetDnsRecordId(string id)
    {
        configInfo.DnsRecordId = id;
        configInfo.DnsRecordSet = true;
        File.WriteAllText(fullPath, JsonSerializer.Serialize(configInfo, serializerOptions));
    }

    public async void GetRecords()
    {
        using HttpClient httpClient = new();
        httpClient.BaseAddress = new Uri(configInfo.ApiUrl);
        httpClient.DefaultRequestHeaders.Add("X-Auth-Email", configInfo.AuthEmail);
        httpClient.DefaultRequestHeaders.Add("X-Auth-Key", configInfo.AuthKey);

        HttpResponseMessage response = await httpClient.GetAsync($"zones/{configInfo.DnsZoneId}/dns_records");
        Console.WriteLine(await response.Content.ReadAsStringAsync());
    }

    private async Task<string> GetExternalIpAsString()
    {
        using HttpClient client = new();
        HttpResponseMessage response = await client.GetAsync(requestUri: new Uri("http://checkip.dyndns.com"));
        string contentString = await response.Content.ReadAsStringAsync();
        contentString = contentString.Replace("<html><head><title>Current IP Check</title></head><body>Current IP Address:", "");
        contentString = contentString.Replace("</body></html>", "");
        return Regex.Replace(contentString, @"\s+", ""); ;
    }

    public async Task UpdateDnsRecordOnApi()
    {
        using HttpClient httpClient = new();
        httpClient.BaseAddress = new Uri(configInfo.ApiUrl);
        httpClient.DefaultRequestHeaders.Add("X-Auth-Email", configInfo.AuthEmail);
        httpClient.DefaultRequestHeaders.Add("X-Auth-Key", configInfo.AuthKey);

        RecordInfo recordInfo = new()
        {
            type = "A",
            name = configInfo.Domain,
            content = await GetExternalIpAsString(),
            ttl = null,
            proxied = true
        };

        string infostring = JsonSerializer.Serialize(recordInfo);

        StringContent httpContent = new(infostring, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await httpClient.PutAsync($"zones/{configInfo.DnsZoneId}/dns_records/{configInfo.DnsRecordId}", httpContent);

        await Console.Out.WriteLineAsync(await response.Content.ReadAsStringAsync());
        await Console.Out.WriteLineAsync(response.StatusCode.ToString());
    }
}

internal class RecordInfo
{
    public string? type { get; set; }
    public string? name { get; set; }
    public string? content { get; set; }
    public object? ttl { get; set; }
    public bool proxied { get; set; }
}