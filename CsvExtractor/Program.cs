using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Azure.Cosmos;
using System.Globalization;
using System.Text.Json;

var filePath = args.Length > 0
    ? args[0]
    : "AMATEUR Query.csv";

if (!File.Exists(filePath))
{
    Console.WriteLine($"File not found: {filePath}");
    return;
}

var cosmosConnectionString = Environment.GetEnvironmentVariable("COSMOS_CONNECTION_STRING");
var cosmosDatabaseName = Environment.GetEnvironmentVariable("COSMOS_DATABASE_NAME");
var cosmosContainerName = Environment.GetEnvironmentVariable("COSMOS_CONTAINER_NAME");

if (string.IsNullOrWhiteSpace(cosmosConnectionString) ||
    string.IsNullOrWhiteSpace(cosmosDatabaseName) ||
    string.IsNullOrWhiteSpace(cosmosContainerName))
{
    Console.WriteLine("Cosmos DB environment variables are missing.");
    return;
}

using var cosmosClient = new CosmosClient(cosmosConnectionString);

var applicationsContainer = cosmosClient.GetContainer(
    cosmosDatabaseName,
    cosmosContainerName
);

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

var folderPath = Path.GetDirectoryName(filePath) ?? "";
var folderName = string.IsNullOrWhiteSpace(folderPath)
    ? "root"
    : new DirectoryInfo(folderPath).Name;

var regionCode = folderName;
var regionPrefix = "RO" + regionCode;

var applicationItems = new List<(object App, string Id)>();

foreach (var item in records)
{
    var id = Guid.NewGuid().ToString();
    var applicantId = Guid.NewGuid().ToString();
    var userId = Guid.NewGuid().ToString();

    var soaSeries = await GetNextPrefixNumberAsync(
        cosmosClient,
        cosmosDatabaseName,
        "SOA_REFERENCE",
        regionPrefix,
        0
    );

    var opSeries = await GetNextPrefixNumberAsync(
        cosmosClient,
        cosmosDatabaseName,
        "OP_REFERENCE",
        regionPrefix,
        0
    );

    var globalSeries = await GetNextPrefixNumberAsync(
        cosmosClient,
        cosmosDatabaseName,
        "GLOBAL_REFERENCE",
        regionCode,
        1000
    );

    var soaNumberValue = GenerateSoaNumber(regionCode, soaSeries);
    var opNumberValue = GenerateOpNumber(regionCode, opSeries);
    var referenceNumberValue = GenerateReferenceNumber(regionCode, globalSeries);

    var documentNumberValue = !string.IsNullOrWhiteSpace(item.LIC_PER_NO)
        ? item.LIC_PER_NO
        : GenerateDocumentNumber(regionCode, "AT", globalSeries);

    var permitNumberValue = !string.IsNullOrWhiteSpace(item.LIC_PER_NO)
        ? item.LIC_PER_NO
        : GeneratePermitNumber(regionCode, "AT", globalSeries);

    var app = new
    {
        soaNumber = soaNumberValue,
        id = id,
        _id = id,

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
            _id = applicantId,
            type = (string?)null,
            userId = userId,
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
                region = regionCode,
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
                requirements = GetRequirements(item.ApplicationType),
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
                userId = userId,
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
            label = "Region " + regionCode,
            value = regionCode,
            code = regionCode
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
            Number = opNumberValue
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
        documentNumber = documentNumberValue,
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

        referenceNumber = referenceNumberValue,
        permitNumber = permitNumberValue,

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
    };

    applicationItems.Add((app, id));
}

var applications = applicationItems.Select(x => x.App).ToList();

var json = JsonSerializer.Serialize(applications, new JsonSerializerOptions
{
    WriteIndented = true
});

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

foreach (var item in applicationItems)
{
    await applicationsContainer.UpsertItemAsync(
        item.App,
        new PartitionKey(item.Id)
    );

    Console.WriteLine($"Saved to Cosmos DB Applications container: {item.Id}");
}

