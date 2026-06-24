using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MediGuardAccess;

public record AiChatMessage(string Role, string Content);

public static class GroqService
{
    // Isi dengan API key Groq jika ingin mengaktifkan AI cloud.
    // Jika dibiarkan placeholder, sistem otomatis memakai NLP lokal tanpa error mengganggu.
    private const string GroqApiKey = "";

    private const string EmptyApiKeyText = "ISI_API_KEY_GROQ_LU_DI_SINI";
    private const string Model = "llama-3.1-8b-instant";

    private static readonly HttpClient Client = new()
    {
        Timeout = TimeSpan.FromSeconds(25)
    };

    public static async Task<string> AskAsync(List<AiChatMessage> history)
    {
        if (history.Count == 0)
            return "Silakan tuliskan keluhan atau pertanyaan terlebih dahulu.";

        string latestUserMessage = history
            .LastOrDefault(x => x.Role == "user")
            ?.Content ?? "";

        string apiKey = GetApiKey();

        // Jika API key belum diatur, langsung pakai mode lokal agar UI tetap mulus.
        if (!HasValidApiKey(apiKey))
            return BuildLocalModeReply(latestUserMessage, "Mode lokal aktif");

        try
        {
            var messages = BuildMessages(history);

            var body = new
            {
                model = Model,
                messages,
                temperature = 0.75,
                max_tokens = 650,
                top_p = 0.95,
                stream = false
            };

            string json = JsonSerializer.Serialize(body);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.groq.com/openai/v1/chat/completions"
            );

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await Client.SendAsync(request);
            string responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                if ((int)response.StatusCode is 401 or 403)
                    return BuildLocalModeReply(latestUserMessage, "Koneksi cloud AI belum berhasil dipakai, mode lokal sementara digunakan");

                return BuildLocalModeReply(
                    latestUserMessage,
                    $"Layanan cloud AI sedang tidak tersedia ({(int)response.StatusCode})"
                );
            }

            string? answer = ExtractAnswer(responseJson);

            if (string.IsNullOrWhiteSpace(answer))
                return BuildLocalModeReply(latestUserMessage, "Jawaban cloud AI kosong, mode lokal digunakan");

            return Clean(answer);
        }
        catch (TaskCanceledException)
        {
            return BuildLocalModeReply(latestUserMessage, "Koneksi AI melampaui batas waktu, mode lokal digunakan");
        }
        catch
        {
            return BuildLocalModeReply(latestUserMessage, "Terjadi kendala pada cloud AI, mode lokal digunakan");
        }
    }

    private static object[] BuildMessages(List<AiChatMessage> history)
    {
        var limitedHistory = history
            .TakeLast(12)
            .Select(x => new
            {
                role = x.Role,
                content = x.Content
            })
            .ToList();

        var messages = new List<object>
        {
            new
            {
                role = "system",
                content =
                    "Kamu adalah MediGuard AI Assistant untuk aplikasi MediGuardAccess. " +
                    "Tugasmu membantu pengguna memahami keluhan awal, arti hasil screening, dan istilah kesehatan dengan bahasa Indonesia yang ramah, ringan, dan mudah dipahami orang awam. " +
                    "Fokus utamamu adalah hipertensi, tekanan darah, red flag kardiovaskular, hasil screening MediGuardAccess, dan pertanyaan kesehatan umum yang aman. " +
                    "Jawabanmu harus singkat, jelas, tidak menghakimi, dan tidak terlalu teknis. " +
                    "Untuk pertanyaan medis, jangan memberikan diagnosis final dan jangan menggantikan dokter. " +
                    "Jika ada tanda bahaya seperti nyeri dada berat, sesak berat, nyeri menjalar, keringat dingin, pingsan, tekanan darah sangat tinggi, muntah darah, BAB hitam, kejang, atau penurunan kesadaran, sarankan pengguna segera ke fasilitas kesehatan. " +
                    "Jika pengguna bertanya hal di luar konteks kesehatan, arahkan kembali dengan sopan bahwa kamu difokuskan untuk edukasi kesehatan awal dan screening. " +
                    "Jika relevan, sarankan pengguna mengisi form screening MediGuardAccess. " +
                    "Gunakan maksimal 6 kalimat dan prioritaskan kejelasan untuk orang awam."
            }
        };

        messages.AddRange(limitedHistory);
        return messages.ToArray();
    }

    private static string GetApiKey()
    {
        string? envKey = Environment.GetEnvironmentVariable("GERDIA_AI_API_KEY");
        if (!string.IsNullOrWhiteSpace(envKey))
            return envKey.Trim();

        envKey = Environment.GetEnvironmentVariable("GROQ_API_KEY");
        if (!string.IsNullOrWhiteSpace(envKey))
            return envKey.Trim();

        return GroqApiKey.Trim();
    }

    private static bool HasValidApiKey(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return false;

        if (apiKey == EmptyApiKeyText)
            return false;

        return apiKey.StartsWith("gsk_");
    }

    private static string BuildLocalModeReply(string latestUserMessage, string status)
    {
        return NlpEngine.Reply(latestUserMessage);
    }

    private static string? ExtractAnswer(string responseJson)
    {
        using var document = JsonDocument.Parse(responseJson);

        if (!document.RootElement.TryGetProperty("choices", out var choices))
            return null;

        if (choices.GetArrayLength() == 0)
            return null;

        var firstChoice = choices[0];

        if (!firstChoice.TryGetProperty("message", out var message))
            return null;

        if (!message.TryGetProperty("content", out var content))
            return null;

        return content.GetString();
    }

    private static string Clean(string text)
    {
        return text
            .Replace("**", "")
            .Replace("*", "")
            .Trim();
    }
}
