// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

Console.WriteLine("--------------------------------------");

try
{

    IHttpClientFactory httpClientFactory;

    ////var cliente = httpClientFactory.CreateClient("client").AddHttpHandler("admin", "p");
    //var handler = new HttpClientHandler()
    //{
    //    Credentials = new NetworkCredential("admin", "*a123456")
    //};
    //var httpClient = new HttpClient();

    ////httpClient.AddHttpHandler("admin", "*a123456");

    //var response = await httpClient.GetAsync("http://my.url");
    //response.EnsureSuccessStatusCode();

    var serviceCollection = new ServiceCollection();
    ApiExtensions.ConfigureServices(serviceCollection);
    var service = serviceCollection.BuildServiceProvider();
    httpClientFactory = service.GetRequiredService<IHttpClientFactory>();





    var client = httpClientFactory.CreateClient("Biometrico");
    client.BaseAddress = new Uri("http://192.168.2.116");
    var request = new HttpRequestMessage(HttpMethod.Get, "/ISAPI/AccessControl/AcsCfg?format=json");
    request.Headers.Add("Accept", "*/*");
    request.Headers.Add("User-Agent", "HttpClientDigestAuthTester");
    request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
    request.Headers.Add("Connection", "keep-alive");

    var response = await client.SendWithDigestAuthAsync(request, HttpCompletionOption.ResponseContentRead, "admin", "*a123456");

    if (response.StatusCode != HttpStatusCode.OK)
    {
        return;
    }

    var result = response.Content.ReadAsStringAsync();

}
catch (Exception)
{
}

public static class ApiExtensions
{
    public static HttpClient AddHttpHandler(this HttpClient httpClient, string user, string pass)
    {
        var original_client = httpClient;

        try
        {

            return original_client;


        }
        catch (Exception ex)
        {
            return original_client;
        }
    }

    public static void ConfigureServices(ServiceCollection services)
    {
        services.AddHttpClient("Biometrico", c =>
        {
            c.Timeout = TimeSpan.FromSeconds(300);
            c.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        });
    }
}

internal class DigestAuthHeader
{
    public DigestAuthHeader(string realm, string username, string password, string nonce, string qualityOfProtection,
        int nonceCount, string clientNonce, string opaque)
    {
        Realm = realm;
        Username = username;
        Password = password;
        Nonce = nonce;
        QualityOfProtection = qualityOfProtection;
        NonceCount = nonceCount;
        ClientNonce = clientNonce;
        Opaque = opaque;
    }

    public string Realm { get; }
    public string Username { get; }
    public string Password { get; }
    public string Nonce { get; }
    public string QualityOfProtection { get; }
    public int NonceCount { get; }
    public string ClientNonce { get; }
    public string Opaque { get; }
}

public static class HttpClientExtensions
{
    /// <summary>
    /// Executes a secondary call to <paramref name="serviceDir"/> if the call to <paramref name="request"/> fails due to a 401 error, using Digest authentication
    /// </summary>
    /// <param name="client">HTTPClient object, this should have the BaseAddress property set</param>
    /// <param name="serviceDir">Relative path to the resource from the host, e.g /services/service1?param=value</param>
    /// <param name="request"></param>
    /// <param name="httpCompletionOption"></param>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static async Task<HttpResponseMessage> SendWithDigestAuthAsync(this HttpClient client,
        HttpRequestMessage request, HttpCompletionOption httpCompletionOption,
        string username, string password)
    {
        var newRequest = CloneBeforeContentSet(request);
        var response = await client.SendAsync(request, httpCompletionOption);
        if (response.StatusCode != System.Net.HttpStatusCode.Unauthorized) return response;

        var wwwAuthenticateHeaderValue = response.Headers.GetValues("WWW-Authenticate").FirstOrDefault();

        var realm = GetChallengeValueFromHeader("realm", wwwAuthenticateHeaderValue);
        var nonce = GetChallengeValueFromHeader("nonce", wwwAuthenticateHeaderValue);
        var qop = GetChallengeValueFromHeader("qop", wwwAuthenticateHeaderValue);

        // Must be fresh on every request, so low chance of same client nonce here by just using a random number.
        var clientNonce = new Random().Next(123400, 9999999).ToString();
        var opaque = GetChallengeValueFromHeader("opaque", wwwAuthenticateHeaderValue);

        // The nonce count 'nc' doesn't really matter, so we just set this to one. Why we always sending two requests per 1 request
        var digestHeader = new DigestAuthHeader(realm, username, password, nonce, qop, nonceCount: 1, clientNonce, opaque);
        var digestRequestHeader = GetDigestHeader(digestHeader, newRequest.RequestUri.ToString(), request.Method);

        newRequest.Headers.Add("Authorization", digestRequestHeader);
        var authRes = await client.SendAsync(newRequest, httpCompletionOption);
        return authRes;
    }

    private static string GetChallengeValueFromHeader(string challengeName, string fullHeaderValue)
    {
        // if variableName = qop, the below regex would look like qop="([^""]*)"
        // So it matches anything with the challenge name and then gets the challenge value
        var regHeader = new Regex($@"{challengeName}=""([^""]*)""");
        var matchHeader = regHeader.Match(fullHeaderValue);

        if (matchHeader.Success) return matchHeader.Groups[1].Value;

        throw new ApplicationException($"Header {challengeName} not found");
    }

    private static string GenerateMD5Hash(string input)
    {
        // x2 formatter is for hexadecimal in the ToString function
        using MD5 hash = MD5.Create();
        return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(input))
                                 .Select(x => x.ToString("x2"))
        );
    }

    private static string GetDigestHeader(DigestAuthHeader digest, string digestUri, HttpMethod method)
    {
        var ha1 = GenerateMD5Hash($"{digest.Username}:{digest.Realm}:{digest.Password}");
        var ha2 = GenerateMD5Hash($"{method}:{digestUri}");
        var digestResponse =
            GenerateMD5Hash($"{ha1}:{digest.Nonce}:{digest.NonceCount:00000000}:{digest.ClientNonce}:{digest.QualityOfProtection}:{ha2}");

        var headerString =
            $"Digest username=\"{digest.Username}\", realm=\"{digest.Realm}\", nonce=\"{digest.Nonce}\", uri=\"{digestUri}\", " +
            $"algorithm=MD5, qop={digest.QualityOfProtection}, nc={digest.NonceCount:00000000}, cnonce=\"{digest.ClientNonce}\", " +
            $"response=\"{digestResponse}\", opaque=\"{digest.Opaque}\"";

        return headerString;
    }

    private static HttpRequestMessage CloneBeforeContentSet(this HttpRequestMessage req)
    {
        // Deep clone of a given request, outlined here:
        // https://stackoverflow.com/questions/18000583/re-send-httprequestmessage-exception/18014515#18014515
        HttpRequestMessage clone = new HttpRequestMessage(req.Method, req.RequestUri);

        clone.Content = req.Content;
        clone.Version = req.Version;

        foreach (KeyValuePair<string, object> prop in req.Properties)
        {
            clone.Properties.Add(prop);
        }

        foreach (KeyValuePair<string, IEnumerable<string>> header in req.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}


