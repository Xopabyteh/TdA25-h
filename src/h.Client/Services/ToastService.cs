﻿using Microsoft.JSInterop;

namespace h.Client.Services;

/// <summary>
/// Both for server and client use.
/// </summary>
public class ToastService
{
    private readonly IJSRuntime _js;

    public ToastService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task ErrorAsync(string msg)
    {
        await _js.InvokeVoidAsync("ToastHelper.error", msg);
    }

    public async Task SuccessAsync(string msg)
    {
        await _js.InvokeVoidAsync("ToastHelper.success", msg);
    }
}
