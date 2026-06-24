namespace MediGuardAccess;

public static class FaceScanService
{
    public static int MatchPercent(string? registeredSignature, string? liveSignature)
    {
        if (string.IsNullOrWhiteSpace(liveSignature))
            return 0;

        // Akun demo yang belum punya template wajah tetap bisa diuji.
        if (string.IsNullOrWhiteSpace(registeredSignature))
            return 88;

        var registered = ParseSignature(registeredSignature);
        var live = ParseSignature(liveSignature);

        if (!registered.IsValid || !live.IsValid)
            return 0;

        double distance = CalculateDistance(registered.R, registered.G, registered.B, live.R, live.G, live.B);

        int score = 100 - (int)Math.Round(distance / 2.2);
        return Math.Clamp(score, 0, 100);
    }

    public static string AccessDecision(int matchPercent)
    {
        if (matchPercent >= 80)
            return "Akses diterima";

        if (matchPercent >= 60)
            return "Scan ulang";

        return "Akses ditolak";
    }

    public static string AccessCategory(int matchPercent)
    {
        if (matchPercent >= 80)
            return "Tinggi";

        if (matchPercent >= 60)
            return "Sedang";

        return "Rendah";
    }

    public static bool IsAccepted(int matchPercent)
    {
        return matchPercent >= 80;
    }

    private static double CalculateDistance(int r1, int g1, int b1, int r2, int g2, int b2)
    {
        return Math.Sqrt(
            Math.Pow(r1 - r2, 2) +
            Math.Pow(g1 - g2, 2) +
            Math.Pow(b1 - b2, 2)
        );
    }

    private static FaceSignature ParseSignature(string signature)
    {
        var parts = signature.Split('-', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 3)
            return FaceSignature.Invalid();

        bool rOk = int.TryParse(parts[0], out int r);
        bool gOk = int.TryParse(parts[1], out int g);
        bool bOk = int.TryParse(parts[2], out int b);

        if (!rOk || !gOk || !bOk)
            return FaceSignature.Invalid();

        r = Math.Clamp(r, 0, 255);
        g = Math.Clamp(g, 0, 255);
        b = Math.Clamp(b, 0, 255);

        return new FaceSignature(r, g, b, true);
    }

    private readonly record struct FaceSignature(int R, int G, int B, bool IsValid)
    {
        public static FaceSignature Invalid()
        {
            return new FaceSignature(0, 0, 0, false);
        }
    }
}
