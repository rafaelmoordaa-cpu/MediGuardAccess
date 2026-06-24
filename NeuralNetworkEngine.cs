namespace MediGuardAccess;

public static class NeuralNetworkEngine
{
    public static string Simulate(ScreeningRecord record)
    {
        int finalSys = record.RecheckSystolic > 0 ? record.RecheckSystolic : record.Systolic;
        int finalDia = record.RecheckDiastolic > 0 ? record.RecheckDiastolic : record.Diastolic;

        double bpRisk = Clamp(((finalSys - 120) / 70.0) + ((finalDia - 80) / 45.0));
        double pulseRisk = Clamp(Math.Abs(record.Bpm - 80) / 80.0);
        double oxygenRisk = Clamp((95 - record.SpO2) / 10.0);
        double rrRisk = Clamp(Math.Abs(record.Rr - 16) / 20.0);
        double tempRisk = Clamp(Math.Abs(record.Temperature - 37.0) / 3.0);
        double redFlagRisk = Clamp(record.SymptomScore / 12.0);

        double z =
            -1.35 +
            (2.35 * bpRisk) +
            (0.85 * pulseRisk) +
            (1.20 * oxygenRisk) +
            (0.80 * rrRisk) +
            (0.55 * tempRisk) +
            (2.10 * redFlagRisk);

        double riskProbability = Sigmoid(z);
        string label = riskProbability switch
        {
            < 0.34 => "Normal",
            < 0.67 => "Waspada",
            _ => "Berisiko"
        };

        return
            $"Simulasi DigestNet/CardioNet feed-forward: kecenderungan risiko {riskProbability:P1}, kelas estimasi {label}. " +
            "Lapisan ini membantu menjelaskan kombinasi tekanan darah, tanda vital, dan red flag; bukan diagnosis medis final.";
    }

    private static double Sigmoid(double x) => 1.0 / (1.0 + Math.Exp(-x));

    private static double Clamp(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            return 0;
        return Math.Max(0, Math.Min(1, value));
    }
}
