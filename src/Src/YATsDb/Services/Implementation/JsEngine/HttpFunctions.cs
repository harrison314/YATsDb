using Jint;
using Jint.Native;
using Jint.Native.Json;
using Jint.Native.Object;
using Microsoft.Net.Http.Headers;

namespace YATsDb.Services.Implementation.JsEngine;

internal class HttpFunctions
{
    private readonly Engine engine;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly bool isEnabled;

    public HttpFunctions(Engine engine, IHttpClientFactory httpClientFactory, bool isEnabled)
    {
        this.engine = engine;
        this.httpClientFactory = httpClientFactory;
        this.isEnabled = isEnabled;
    }

    public JsValue GetJson(string url)
    {
        return this.GetJsonAsync(url, null).GetAwaiter().GetResult();
    }

    public JsValue GetJson(string url, IDictionary<string, object>? headers)
    {
        return this.GetJsonAsync(url, headers).GetAwaiter().GetResult();
    }

    public Task<JsValue> GetJsonAsync(string url)
    {
        return this.GetJsonAsync(url, null);
    }

    public async Task<JsValue> GetJsonAsync(string url, IDictionary<string, object>? headers)
    {
        this.CheckEnabled();

        HttpClient httpClient = this.httpClientFactory.CreateClient();
        using HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get,
             url);

        if (headers != null)
        {
            foreach ((string headerName, object headerValue) in headers)
            {

                httpRequestMessage.Headers.Remove(headerName);
                httpRequestMessage.Headers.TryAddWithoutValidation(headerName, (string)headerValue);
            }

        }
        else
        {
            httpRequestMessage.Headers.Accept.Clear();
            httpRequestMessage.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        using HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequestMessage);
        httpResponse.EnsureSuccessStatusCode();

        string content = await httpResponse.Content.ReadAsStringAsync();
        return new JsonParser(this.engine).Parse(content);
    }

    public async Task<JsValue> PostJsonAsync(string url, IDictionary<string, object> json, IDictionary<string, object>? headers)
    {
        this.CheckEnabled();

        HttpClient httpClient = this.httpClientFactory.CreateClient();
        using HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get,
             url);

        if (headers != null)
        {
            foreach ((string headerName, object headerValue) in headers)
            {

                httpRequestMessage.Headers.Remove(headerName);
                httpRequestMessage.Headers.TryAddWithoutValidation(headerName, (string)headerValue);
            }

        }
        else
        {
            httpRequestMessage.Headers.Accept.Clear();
            httpRequestMessage.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        httpRequestMessage.Content = JsonContent.Create(json);

        using HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequestMessage);
        httpResponse.EnsureSuccessStatusCode();

        string? content = await httpResponse.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(content))
        {
            return JsValue.Null;
        }

        return new JsonParser(this.engine).Parse(content);
    }

    public Task<JsValue> PostStringAsync(string url, string contentValue)
    {
        return this.PostStringAsync(url, contentValue, null);
    }

    public async Task<JsValue> PostStringAsync(string url, string contentValue, IDictionary<string, object>? headers)
    {
        this.CheckEnabled();

        HttpClient httpClient = this.httpClientFactory.CreateClient();
        using HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get,
             url);

        if (headers != null)
        {
            foreach ((string headerName, object headerValue) in headers)
            {

                httpRequestMessage.Headers.Remove(headerName);
                httpRequestMessage.Headers.TryAddWithoutValidation(headerName, (string)headerValue);
            }

        }
        else
        {
            httpRequestMessage.Headers.Accept.Clear();
            httpRequestMessage.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        httpRequestMessage.Content = new StringContent(contentValue);

        using HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequestMessage);
        httpResponse.EnsureSuccessStatusCode();

        string? content = await httpResponse.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(content))
        {
            return JsValue.Null;
        }

        return new JsonParser(this.engine).Parse(content);
    }

    public Task<JsValue> PostJsonAsync(string url, IDictionary<string, object> json)
    {
        return this.PostJsonAsync(url, json, null);
    }

    private void CheckEnabled()
    {
        if (!this.isEnabled)
        {
            throw new JsApiException("HTTP API is not enabled.");
        }
    }
}