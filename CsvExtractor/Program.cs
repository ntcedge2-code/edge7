using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using System.Text.Json.Nodes;
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

var applications = records.Select(item => new
{
    soaNumber = "",
    id = Guid.NewGuid().ToString(),
    _id = Guid.NewGuid().ToString(),

    receiptGoogleSheetUrl = "",
    googleSoaReport = (string?)null,
    type = "",
    documentFileUrl = (string?)null,

    isBulk = (bool?)null,
    isBulkParent = (bool?)null,
    isBulkChildren = (bool?)null,
    appBulkId = (string?)null,

    applicant = new
    {
        _id = Guid.NewGuid().ToString(),
        type = (string?)null,
        userId = Guid.NewGuid().ToString(),
        userType = "Individual",
        companyName = "",
        applicantName = item.OWNER,

        firstName = ExtractFirstName(item.OWNER),
        lastName = ExtractLastName(item.OWNER),
        middleName = "",

        suffix = "",
        nationality = "Filipino",
        sex = "",
        signature = "",
        height = 0,
        weight = 0,

        address = new
        {
            street = "",
            unit = "",
            barangay = "",
            city = item.TOWNCITY,
            province = item.PROVINCE,
            region = "",
            zipCode = ""
        },

        contact = new
        {
            contactNumber = item.ContactNumber,
            email = ""
        },

        dateOfBirth = (string?)null,
        email = (string?)null,

        education = new
        {
            schoolAttended = "",
            courseTaken = "",
            yearGraduated = ""
        },

        profilePicture = (string?)null
    },

    service = new
    {
        _id = "61c400d8ad8d7afe8ee5f617",
        name = "Licenses in the Amateur Service",
        serviceCode = "service-3",

        applicationType = new
        {
            label = GetApplicationTypeLabel(item.ApplicationType),
            elements = new[]
            {
                "Class A",
                "Class B",
                "Class C",
                "Class D"
            },
            formCode = "ntc1-03-AT-RSL",
            requirements = Array.Empty<object>(),
            serviceCode = "AT-RSL",
            sequenceCode = "AT",
            element = $"Class {item.CLASS}"
        },

        applicationDetails = new
        {
            noOfYears = "1"
        },

        license = new
        {
            licenseNumber = item.LIC_PER_NO,
            dateOfExpiry = item.EXPDATE
        },

        basic = new
        {
            userId = "",
            lastName = ExtractLastName(item.OWNER),
            firstName = ExtractFirstName(item.OWNER),
            middleName = "",
            suffix = "",
            dateOfBirth = new
            {
                year = "",
                month = "",
                day = ""
            },
            sex = "",
            nationality = "Filipino"
        },

        callSign = new
        {
            callSign = item.CALLSIGN
        },

        exam = new
        {
            exam = "",
            examDate = "",
            rating = ""
        },

        particulars = new[]
        {
            new
            {
                equipment = new
                {
                    makeTypeModel = $"{item.MAKE} {item.TYPE}".Trim()
                },
                equipments = new[]
                {
                    new
                    {
                        serialNumber = item.SERIAL,
                        frequencyRange = item.FrequencyRange,
                        powerKw = item.PowerKw,
                        classOfStation = item.ClassOfStation
                    }
                }
            }
        }
    },

    serviceName = "licenses in the amateur service",
    applicationProcess = "",
    applicationTypeLabel = GetApplicationTypeLabel(item.ApplicationType).ToLower(),

    region = new
    {
        _id = (string?)null,
        address = "",
        supportEmail = "edge@gov.ph",
        label = "",
        value = "",
        code = ""
    },

    status = "Approved",
    paymentStatus = "Paid",
    paymentMethod = "cash",
    amnesty = (string?)null,

    auditTrail = new[]
    {
        new
        {
            Date = DateTime.UtcNow.ToString("O"),
            From = "",
            To = item.ORNumber,
            Reason = "",
            By = ""
        }
    },

    totalFee = ParseDecimal(item.AMOUNT),
    amnestyTotalFee = (decimal?)null,
    assignedPersonnel = (string?)null,
    isPinned = false,

    approvalHistory = Array.Empty<object>(),
    paymentHistory = Array.Empty<object>(),

    soa = new[]
    {
        new
        {
            id = "5",
            item = "License Fee",
            amount = ParseDecimal(item.AMOUNT),
            validity = 1,
            equipment = 0,
            channel = 0,
            fee = ParseDecimal(item.AMOUNT),
            percent = 0,
            type = (string?)null,
            code = "Validity = 1 Year(s)",
            description = "4-02-01-060",
            applicationName = "",
            Section = "For Licenses"
        }
    },

    soaHistory = (string?)null,
    exam = (string?)null,

    officialReceipt = new
    {
        ORNumber = item.ORNumber,
        pdf = "",
        landscapePdf = "",
        bankName = "",
        payor = item.OWNER,
        checkNumber = "",
        checkDate = "",
        ORBy = (object?)null,
        createdAt = DateTime.UtcNow.ToString("O")
    },

    orderOfPayment = new
    {
        pdf = "",
        fileUrl = "",
        OrderOfPaymentBy = (object?)null,
        createdAt = DateTime.UtcNow.ToString("O"),
        Number = ""
    },

    Make = item.MAKE,

    schedule = new
    {
        id = (string?)null,
        venue = (string?)null,
        region = (string?)null,
        slots = 0,
        seatNumber = (string?)null,
        dateStart = (string?)null,
        dateEnd = (string?)null,
        applicationStartDate = (string?)null,
        applicationEndDate = (string?)null
    },

    proofOfPayment = Array.Empty<object>(),

    personnelIds = Array.Empty<string>(),
    PersonnelNames = Array.Empty<string>(),

    document = "",
    tempDocument = "",
    documentNumber = item.LIC_PER_NO,
    QRCode = "",

    note = "",

    dateOfExpiry = item.EXPDATE,
    validUntil = item.EXPDATE,
    dueDate = (string?)null,

    createdAt = DateTime.UtcNow.ToString("O"),
    soaDocument = (string?)null,
    updatedAt = DateTime.UtcNow.ToString("O"),

    dateOfBirth = (string?)null,
    validity = item.EFFDATE,
    notifyExpiry = (string?)null,

    renew = new
    {
        forRenewal = false,
        renewed = false,
        renewedFrom = (string?)null,
        applicationType = (string?)null
    },

    isModified = false,
    isEndorsed = false,

    referenceNumber = "",
    permitNumber = item.LIC_PER_NO,

    soaReport = (string?)null,
    soaReportPdf = "",
    formDocument = (string?)null,
    reason = Array.Empty<object>(),
    accountableForm = "",

    receiptVoidRequest = (string?)null,
    receiptVoidHistory = Array.Empty<object>(),
    hasPendingReceiptVoidRequest = false,
    latestReceiptVoidStatus = (string?)null,
    latestReceiptVoidUpdatedAt = (string?)null,

    version = "1",
    environment = "1",

    csvSource = new
    {
        owner = item.OWNER,
        classType = item.CLASS,
        nature = item.NATURE,
        applicationType = item.ApplicationType,
        serial = item.SERIAL,
        licensePermitNumber = item.LIC_PER_NO,
        make = item.MAKE,
        type = item.TYPE,
        callSign = item.CALLSIGN,
        orNumber = item.ORNumber,
        townCity = item.TOWNCITY,
        province = item.PROVINCE,
        stationLocation = item.STNLOC,
        datePaid = item.DATEPAID,
        effectiveDate = item.EFFDATE,
        expiryDate = item.EXPDATE,
        dateIssued = item.DATEISS,
        amount = item.AMOUNT,
        contactNumber = item.ContactNumber,
        longitude = item.LONGITUDE,
        latitude = item.LATITUDE,
        classOfStation = item.ClassOfStation,
        powerKw = item.PowerKw,
        frequencyRange = item.FrequencyRange
    }
}).ToList();

