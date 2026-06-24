using Microsoft.Data.Sqlite;

namespace MediGuardAccess;

/// <summary>
/// Penyimpanan lokal SQLite untuk MediGuardAccess.
/// Salinan awal database disertakan di Resources/Raw lalu disalin ke AppDataDirectory
/// saat aplikasi pertama kali dijalankan. File .sqlite di folder proyek dapat langsung
/// dibuka memakai SQLite Viewer untuk melihat struktur dan data demo.
/// </summary>
public static class LocalDatabaseService
{
    private const string DatabaseFileName = "MediGuardAccess.sqlite";

    private static string DatabasePath => Path.Combine(FileSystem.AppDataDirectory, DatabaseFileName);

    public static AppDatabaseSnapshot Load()
    {
        try
        {
            EnsureDatabase();
            using var connection = OpenConnection();

            var snapshot = new AppDatabaseSnapshot();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT FullName, Username, Password, Role, FaceSignature, IsActive
                                        FROM Users ORDER BY Id;";
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    snapshot.Users.Add(new UserAccount
                    {
                        FullName = reader.GetString(0),
                        Username = reader.GetString(1),
                        Password = reader.GetString(2),
                        Role = Enum.TryParse<UserRole>(reader.GetString(3), out var role) ? role : UserRole.User,
                        FaceSignature = reader.IsDBNull(4) ? null : reader.GetString(4),
                        IsActive = reader.GetInt64(5) == 1
                    });
                }
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"SELECT Date, Username, Name, Age, Gender, ChiefComplaint,
                    Systolic, Diastolic, RecheckSystolic, RecheckDiastolic, BpTrendNote,
                    SpO2, Bpm, Rr, Temperature, CoughDays, SymptomScore, RiskScore,
                    Result, Recommendation, SymptomDetails, Reason, NnSummary
                    FROM ScreeningRecords ORDER BY Id;";
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    snapshot.Records.Add(new ScreeningRecord
                    {
                        Date = DateTime.TryParse(reader.GetString(0), out var date) ? date : DateTime.Now,
                        Username = reader.GetString(1), Name = reader.GetString(2), Age = reader.GetInt32(3),
                        Gender = reader.GetString(4), ChiefComplaint = reader.GetString(5),
                        Systolic = reader.GetInt32(6), Diastolic = reader.GetInt32(7),
                        RecheckSystolic = reader.GetInt32(8), RecheckDiastolic = reader.GetInt32(9),
                        BpTrendNote = reader.GetString(10), SpO2 = reader.GetInt32(11), Bpm = reader.GetInt32(12),
                        Rr = reader.GetInt32(13), Temperature = reader.GetDouble(14), CoughDays = reader.GetInt32(15),
                        SymptomScore = reader.GetInt32(16), RiskScore = reader.GetDouble(17), Result = reader.GetString(18),
                        Recommendation = reader.GetString(19), SymptomDetails = reader.GetString(20),
                        Reason = reader.GetString(21), NnSummary = reader.GetString(22)
                    });
                }
            }
            return snapshot;
        }
        catch
        {
            return new AppDatabaseSnapshot();
        }
    }

    public static void Save(IEnumerable<UserAccount> users, IEnumerable<ScreeningRecord> records)
    {
        try
        {
            EnsureDatabase();
            using var connection = OpenConnection();
            using var transaction = connection.BeginTransaction();

            Execute(connection, transaction, "DELETE FROM FaceEnrollments;");
            Execute(connection, transaction, "DELETE FROM ScreeningRecords;");
            Execute(connection, transaction, "DELETE FROM Users;");

            foreach (var user in users)
            {
                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"INSERT INTO Users (FullName, Username, Password, Role, FaceSignature, IsActive)
                                        VALUES ($fullName, $username, $password, $role, $signature, $isActive);";
                command.Parameters.AddWithValue("$fullName", user.FullName);
                command.Parameters.AddWithValue("$username", user.Username);
                command.Parameters.AddWithValue("$password", user.Password);
                command.Parameters.AddWithValue("$role", user.Role.ToString());
                command.Parameters.AddWithValue("$signature", (object?)user.FaceSignature ?? DBNull.Value);
                command.Parameters.AddWithValue("$isActive", user.IsActive ? 1 : 0);
                command.ExecuteNonQuery();

                if (!string.IsNullOrWhiteSpace(user.FaceSignature))
                {
                    using var faceCommand = connection.CreateCommand();
                    faceCommand.Transaction = transaction;
                    faceCommand.CommandText = @"INSERT INTO FaceEnrollments
                        (UserId, Role, FaceSignature, FacePhoto, PhotoMimeType, PhotoSource, CreatedAt, IsPrimary)
                        VALUES ((SELECT Id FROM Users WHERE Username = $username), $role, $signature,
                        NULL, 'image/png', 'Face signature captured by MediGuardAccess verification flow', $createdAt, 1);";
                    faceCommand.Parameters.AddWithValue("$username", user.Username);
                    faceCommand.Parameters.AddWithValue("$role", user.Role.ToString());
                    faceCommand.Parameters.AddWithValue("$signature", user.FaceSignature);
                    faceCommand.Parameters.AddWithValue("$createdAt", DateTime.Now.ToString("O"));
                    faceCommand.ExecuteNonQuery();
                }
            }

            foreach (var record in records)
            {
                using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = @"INSERT INTO ScreeningRecords
                (Date, Username, Name, Age, Gender, ChiefComplaint, Systolic, Diastolic, RecheckSystolic, RecheckDiastolic,
                 BpTrendNote, SpO2, Bpm, Rr, Temperature, CoughDays, SymptomScore, RiskScore, Result, Recommendation,
                 SymptomDetails, Reason, NnSummary)
                 VALUES ($date, $username, $name, $age, $gender, $complaint, $sys, $dia, $reSys, $reDia, $trend,
                 $spo2, $bpm, $rr, $temp, $cough, $symptoms, $risk, $result, $recommendation, $details, $reason, $nn);";
                command.Parameters.AddWithValue("$date", record.Date.ToString("O"));
                command.Parameters.AddWithValue("$username", record.Username); command.Parameters.AddWithValue("$name", record.Name);
                command.Parameters.AddWithValue("$age", record.Age); command.Parameters.AddWithValue("$gender", record.Gender);
                command.Parameters.AddWithValue("$complaint", record.ChiefComplaint); command.Parameters.AddWithValue("$sys", record.Systolic);
                command.Parameters.AddWithValue("$dia", record.Diastolic); command.Parameters.AddWithValue("$reSys", record.RecheckSystolic);
                command.Parameters.AddWithValue("$reDia", record.RecheckDiastolic); command.Parameters.AddWithValue("$trend", record.BpTrendNote);
                command.Parameters.AddWithValue("$spo2", record.SpO2); command.Parameters.AddWithValue("$bpm", record.Bpm);
                command.Parameters.AddWithValue("$rr", record.Rr); command.Parameters.AddWithValue("$temp", record.Temperature);
                command.Parameters.AddWithValue("$cough", record.CoughDays); command.Parameters.AddWithValue("$symptoms", record.SymptomScore);
                command.Parameters.AddWithValue("$risk", record.RiskScore); command.Parameters.AddWithValue("$result", record.Result);
                command.Parameters.AddWithValue("$recommendation", record.Recommendation); command.Parameters.AddWithValue("$details", record.SymptomDetails);
                command.Parameters.AddWithValue("$reason", record.Reason); command.Parameters.AddWithValue("$nn", record.NnSummary);
                command.ExecuteNonQuery();
            }
            transaction.Commit();
        }
        catch
        {
            // Kegagalan penyimpanan tidak menghentikan alur screening prototype.
        }
    }

    public static string GetDatabaseInfo() => $"Database SQLite lokal: {DatabasePath}";

    private static SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection($"Data Source={DatabasePath}");
        connection.Open();
        return connection;
    }

    private static void EnsureDatabase()
    {
        Directory.CreateDirectory(FileSystem.AppDataDirectory);
        if (File.Exists(DatabasePath)) return;

        using var connection = OpenConnection();
        Execute(connection, null, SchemaSql);
        Execute(connection, null, SeedSql);
    }

    private static void Execute(SqliteConnection connection, SqliteTransaction? transaction, string sql)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private const string SchemaSql = @"
CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    FullName TEXT NOT NULL,
    Username TEXT NOT NULL UNIQUE,
    Password TEXT NOT NULL,
    Role TEXT NOT NULL,
    FaceSignature TEXT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1
);
CREATE TABLE IF NOT EXISTS ScreeningRecords (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Date TEXT NOT NULL, Username TEXT NOT NULL, Name TEXT NOT NULL, Age INTEGER NOT NULL, Gender TEXT NOT NULL,
    ChiefComplaint TEXT NOT NULL, Systolic INTEGER NOT NULL, Diastolic INTEGER NOT NULL,
    RecheckSystolic INTEGER NOT NULL, RecheckDiastolic INTEGER NOT NULL, BpTrendNote TEXT NOT NULL,
    SpO2 INTEGER NOT NULL, Bpm INTEGER NOT NULL, Rr INTEGER NOT NULL, Temperature REAL NOT NULL,
    CoughDays INTEGER NOT NULL, SymptomScore INTEGER NOT NULL, RiskScore REAL NOT NULL, Result TEXT NOT NULL,
    Recommendation TEXT NOT NULL, SymptomDetails TEXT NOT NULL, Reason TEXT NOT NULL, NnSummary TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS FaceEnrollments (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId INTEGER NOT NULL,
    Role TEXT NOT NULL,
    FaceSignature TEXT NOT NULL,
    FacePhoto BLOB NULL,
    PhotoMimeType TEXT NOT NULL,
    PhotoSource TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    IsPrimary INTEGER NOT NULL DEFAULT 1,
    FOREIGN KEY(UserId) REFERENCES Users(Id)
);";

    // Data awal untuk demo dashboard dan pemeriksaan SQLite Viewer. Nilainya konsisten
    // dengan variabel skrining yang dipakai MediGuardAccess: tekanan darah, SpO2, nadi,
    // frekuensi napas, suhu, skor fuzzy, serta estimasi risiko Neural Network.
    private const string SeedSql = @"
INSERT INTO Users (FullName, Username, Password, Role, FaceSignature, IsActive) VALUES
('Admin MediGuardAccess','admin','admin123','Admin','120-160-180',1),
('User Demo','user','user123','User','132-154-176',1),
('Alya Pratama','alya','alya123','User','128-158-181',1),
('Bagas Setiawan','bagas','bagas123','User','126-151-177',1),
('Citra Lestari','citra','citra123','User','134-162-185',1);
INSERT INTO FaceEnrollments (UserId,Role,FaceSignature,FacePhoto,PhotoMimeType,PhotoSource,CreatedAt,IsPrimary)
SELECT Id,Role,FaceSignature,NULL,'image/png','Face enrollment - MediGuardAccess prototype','2026-06-24T21:48:00',1 FROM Users;
INSERT INTO ScreeningRecords (Date,Username,Name,Age,Gender,ChiefComplaint,Systolic,Diastolic,RecheckSystolic,RecheckDiastolic,BpTrendNote,SpO2,Bpm,Rr,Temperature,CoughDays,SymptomScore,RiskScore,Result,Recommendation,SymptomDetails,Reason,NnSummary) VALUES
('2026-06-24T08:10:00','user','User Demo',21,'Laki-laki','Kontrol rutin tanpa keluhan berat',118,76,116,74,'Smart BP Recheck: hasil ulang relatif stabil.',98,78,16,36.8,0,0,8.0,'Normal','Pemantauan mandiri dan pola hidup sehat.','Tidak ada red flag.','Tekanan darah dan vital dalam rentang aman.','CardioNet: risiko rendah, kelas Normal.'),
('2026-06-24T09:05:00','bagas','Bagas Setiawan',24,'Laki-laki','Sakit kepala ringan',138,88,136,86,'Smart BP Recheck: tekanan darah masih perlu dipantau.',97,90,19,37.0,2,2,34.0,'Waspada','Pantau tekanan darah dan konsultasi bila menetap.','Sakit kepala: +2','Tekanan darah meningkat ringan dengan keluhan pendukung.','CardioNet: risiko sedang, kelas Waspada.'),
('2026-06-24T09:40:00','citra','Citra Lestari',28,'Perempuan','Pusing dan berdebar',148,94,146,92,'Smart BP Recheck: tekanan darah tetap tinggi.',96,104,21,37.2,2,4,52.0,'Waspada','Observasi dan konsultasi ke fasilitas kesehatan.','Pusing: +2; berdebar: +2','Tekanan darah tinggi derajat awal disertai gejala.','CardioNet: risiko sedang, kelas Waspada.'),
('2026-06-24T11:25:00','bagas','Bagas Setiawan',24,'Laki-laki','Nyeri kepala dan pandangan kabur',162,102,160,100,'Smart BP Recheck: tekanan darah masih sangat tinggi.',95,108,23,37.1,1,7,78.0,'Berisiko','Segera evaluasi ke fasilitas kesehatan hari ini.','Nyeri kepala berat: +4; pandangan kabur: +3','Tekanan darah sangat tinggi dengan gejala prioritas.','CardioNet: risiko tinggi, kelas Berisiko.'),
('2026-06-24T12:00:00','citra','Citra Lestari',28,'Perempuan','Sesak napas dan nyeri dada',170,108,168,106,'Smart BP Recheck: hasil ulang tetap tinggi.',93,116,26,37.8,1,11,91.0,'Berisiko','Segera ke IGD atau fasilitas kesehatan terdekat.','Sesak berat: +4; nyeri dada: +4; lemah: +3','Kombinasi red flag dan tekanan darah sangat tinggi.','CardioNet: risiko sangat tinggi, kelas Berisiko.');
";

}
