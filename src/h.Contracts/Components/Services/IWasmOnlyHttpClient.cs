namespace h.Contracts.Components.Services;

/// <summary>
/// Server implementation throws exception, 
/// meanwhile the WASM implementation returns the HttpClient.
/// </summary>
public interface IWasmOnlyHttpClient
{
    public HttpClient Http { get; }

    public class CannotUseWasmOnlyHttpClientOutsideWasmException : Exception
    {
        public CannotUseWasmOnlyHttpClientOutsideWasmException()
            : base("Cannot use WasmOnlyHttpClient outside of WebAssembly.") { }
    }
}
