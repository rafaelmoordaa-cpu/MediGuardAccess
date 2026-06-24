using System.Collections.Generic;

namespace MediGuardAccess;

public static class FuzzyEngine
{
    public static FuzzyResult Analyze(
        int systolic,
        int diastolic,
        int recheckSystolic,
        int recheckDiastolic,
        int bpm,
        int rr,
        int spo2,
        double temp,
        int complaintDays,
        bool historyHypertension,
        bool diabetesOrHeartDisease,
        bool severeChestPain,
        bool radiatingPain,
        bool severeShortness,
        bool coldSweat,
        bool faintingOrSevereWeakness,
        bool smokerStressOrObesity
    )
    {
        int indicatorScore = 0;
        var details = new List<string>();

        void Add(bool flag, string label, int score)
        {
            if (!flag) return;
            indicatorScore += score;
            details.Add($"{label}: +{score}");
        }

        Add(historyHypertension, "Riwayat hipertensi", 2);
        Add(diabetesOrHeartDisease, "Riwayat diabetes/penyakit jantung", 2);
        Add(severeChestPain, "Nyeri dada berat/menekan", 5);
        Add(radiatingPain, "Nyeri menjalar ke lengan/rahang/punggung", 4);
        Add(severeShortness, "Sesak napas berat", 4);
        Add(coldSweat, "Keringat dingin/mual berat", 3);
        Add(faintingOrSevereWeakness, "Pingsan/hampir pingsan atau lemah berat", 4);
        Add(smokerStressOrObesity, "Faktor risiko gaya hidup", 1);

        bool hasRecheck = recheckSystolic > 0 && recheckDiastolic > 0;
        int finalSystolic = hasRecheck ? recheckSystolic : systolic;
        int finalDiastolic = hasRecheck ? recheckDiastolic : diastolic;

        int bpLevel = LevelBp(finalSystolic, finalDiastolic);
        int bpmLevel = LevelBpm(bpm);
        int rrLevel = LevelRr(rr);
        int spo2Level = LevelSpO2(spo2);
        int tempLevel = LevelTemp(temp);
        int durationLevel = LevelComplaintDuration(complaintDays);
        int indicatorLevel = LevelSymptoms(indicatorScore);

        int total = (bpLevel * 2) + bpmLevel + rrLevel + spo2Level + tempLevel + durationLevel + indicatorLevel;
        double risk = Math.Round((total / 16.0) * 100, 1);

        bool hypertensiveUrgency = finalSystolic >= 180 || finalDiastolic >= 120;
        bool cardiovascularRedFlag = severeChestPain || radiatingPain || severeShortness || faintingOrSevereWeakness;
        bool unstableVital = spo2 <= 92 || rr >= 30 || rr <= 8 || bpm < 45 || bpm > 130;

        int riskCount = CountRisk(bpLevel, bpmLevel, rrLevel, spo2Level, tempLevel, durationLevel, indicatorLevel);
        int warningCount = CountWarning(bpLevel, bpmLevel, rrLevel, spo2Level, tempLevel, durationLevel, indicatorLevel);

        string result;
        if (hypertensiveUrgency || cardiovascularRedFlag || unstableVital || riskCount >= 2 || total >= 9)
            result = "Berisiko";
        else if (bpLevel >= 1 || riskCount == 1 || warningCount >= 2 || total >= 4)
            result = "Waspada";
        else
            result = "Normal";

        string bpTrend = BuildBpTrend(systolic, diastolic, recheckSystolic, recheckDiastolic);
        string rec = BuildRecommendation(result, hypertensiveUrgency, cardiovascularRedFlag, hasRecheck);

        string detail = details.Count == 0
            ? "Tidak ada red flag kardiovaskular atau faktor risiko tambahan yang dipilih."
            : string.Join("\n", details);

        string reason = BuildReason(
            result,
            bpLevel,
            bpmLevel,
            rrLevel,
            spo2Level,
            tempLevel,
            durationLevel,
            indicatorLevel,
            hypertensiveUrgency,
            cardiovascularRedFlag,
            hasRecheck
        );

        return new FuzzyResult
        {
            SymptomScore = indicatorScore,
            RiskScore = risk,
            Result = result,
            Recommendation = rec,
            SymptomDetails = detail,
            Reason = reason,
            BpTrendNote = bpTrend
        };
    }

