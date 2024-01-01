namespace DnsTool;

public class ConfigInfo
{
    public string ApiUrl { get; set; } = "https://api.cloudflare.com/client/v4/";
    public string AuthEmail { get; set; } = "example@example.com";
    public string AuthKey { get; set; } = "changeMe!changeMe!changeMe!changeMe";
    public string Domain { get; set; } = "example.Tld";
    public string DnsZoneId { get; set; } = "changeMe!changeMe!changeMe!changeMe";
    public string DnsRecordId { get; set; } = "changeMe!changeMe!changeMe!changeMe";
    public bool DnsRecordSet { get; set; }
}
