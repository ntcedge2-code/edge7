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
    var line = string.Join(" | ", new[]
    {
        item.OWNER,
        item.CLASS,
        item.NATURE,
        item.ApplicationType,
        item.SERIAL,
        item.LIC_PER_NO,
        item.MAKE,
        item.TYPE,
        item.CALLSIGN,
        item.ORNumber,
        item.TOWNCITY,
        item.PROVINCE,
        item.STNLOC,
        item.DATEPAID,
        item.EFFDATE,
        item.EXPDATE,
        item.DATEISS,
        item.AMOUNT,
        item.ContactNumber,
        item.LONGITUDE,
        item.LATITUDE,
        item.ClassOfStation,
        item.PowerKw,
        item.FrequencyRange
    });

    Console.WriteLine(line);
}