    public static int LevelBp(int systolic, int diastolic)
    {
        if (systolic <= 0 || diastolic <= 0) return 0;
        if (systolic >= 160 || diastolic >= 100) return 2;
        if (systolic >= 140 || diastolic >= 90) return 1;
        if (systolic >= 130 || diastolic >= 85) return 1;
        return 0;
    }

    public static int LevelSpO2(int v) => v <= 0 ? 0 : v <= 92 ? 2 : v <= 94 ? 1 : 0;
    public static int LevelBpm(int v) => v <= 0 ? 0 : (v < 50 || v > 110) ? 2 : ((v >= 50 && v <= 59) || (v >= 101 && v <= 110) ? 1 : 0);
    public static int LevelRr(int v) => v <= 0 ? 0 : (v < 10 || v > 24) ? 2 : ((v >= 10 && v <= 11) || (v >= 21 && v <= 24) ? 1 : 0);
    public static int LevelTemp(double v) => v <= 0 ? 0 : (v < 36.0 || v >= 38.5) ? 2 : (v >= 37.5 ? 1 : 0);
    public static int LevelCough(int v) => LevelComplaintDuration(v);
    public static int LevelComplaintDuration(int v) => v >= 7 ? 2 : v >= 3 ? 1 : 0;
    public static int LevelSymptoms(int v) => v >= 7 ? 2 : v >= 3 ? 1 : 0;

    public static string LevelText(int level) => level switch
    {
        0 => "Normal",
        1 => "Waspada",
        _ => "Berisiko"
    };

    public static string BpReference(int systolic, int diastolic) =>
        $"{LevelText(LevelBp(systolic, diastolic))} • acuan: <130/<85 relatif aman, 130-139/85-89 pantau, ≥140/90 perlu perhatian, ≥160/100 berisiko lebih tinggi";

    public static string SpO2Reference(int v) =>
        $"{LevelText(LevelSpO2(v))} • acuan: 95-100 normal, 93-94 waspada, <=92 berisiko";

    public static string BpmReference(int v) =>
        $"{LevelText(LevelBpm(v))} • acuan: 60-100 normal, 50-59 / 101-110 waspada, <50 atau >110 berisiko";

    public static string RrReference(int v) =>
        $"{LevelText(LevelRr(v))} • acuan: 12-20 normal, 10-11 / 21-24 waspada, <10 atau >24 berisiko";

    public static string TempReference(double v) =>
        $"{LevelText(LevelTemp(v))} • acuan: 36,0-37,4 normal, 37,5-38,4 waspada, <36,0 atau >=38,5 berisiko";

    public static string CoughReference(int v) =>
        $"{LevelText(LevelComplaintDuration(v))} • acuan keluhan: 0-2 hari normal, 3-6 hari waspada, >=7 hari perlu perhatian";

    public static string SymptomReference(int v) =>
        $"{LevelText(LevelSymptoms(v))} • acuan: skor 0-2 normal, 3-6 waspada, >=7 berisiko";

    private static string BuildBpTrend(int sys1, int dia1, int sys2, int dia2)
    {
        if (sys2 <= 0 || dia2 <= 0)
            return "Smart BP Recheck belum diisi. Jika tekanan darah awal tinggi, istirahat 5 menit lalu isi pengukuran ulang agar pembacaan lebih stabil.";

        int deltaSys = sys2 - sys1;
        int deltaDia = dia2 - dia1;

        if (Math.Abs(deltaSys) <= 5 && Math.Abs(deltaDia) <= 5)
            return $"Smart BP Recheck: hasil ulang relatif stabil ({sys1}/{dia1} → {sys2}/{dia2} mmHg).";

        if (deltaSys < -5 || deltaDia < -5)
            return $"Smart BP Recheck: hasil ulang menurun setelah jeda ({sys1}/{dia1} → {sys2}/{dia2} mmHg). Tetap pantau bila masih ≥140/90.";

        return $"Smart BP Recheck: hasil ulang meningkat ({sys1}/{dia1} → {sys2}/{dia2} mmHg). Hindari aktivitas berat dan pertimbangkan konsultasi bila tetap tinggi.";
    }

