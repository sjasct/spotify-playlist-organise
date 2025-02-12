﻿using Microsoft.Extensions.Options;
using PlaylistOrganiser.Models.Configuration;
using SpotifyAPI.Web;

namespace PlaylistOrganiser.Handlers;

public class AuthHandler(IOptions<AppConfig> config)
{
    private AuthorizationCodeTokenResponse? _tokens;
    
    public Uri GetAuthUrl()
    {
        var request = new LoginRequest(GetRedirectUri(), config.Value.ApiCredentials.ClientId, LoginRequest.ResponseType.Code)
        {
            Scope = new List<string>
            {
                Scopes.PlaylistReadPrivate, Scopes.PlaylistReadCollaborative, Scopes.PlaylistModifyPrivate,
                Scopes.PlaylistModifyPublic
            }
        };

        return request.ToUri();
    }
    
    public async Task ExchangeCode(string code)
    {
        _tokens = await new OAuthClient().RequestToken(
            new AuthorizationCodeTokenRequest(config.Value.ApiCredentials.ClientId, config.Value.ApiCredentials.Secret, code, GetRedirectUri())
        );
    }

    // todo: better way of storing and getting tokens
    public AuthorizationCodeTokenResponse? GetTokens()
    {
        return _tokens;
    }

    private Uri GetRedirectUri()
    {
        return new Uri($"{config.Value.BaseUrl}/auth/redirect");
    }
}