var json = JsonSerializer.Serialize(applications, new JsonSerializerOptions
{
    WriteIndented = true
});
var folderPath = Path.GetDirectoryName(filePath) ?? "";
var folderName = string.IsNullOrWhiteSpace(folderPath)
    ? "root"
    : new DirectoryInfo(folderPath).Name;

Directory.CreateDirectory("output");

var safeFileName = Path.GetFileNameWithoutExtension(filePath)
    .Replace(" ", "-")
    .Replace("/", "-")
    .Replace("\\", "-")
    .Replace("(", "")
    .Replace(")", "");

var outputFileName = Path.Combine(
    "output",
    $"amateur-applications-{folderName}-{safeFileName}.json"
);

File.WriteAllText(outputFileName, json);

Console.WriteLine($"JSON created: {outputFileName}");

var cosmosConnectionString = Environment.GetEnvironmentVariable("COSMOS_CONNECTION_STRING");
var cosmosDatabaseName = Environment.GetEnvironmentVariable("COSMOS_DATABASE_NAME");
var cosmosContainerName = Environment.GetEnvironmentVariable("COSMOS_CONTAINER_NAME");

if (string.IsNullOrWhiteSpace(cosmosConnectionString) ||
    string.IsNullOrWhiteSpace(cosmosDatabaseName) ||
    string.IsNullOrWhiteSpace(cosmosContainerName))
{
    Console.WriteLine("Cosmos DB environment variables are missing. JSON was created but not uploaded to Cosmos DB.");
    return;
}