Console.WriteLine($"Saved {applicationItems.Count} records to Cosmos DB.");
static object[] GetRequirements(string? applicationType)
{
    var type = applicationType?.Trim().ToUpperInvariant() ?? "";

    if (type is "NEW")
    {
        return new object[]
        {
            new
            {
                key = "purchase-possess-permit",
                title = "Permit to Purchase/Possess",
                required = true
            },
            new
            {
                key = "amateur-roc",
                title = "For Amateur Radio Operator Certificate (AT-ROC) holders:",
                description = "Valid AT-ROC",
                required = false
            },
            new
            {
                key = "source-of-equipment-proof",
                title = "Copy of document indicating source of equipment:",
                description = "(a) For locally-sourced equipment, Official Receipt or Sales Invoice from authorized Radio Dealer, OR\n(b) For imported equipment, Copy of Invoice from the supplier AND Copy of Permit to Import, OR\n(c) For equipment from licensed Amateur, Permit to Sell/Transfer AND Original AT-RSL of the Seller\nNote 1: Apply for Duplicate Copy if Original is lost/mutilated/destroyed or not available.",
                required = true
            },
            new
            {
                key = "id-picture",
                title = "Please provide a clear 1x1 ID picture taken within the last six (6) months.",
                required = true
            }
        };
    }

    if (type is "REN" or "RENEWAL")
    {
        return new object[]
        {
            new
            {
                key = "photocopy-of-at-rsl",
                title = "Photocopy of AT-RSL",
                required = true
            },
            new
            {
                key = "amateur-activities-proof",
                title = "Proof of Amateur Activities",
                required = true
            },
            new
            {
                key = "id-picture",
                title = "Please provide a clear 1x1 ID picture taken within the last six (6) months.",
                required = true
            }
        };
    }

    if (type is "MOD" or "MODIFICATION")
    {
        return new object[]
        {
            new
            {
                key = "id-picture",
                title = "Please provide a clear 1x1 ID picture taken within the last six (6) months.",
                required = true
            }
        };
    }

    if (type is "DUP" or "DUPLICATE")
    {
        return new object[]
        {
            new
            {
                key = "affidavit-of-loss",
                title = "Affidavit of Loss or proof of lost/mutilated/destroyed license",
                required = true
            },
            new
            {
                key = "id-picture",
                title = "Please provide a clear 1x1 ID picture taken within the last six (6) months.",
                required = true
            }
        };
    }

    if (type is "STORAGE")
    {
        return new object[]
        {
            new
            {
                key = "storage-request",
                title = "Request or supporting document for storage",
                required = true
            },
            new
            {
                key = "permit-to-possess",
                title = "Permit to Purchase/Possess",
                required = true
            }
        };
    }

