namespace MediGuardAccess;

public static class AppData
{
    public static UserAccount? CurrentUser { get; private set; }

    public static readonly List<UserAccount> Users = new();
    public static readonly List<ScreeningRecord> Records = new();

    static AppData()
    {
        LoadFromDatabase();
    }

    private static void LoadFromDatabase()
    {
        var snapshot = LocalDatabaseService.Load();

        if (snapshot.Users.Count > 0)
        {
            Users.AddRange(snapshot.Users);
            Records.AddRange(snapshot.Records);
            return;
        }

        Users.AddRange(new[]
        {
            new UserAccount { FullName = "Admin MediGuardAccess", Username = "admin", Password = "admin123", Role = UserRole.Admin },
            new UserAccount { FullName = "User Demo", Username = "user", Password = "user123", Role = UserRole.User }
        });

        Records.AddRange(new[]
        {
            new ScreeningRecord
            {
                Username="user",
                Name="Ahmad",
                Age=21,
                Gender="Laki-laki",
                ChiefComplaint="Tidak ada keluhan berat",
                Systolic=118,
                Diastolic=76,
                RecheckSystolic=116,
                RecheckDiastolic=74,
                BpTrendNote="Smart BP Recheck: hasil ulang relatif stabil (118/76 → 116/74 mmHg).",
                SpO2=98,
                Bpm=78,
                Rr=16,
                Temperature=36.8,
                CoughDays=2,
                SymptomScore=0,
                RiskScore=0,
                Result="Normal",
                Recommendation="Tekanan darah dan parameter pendukung tampak relatif aman untuk pemantauan mandiri.",
                SymptomDetails="Tidak ada red flag kardiovaskular atau faktor risiko tambahan yang dipilih.",
                Reason="Output Normal karena tekanan darah, parameter vital, dan red flag kardiovaskular berada pada rentang aman.",
                NnSummary="Simulasi CardioNet feed-forward: kecenderungan risiko rendah, kelas estimasi Normal."
            },
            new ScreeningRecord
            {
                Username="user",
                Name="Citra",
                Age=28,
                Gender="Perempuan",
                ChiefComplaint="Pusing, dada terasa berat, dan tekanan darah tinggi",
                Systolic=168,
                Diastolic=104,
                RecheckSystolic=164,
                RecheckDiastolic=101,
                BpTrendNote="Smart BP Recheck: hasil ulang masih tinggi (168/104 → 164/101 mmHg). Perlu konsultasi bila menetap.",
                SpO2=96,
                Bpm=112,
                Rr=25,
                Temperature=38.1,
                CoughDays=8,
                SymptomScore=11,
                RiskScore=83.3,
                Result="Berisiko",
                Recommendation="Terdapat tanda prioritas. Segera lakukan evaluasi ke fasilitas kesehatan, terutama bila ada nyeri dada berat, sesak berat, pingsan, kebingungan, nyeri perut hebat, atau SpO2 rendah.",
                SymptomDetails="Nyeri dada berat/menekan: +4\nSesak napas berat: +4\nLemah berat/dehidrasi: +3",
                Reason="Output Berisiko karena terdapat kombinasi SpO2 rendah, frekuensi napas tinggi, durasi keluhan, dan indikator keselamatan tinggi.",
                NnSummary="Simulasi NN feed-forward: probabilitas risiko tinggi, kelas estimasi Berisiko."
            }
        });

        Save();
    }

    public static bool Register(string name, string username, string password, UserRole role, string? faceSignature, out string message)
    {
        username = username.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            message = "Nama, username, dan password wajib diisi.";
            return false;
        }

        if (Users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
        {
            message = "Username sudah digunakan.";
            return false;
        }

        Users.Add(new UserAccount
        {
            FullName = name.Trim(),
            Username = username,
            Password = password,
            Role = role,
            FaceSignature = faceSignature
        });

        Save();
        message = "Akun berhasil dibuat.";
        return true;
    }

    public static bool Login(string username, string password, out string message)
    {
        username = username.Trim().ToLowerInvariant();
        var user = Users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && u.Password == password);

        if (user == null)
        {
            message = "Username atau password tidak sesuai.";
            return false;
        }

        if (!user.IsActive)
        {
            message = "Akun sedang nonaktif.";
            return false;
        }

        CurrentUser = user;
        message = "Login berhasil.";
        return true;
    }

    public static void Logout() => CurrentUser = null;

    public static IEnumerable<ScreeningRecord> MyRecords()
    {
        return CurrentUser == null
            ? Enumerable.Empty<ScreeningRecord>()
            : Records.Where(r => r.Username == CurrentUser.Username);
    }

    public static void AddRecord(ScreeningRecord record)
    {
        Records.Add(record);
        Save();
    }

    public static void Save()
    {
        LocalDatabaseService.Save(Users, Records);
    }

    public static string DatabaseInfo => LocalDatabaseService.GetDatabaseInfo();
}
