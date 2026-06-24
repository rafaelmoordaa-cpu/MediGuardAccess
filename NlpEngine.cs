namespace MediGuardAccess;

public static class NlpEngine
{
    public static string Reply(string text)
    {
        string raw = text ?? "";
        string s = raw.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(s))
            return "Silakan tuliskan gejala, hasil tekanan darah, atau pertanyaan kesehatan. MediGuard AI bisa membantu menjelaskan hipertensi, red flag kardiovaskular, dan keluhan umum secara aman.";

        if (Has(s, "halo", "hai", "pagi", "siang", "malam", "assalamualaikum"))
            return "Halo, aku MediGuard AI. Aku bisa membantu menjelaskan hasil skrining hipertensi, tekanan darah, nyeri dada, pusing, sesak, atau pertanyaan kesehatan umum dengan bahasa yang aman dan mudah dipahami.";

        if (IsRedFlag(s))
            return "Keluhan yang kamu sebutkan termasuk tanda yang perlu diperhatikan. MediGuardAccess tidak dapat memastikan diagnosis, tetapi nyeri dada berat, sesak berat, pingsan, kelemahan mendadak, muntah darah, BAB hitam, atau penurunan kesadaran sebaiknya segera diperiksa ke fasilitas kesehatan.";

        if (Has(s, "tekanan darah", "hipertensi", "tensi", "darah tinggi", "sistolik", "diastolik"))
            return "Hipertensi berarti tekanan darah cenderung tinggi. Secara umum, angka 140/90 mmHg atau lebih perlu dipantau dan dikonsultasikan bila berulang. Di MediGuardAccess, gunakan Smart BP Recheck: istirahat sekitar 5 menit, ukur ulang, lalu lihat apakah hasil masih tinggi atau menurun.";

        if (Has(s, "hasil saya", "hasil skrining", "normal", "waspada", "berisiko", "kenapa hasil"))
            return "Hasil MediGuardAccess dibaca dari gabungan tekanan darah, denyut nadi, SpO2, frekuensi napas, suhu, durasi keluhan, dan red flag. Normal berarti relatif aman untuk pemantauan, Waspada berarti perlu dipantau/diulang, dan Berisiko berarti sebaiknya tidak menunda pemeriksaan.";

        if (Has(s, "nyeri dada", "dada sakit", "dada berat", "jantung"))
            return "Nyeri dada perlu diperhatikan, terutama bila terasa berat/menekan, menjalar ke lengan/rahang/punggung, disertai sesak, keringat dingin, mual berat, pusing, atau hampir pingsan. Jika gejalanya seperti itu, jangan menunggu hasil aplikasi dan segera cari bantuan medis.";

        if (Has(s, "pusing", "lemas", "sakit kepala"))
            return "Pusing atau lemas bisa disebabkan kurang tidur, dehidrasi, kurang makan, tekanan darah naik/turun, anemia, stres, atau infeksi. Coba pantau tekanan darah, cukup cairan, istirahat, dan catat durasinya. Bila disertai pingsan, kelemahan satu sisi, bicara pelo, nyeri dada, atau sesak, segera periksa.";

        if (Has(s, "sesak", "napas", "nafas", "sulit bernapas"))
            return "Sesak napas termasuk keluhan yang perlu dipantau serius. Jika memungkinkan, cek SpO2 dan frekuensi napas. Bila sesak berat, bibir kebiruan, nyeri dada, keringat dingin, atau SpO2 rendah, sebaiknya segera ke fasilitas kesehatan.";

        if (Has(s, "demam", "panas", "suhu"))
            return "Demam dapat terjadi karena infeksi atau peradangan. Catat suhu, durasi, dan gejala lain seperti sesak, nyeri dada, lemas berat, muntah/diare, atau penurunan kesadaran. Bila demam tinggi menetap atau disertai tanda bahaya, sebaiknya periksa.";

        if (Has(s, "mual", "muntah", "diare", "sakit perut", "nyeri perut"))
            return "Keluhan mual, muntah, diare, atau sakit perut bisa berkaitan dengan gangguan pencernaan, infeksi, makanan, stres, atau kondisi lain. Pantau cairan tubuh, frekuensi muntah/diare, dan nyeri. Jika muntah/diare terus-menerus, nyeri perut hebat, BAB hitam, muntah darah, atau lemas berat, segera periksa.";

        if (Has(s, "apa yang harus", "harus apa", "gimana", "bagaimana", "saran", "solusi"))
            return "Langkah aman awal: hentikan aktivitas berat, duduk/istirahat, catat keluhan dan durasinya, ukur tekanan darah bila tersedia, lalu isi skrining MediGuardAccess. Jika ada red flag seperti nyeri dada berat, sesak berat, pingsan, atau tekanan darah sangat tinggi, segera cari bantuan medis.";

        if (LooksNonMedical(s))
            return "Aku difokuskan untuk membantu pertanyaan kesehatan, hasil skrining, gejala, dan edukasi medis sederhana. Coba tuliskan keluhan tubuh atau hasil pengukuran yang ingin kamu pahami.";

        return "Keluhan tersebut bisa memiliki banyak penyebab dan tidak bisa dipastikan hanya dari chat. Pantau durasi, intensitas, gejala penyerta, dan tanda bahaya. Untuk langkah awal, istirahat, cukup cairan, catat keluhan, dan gunakan skrining MediGuardAccess bila terkait tekanan darah atau kondisi tubuh umum. Jika memburuk atau ada red flag, segera periksa ke tenaga kesehatan.";
    }

    private static bool IsRedFlag(string s)
    {
        return Has(s,
            "nyeri dada berat", "dada berat", "sesak berat", "sulit bernapas berat", "pingsan", "hampir pingsan", "kejang", "bingung", "tidak sadar", "kesadaran menurun", "muntah darah", "bab hitam", "lemah satu sisi", "bicara pelo", "nyeri perut hebat", "tekanan darah 180", "tensi 180", "keringat dingin");
    }

    private static bool LooksNonMedical(string s)
    {
        return Has(s, "puisi", "lagu", "film", "game", "coding", "matematika", "fisika", "translate", "terjemah", "saham", "bola");
    }

    private static bool Has(string source, params string[] keys)
    {
        foreach (var key in keys)
            if (source.Contains(key)) return true;
        return false;
    }
}