    private static string BuildRecommendation(string result, bool hypertensiveUrgency, bool cardiovascularRedFlag, bool hasRecheck)
    {
        if (hypertensiveUrgency || cardiovascularRedFlag)
            return "Terdapat tanda prioritas kardiovaskular atau tekanan darah sangat tinggi. Jangan menunda pemeriksaan. Segera cari bantuan medis, terutama bila ada nyeri dada berat, sesak berat, pingsan, nyeri menjalar, atau keringat dingin.";

        return result switch
        {
            "Normal" =>
                "Tekanan darah dan parameter pendukung tampak relatif aman untuk pemantauan mandiri. Pertahankan pola hidup sehat, batasi garam berlebih, istirahat cukup, dan ulangi skrining bila muncul keluhan baru.",
            "Waspada" =>
                (hasRecheck
                    ? "Hasil menunjukkan perlunya pemantauan tekanan darah dan faktor risiko. Catat hasil pengukuran, ulangi pengukuran di waktu berbeda, dan pertimbangkan konsultasi bila nilai tetap ≥140/90 atau keluhan menetap."
                    : "Hasil menunjukkan perlunya pemantauan. Gunakan fitur Smart BP Recheck: istirahat 5 menit, lalu masukkan pengukuran ulang. Konsultasikan bila tekanan darah tetap tinggi atau keluhan memburuk."),
            _ =>
                "Terdapat tanda prioritas. Segera lakukan evaluasi ke fasilitas kesehatan, terutama bila tekanan darah sangat tinggi, nyeri dada berat, sesak, pingsan, atau kondisi memburuk."
        };
    }

    private static string BuildReason(
        string result,
        int bpLevel,
        int bpmLevel,
        int rrLevel,
        int spo2Level,
        int tempLevel,
        int durationLevel,
        int indicatorLevel,
        bool hypertensiveUrgency,
        bool cardiovascularRedFlag,
        bool hasRecheck
    )
    {
        var reasons = new List<string>();

        if (bpLevel > 0) reasons.Add($"tekanan darah {LevelText(bpLevel)}");
        if (bpmLevel > 0) reasons.Add($"denyut nadi {LevelText(bpmLevel)}");
        if (rrLevel > 0) reasons.Add($"frekuensi napas {LevelText(rrLevel)}");
        if (spo2Level > 0) reasons.Add($"SpO2 {LevelText(spo2Level)}");
        if (tempLevel > 0) reasons.Add($"suhu tubuh {LevelText(tempLevel)}");
        if (durationLevel > 0) reasons.Add($"durasi keluhan {LevelText(durationLevel)}");
        if (indicatorLevel > 0) reasons.Add($"red flag/faktor risiko {LevelText(indicatorLevel)}");
        if (hypertensiveUrgency) reasons.Add("tekanan darah berada pada zona sangat tinggi");
        if (cardiovascularRedFlag) reasons.Add("ada red flag kardiovaskular");
        if (hasRecheck) reasons.Add("fitur Smart BP Recheck sudah digunakan");

        if (reasons.Count == 0)
            return "Output Normal karena tekanan darah, parameter vital, dan red flag kardiovaskular berada pada rentang aman untuk skrining awal.";

        return $"Output {result} karena " + string.Join(", ", reasons) + ".";
    }

    private static int CountRisk(params int[] values)
    {
        int count = 0;
        foreach (var v in values)
            if (v == 2) count++;
        return count;
    }

    private static int CountWarning(params int[] values)
    {
        int count = 0;
        foreach (var v in values)
            if (v == 1) count++;
        return count;
    }
}
