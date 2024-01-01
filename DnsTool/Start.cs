using DnsTool;

Tool tool = new();

if (tool.CheckConfig())
{
    if (tool.ShouldSetRecordId)
    {
        tool.GetRecords();

        if (Console.ReadLine() is string input && !string.IsNullOrWhiteSpace(input))
        {
            tool.SetDnsRecordId(input);
        }
    }

    await tool.UpdateDnsRecordOnApi();
}