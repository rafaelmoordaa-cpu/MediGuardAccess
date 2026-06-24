namespace MediGuardAccess;

public enum UserRole
{
    User,
    Admin
}

public sealed class UserAccount
{
    public string FullName { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public UserRole Role { get; set; } = UserRole.User;
    public string? FaceSignature { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class ScreeningRecord
{
    public DateTime Date { get; set; } = DateTime.Now;
    public string Username { get; set; } = "";
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public string Gender { get; set; } = "";
    public string ChiefComplaint { get; set; } = "";

    // Fokus final project: skrining awal hipertensi dan prioritas kardiovaskular.
    public int Systolic { get; set; }
    public int Diastolic { get; set; }
    public int RecheckSystolic { get; set; }
    public int RecheckDiastolic { get; set; }
    public string BpTrendNote { get; set; } = "";

    // Parameter vital pendukung.
    public int SpO2 { get; set; }
    public int Bpm { get; set; }
    public int Rr { get; set; }
    public double Temperature { get; set; }
    public int CoughDays { get; set; } // dipakai sebagai durasi keluhan agar tetap kompatibel dengan versi lama.

    public int SymptomScore { get; set; }
    public double RiskScore { get; set; }
    public string Result { get; set; } = "Normal";
    public string Recommendation { get; set; } = "";
    public string SymptomDetails { get; set; } = "";
    public string Reason { get; set; } = "";
    public string NnSummary { get; set; } = "";
}

public sealed class FuzzyResult
{
    public int SymptomScore { get; init; }
    public double RiskScore { get; init; }
    public string Result { get; init; } = "Normal";
    public string Recommendation { get; init; } = "";
    public string SymptomDetails { get; init; } = "";
    public string Reason { get; init; } = "";
    public string BpTrendNote { get; init; } = "";
}

public sealed class AppDatabaseSnapshot
{
    public List<UserAccount> Users { get; set; } = new();
    public List<ScreeningRecord> Records { get; set; } = new();
}
