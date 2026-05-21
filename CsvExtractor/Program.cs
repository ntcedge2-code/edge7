using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

var filePath = args.Length > 0 
    ? args[0] 
    : "AMATEUR Query.csv";

if (!File.Exists(filePath))
{
    Console.WriteLine($"File not found: {filePath}");
    return;
}

var config = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    HeaderValidated = null,
    MissingFieldFound = null,
    BadDataFound = null,
    TrimOptions = TrimOptions.Trim,
};

using var reader = new StreamReader(filePath);
using var csv = new CsvReader(reader, config);

var records = csv.GetRecords<AmateurRecord>().ToList();

Console.WriteLine($"Total records extracted: {records.Count}");

foreach (var item in records)
{
    Console.WriteLine($"{item.OWNER} | {item.CALLSIGN} | {item.LIC_PER_NO} | {item.PROVINCE}");
}