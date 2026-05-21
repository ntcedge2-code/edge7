using CsvHelper.Configuration.Attributes;

public class AmateurRecord
{
    public string? OWNER { get; set; }
    public string? CLASS { get; set; }
    public string? NATURE { get; set; }

    [Name("NEW/REN/MOD/DUP/STORAGE")]
    public string? ApplicationType { get; set; }

    public string? SERIAL { get; set; }
    public string? LIC_PER_NO { get; set; }
    public string? MAKE { get; set; }
    public string? TYPE { get; set; }
    public string? CALLSIGN { get; set; }

    [Name("OR#")]
    public string? ORNumber { get; set; }

    public string? TOWNCITY { get; set; }
    public string? PROVINCE { get; set; }
    public string? STNLOC { get; set; }

    public string? DATEPAID { get; set; }
    public string? EFFDATE { get; set; }
    public string? EXPDATE { get; set; }
    public string? DATEISS { get; set; }

    public string? AMOUNT { get; set; }

    [Name("CONTACT NUMBER")]
    public string? ContactNumber { get; set; }

    public string? LONGITUDE { get; set; }
    public string? LATITUDE { get; set; }

    [Name("CLASS OF STATION")]
    public string? ClassOfStation { get; set; }

    [Name("POWER(kW)")]
    public string? PowerKw { get; set; }

    [Name("FREQUENCY RANGE")]
    public string? FrequencyRange { get; set; }
}