    return Array.Empty<object>();
}
static async Task<int> GetNextPrefixNumberAsync(
    CosmosClient cosmosClient,
    string databaseName,
    string prefixName,
    string regionPrefix,
    int initialLastNumber)
{
    var prefixContainer = cosmosClient.GetContainer(databaseName, "DocumentPrefixes");

    var query = new QueryDefinition(
        "SELECT TOP 1 * FROM c WHERE c.Prefix = @prefix AND c.RegionPrefix = @regionPrefix"
    )
    .WithParameter("@prefix", prefixName)
    .WithParameter("@regionPrefix", regionPrefix);

    var iterator = prefixContainer.GetItemQueryIterator<DocumentPrefixRecord>(query);

    DocumentPrefixRecord? prefixRecord = null;

    while (iterator.HasMoreResults)
    {
        var response = await iterator.ReadNextAsync();
        prefixRecord = response.FirstOrDefault();

        if (prefixRecord != null)
            break;
    }

    var now = DateTime.UtcNow;

    if (prefixRecord == null)
    {
        prefixRecord = new DocumentPrefixRecord
        {
            id = Guid.NewGuid().ToString(),
            _id = "",
            Prefix = prefixName,
            LastNumber = initialLastNumber,
            RegionPrefix = regionPrefix,
            LastMonth = now.Month,
            LastYear = now.Year,
            Suffix = "",
            LastUpdated = now
        };
    }

    if (prefixRecord.LastYear == 0)
    {
        prefixRecord.LastYear = now.Year;
    }

    if (prefixRecord.LastYear != now.Year)
    {
        prefixRecord.LastNumber = initialLastNumber;
        prefixRecord.LastYear = now.Year;
        prefixRecord.LastMonth = now.Month;
    }

    prefixRecord.LastNumber += 1;
    prefixRecord.LastUpdated = now;

    if (string.IsNullOrWhiteSpace(prefixRecord._id))
    {
        prefixRecord._id = prefixRecord.id;
    }

    await prefixContainer.UpsertItemAsync(
        prefixRecord,
        new PartitionKey(prefixRecord.id)
    );

    return prefixRecord.LastNumber;
}

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

    return decimal.TryParse(
        value,
        NumberStyles.Any,
        CultureInfo.InvariantCulture,
        out var result
    )
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

static string GenerateSoaNumber(string regionCode, int seriesNumber)
{
    var now = DateTime.UtcNow;
    var year = now.Year.ToString();
    var month = now.Month.ToString("D2");

    var prefix = regionCode switch
    {
        "XIII" => "70",
        "IV-A" => "43A RO",
        "XIV" => "43A RO",
        _ => "61"
    };

    return $"{prefix}-{year}-{month}-{seriesNumber:D4}";
}

static string GenerateOpNumber(string regionCode, int seriesNumber)
{
    var now = DateTime.UtcNow;
    var year = now.Year.ToString();
    var month = now.Month.ToString("D2");

    var prefix = regionCode switch
    {
        "XIII" => "71",
        "IV-A" => "",
        _ => "61"
    };

    return string.IsNullOrWhiteSpace(prefix)
        ? $"{year}-{month}-{seriesNumber:D4}"
        : $"{prefix}-{year}-{month}-{seriesNumber:D4}";
}

static string GenerateReferenceNumber(string regionCode, int globalSeriesNumber)
{
    var now = DateTime.UtcNow;
    var year = now.Year.ToString();
    var month = now.Month.ToString();

    var prefix = regionCode switch
    {
        "XIII" => "70",
        "IV-A" => "43A",
        _ => "61"
    };

    var random3 = Math.Abs((int)(DateTime.UtcNow.Ticks % 1000));

    return $"{prefix}-{month}-{year}-{globalSeriesNumber}-{random3:D3}";
}

static string GenerateDocumentNumber(string regionCode, string sequenceCode, int seriesNumber)
{
    var year = DateTime.UtcNow.Year.ToString();
    var regionPrefix = "RO" + regionCode;
    var number = seriesNumber.ToString("D5");

    return $"{sequenceCode}-{regionPrefix}-{number}-{year}";
}

static string GeneratePermitNumber(string regionCode, string sequenceCode, int seriesNumber)
{
    var yy = DateTime.UtcNow.ToString("yy");
    var regionPrefix = "RO" + regionCode;
    var number = seriesNumber.ToString("D5");

    return $"{sequenceCode}-{regionPrefix}-{number}-{yy}";
}

public class DocumentPrefixRecord
{
    public string id { get; set; } = Guid.NewGuid().ToString();
    public string? _id { get; set; }

    public string? Prefix { get; set; }
    public int LastNumber { get; set; }
    public string? RegionPrefix { get; set; }
    public int LastMonth { get; set; }
    public int LastYear { get; set; }
    public string? Suffix { get; set; }
    public string? RegionCode { get; set; }
    public string? ApplicationId { get; set; }
    public string? Label { get; set; }
    public bool IsUsed { get; set; }
    public DateTime LastUpdated { get; set; }
}