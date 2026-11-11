using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using RaceDay.MVC.Models;

namespace RaceDay.MVC.Services;

public class RaceDayApiClient
{
    private readonly IHttpClientFactory _factory;
    private readonly IHttpContextAccessor _accessor;
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web) { PropertyNameCaseInsensitive = true };

    public RaceDayApiClient(IHttpClientFactory factory, IHttpContextAccessor accessor)
    { _factory = factory; _accessor = accessor; }

    private HttpClient Client => _factory.CreateClient("RaceDayApi");
    private ISession Session => _accessor.HttpContext!.Session;
    private void AddApiCookie(HttpRequestMessage request)
    {
        var cookie = Session.GetString("ApiSessionCookie");
        if (!string.IsNullOrWhiteSpace(cookie)) request.Headers.TryAddWithoutValidation("Cookie", cookie);
    }
    private async Task<HttpResponseMessage> Send(HttpMethod method, string path, object? body = null)
    {
        var req = new HttpRequestMessage(method, path);
        AddApiCookie(req);
        if (body != null)
            req.Content = new StringContent(JsonSerializer.Serialize(body, _json), Encoding.UTF8, "application/json");
        return await Client.SendAsync(req);
    }
    public async Task<(bool Ok, UserResponse? User, string Error)> Login(LoginVm vm)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, "api/auth/login")
        { Content = new StringContent(JsonSerializer.Serialize(new { vm.Email, vm.Password }), Encoding.UTF8, "application/json") };
        using var response = await Client.SendAsync(req);
        if (!response.IsSuccessStatusCode) return (false, null, await Error(response));
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
            return (false, null, "The API did not return a session cookie. Please check the API session configuration.");
        var sessionCookie = cookies.FirstOrDefault(x =>
            x.TrimStart().StartsWith(".RaceDay.Session=", StringComparison.OrdinalIgnoreCase));
        if (sessionCookie is null)
            return (false, null, "The API login succeeded but did not establish a session.");
        Session.SetString("ApiSessionCookie", sessionCookie.Trim().Split(';')[0]);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>(_json);
        if (user == null) return (false, null, "The API returned an empty login response.");
        Session.SetString("RaceDayUser", JsonSerializer.Serialize(user, _json));
        return (true, user, "");
    }
    public async Task<(bool Ok, string Error)> Register(RegisterVm vm)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, "api/auth/register")
        { Content = new StringContent(JsonSerializer.Serialize(vm, _json), Encoding.UTF8, "application/json") };
        using var response = await Client.SendAsync(req);
        return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "" : await Error(response));
    }
    public UserResponse? CurrentUser()
    {
        var json = Session.GetString("RaceDayUser");
        return string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<UserResponse>(json, _json);
    }
    public async Task Logout()
    {
        try { using var r = await Send(HttpMethod.Post, "api/auth/logout"); } catch { }
        Session.Clear();
    }
    public async Task<(bool Ok, T? Data, string Error)> Get<T>(string path)
    {
        using var r = await Send(HttpMethod.Get, path);
        if (!r.IsSuccessStatusCode) return (false, default, await Error(r));
        return (true, await r.Content.ReadFromJsonAsync<T>(_json), "");
    }
    public async Task<(bool Ok, string Error)> Post(string path, object body)
    {
        using var r = await Send(HttpMethod.Post, path, body);
        return (r.IsSuccessStatusCode, r.IsSuccessStatusCode ? "" : await Error(r));
    }
    public async Task<(bool Ok, string Error)> Put(string path, object body)
    {
        using var r = await Send(HttpMethod.Put, path, body);
        return (r.IsSuccessStatusCode, r.IsSuccessStatusCode ? "" : await Error(r));
    }
    public async Task<(bool Ok, string Error)> Delete(string path)
    {
        using var r = await Send(HttpMethod.Delete, path);
        return (r.IsSuccessStatusCode, r.IsSuccessStatusCode ? "" : await Error(r));
    }
    public async Task<(bool Ok, string? Url, string Error)> UploadImage(string path, IFormFile file)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, path);
        AddApiCookie(req);
        using var content = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        content.Add(fileContent, "image", Path.GetFileName(file.FileName));
        req.Content = content;
        using var response = await Client.SendAsync(req);
        if (!response.IsSuccessStatusCode) return (false, null, await Error(response));
        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var url = doc.RootElement.TryGetProperty("imageUrl", out var p) ? p.GetString() :
                  doc.RootElement.TryGetProperty("url", out var q) ? q.GetString() : null;
        return (true, url, "");
    }
    private static async Task<string> Error(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("message", out var msg)) return msg.GetString() ?? response.ReasonPhrase ?? "API request failed.";
            if (doc.RootElement.TryGetProperty("title", out var title)) return title.GetString() ?? "The request was not accepted.";
        }
        catch { }
        return string.IsNullOrWhiteSpace(body) ? $"API request failed ({(int)response.StatusCode})." : body;
    }
}
