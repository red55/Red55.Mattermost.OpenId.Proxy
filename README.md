# Red55.Mattermost.OpenId.Proxy

Free version of Mattermost doesn't support OpenID Connect authentication. 

This project is a proxy that allows you to use KeyCloak (virtually any OAuth2 capable authentication service) as an OpenID Connect provider for Mattermost.
This proxy will handle the authentication flow and pass the user information to Mattermost.

If authenticating user does exists in KeyCloak only the proxy will create a new user in GitLab and Mattermost.
This is required because Mattermost doesn't support OpenID Connect and requires a GitLab user to be created first to get it's integer Id.
KeyCloak IDs is a GUID, so it can't be used as a GitLab user ID.

## Pre-requisites

1. Create a new Admin user in GitLab.
2. Create Personal Access Token with `api` scope. Set it in `appsettings.yml`.
4. Create a new client in KeyCloak. Put it's credentials in `appsettings.yml`.
5. Don't forget to change KeyCloak and GitLab URLs in `appsettings.yml` to your own.

```yaml
AppConfig:
  KeyCloak:
    Url: "https://kc.apps.yunqi.studio"
    AppId: "<KC CLIENT ID HERE>"
    AppSecret: "<KC CLIENT PASSWORD HERE>"
  GitLab:
    Url: "http://gitlab.apps.yunqi.studio"
    PAT: "<YOUR PAT HERE>"
```

The YAML configuration file could be overridden by environment variables.
For example, you can set `APPCONFIG_KEYCLOAK_URL` to override the KeyCloak URL.