using var cosmosClient = new CosmosClient(cosmosConnectionString);

var database = await cosmosClient.CreateDatabaseIfNotExistsAsync(cosmosDatabaseName);

var container = await database.Database.CreateContainerIfNotExistsAsync(
    id: cosmosContainerName,
    partitionKeyPath: "/region/code"
);

var jsonArray = JsonNode.Parse(json)?.AsArray();

if (jsonArray == null)
{
    Console.WriteLine("No JSON records found to save.");
    return;
}

foreach (var record in jsonArray)
{
    if (record == null)
        continue;

    var id = record["id"]?.ToString();

    if (string.IsNullOrWhiteSpace(id))
    {
        id = Guid.NewGuid().ToString();
        record["id"] = id;
        record["_id"] = id;
    }

    var regionCode = record["region"]?["code"]?.ToString();

    if (string.IsNullOrWhiteSpace(regionCode))
    {
        regionCode = folderName;
        record["region"]!["code"] = regionCode;
        record["region"]!["value"] = regionCode;
        record["region"]!["label"] = $"Region {regionCode}";
    }

    await container.Container.UpsertItemAsync(
        record,
        new PartitionKey(regionCode)
    );

    Console.WriteLine($"Saved to Cosmos DB: {id} | Region: {regionCode}");
}

Console.WriteLine($"Saved {jsonArray.Count} records to Azure Cosmos DB.");


static string GetApplicationTypeLabel(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
        return "Amateur Radio Station License";

    var text = value.Trim().ToUpperInvariant();

    return text switch
    {
        "NEW" => "Amateur Radio Station License (NEW)",
        "REN" => "Amateur Radio Station License (RENEWAL)",
        "RENEWAL" => "Amateur Radio Station License (RENEWAL)",
        "MOD" => "Amateur Radio Station License (MODIFICATION)",
        "MODIFICATION" => "Amateur Radio Station License (MODIFICATION)",
        "DUP" => "Amateur Radio Station License (DUPLICATE)",
        "STORAGE" => "Amateur Radio Station License (STORAGE)",
        _ => $"Amateur Radio Station License ({value})"
    };
}

static decimal ParseDecimal(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
        return 0;

    value = value.Replace(",", "").Trim();

    return decimal.TryParse(value, out var result)
        ? result
        : 0;
}

static string ExtractFirstName(string? fullName)
{
    if (string.IsNullOrWhiteSpace(fullName))
        return "";

    var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

    if (parts.Length <= 1)
        return fullName.Trim();

    return string.Join(" ", parts.Take(parts.Length - 1));
}

static string ExtractLastName(string? fullName)
{
    if (string.IsNullOrWhiteSpace(fullName))
        return "";

    var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

    return parts.LastOrDefault() ?? "";
}
