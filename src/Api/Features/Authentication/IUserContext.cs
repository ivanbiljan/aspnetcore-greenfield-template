﻿using System.Globalization;
using System.Security.Claims;

namespace Api.Features.Authentication;

public interface IUserContext
{
    bool IsAuthenticated { get; }

    string? PreferredLanguage { get; }

    string? UserAgent { get; }

    string? IpAddress { get; }

    int GetId();
}

[RegisterScoped]
internal sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public int GetId()
    {
        if (_httpContextAccessor.HttpContext?.User is not {Identity.IsAuthenticated: true} principal)
        {
            throw new InvalidOperationException();
        }

        return int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!, CultureInfo.InvariantCulture);
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User is {Identity.IsAuthenticated: true};

    public string? PreferredLanguage =>
        _httpContextAccessor.HttpContext!.Request.Headers.AcceptLanguage.FirstOrDefault();

    public string? UserAgent => _httpContextAccessor.HttpContext!.Request.Headers.UserAgent;

    public string? IpAddress => _httpContextAccessor.HttpContext!.Connection.RemoteIpAddress?.ToString();
}