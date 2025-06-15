# Red55.Mattermost.OpenId.Proxy

Free version of Mattermost doesn't support OpenID Connect authentication.

This project is a proxy that allows you to use KeyCloak (virtually any OAuth2 capable authentication service) as an OpenID Connect provider for Mattermost.
This proxy will handle the authentication flow and pass the user information to Mattermost.

If authenticating user does exists in OpenId authorization service only, the proxy will create a new user in GitLab and Mattermost.
This is required because Mattermost doesn't support OpenID Connect and requires a GitLab user to be created first to get its integer Id.
OpenId IDs is a GUID, so it can't be used as a GitLab user ID.

Available as container image:

```shell
docker pull red55/mm-openid-proxy:latest
```

## Pre-requisites

1. Create a new Admin user in GitLab.
2. Create Personal Access Token with **`[api,self_rotate]`** scopes. Set it in `appsettings.yml`.
3. Create a new client in OpenId authorization service. Put its credentials in `appsettings.yml`.
4. Don't forget to change OpenId and GitLab URLs in `appsettings.yml` to your own.

## Configuration

```yaml
AppConfig:
  OpenId:
    Url: "https://kc.apps.yunqi.studio"
    AppId: "<OpenID CLIENT ID HERE>"
    AppSecret: "<OpenID CLIENT PASSWORD HERE>"
  GitLab:
    Url: "http://gitlab.apps.yunqi.studio"
    PAT:
      BootstrapToken: "<GitLab PAT HERE>"
      StoreLocation: "/home/app/.gitlab/pat"
      GracePeriod: "1.00:30:00" # 1 day 30 minutes
```

The YAML configuration file could be overridden by environment variables.
For example, you can set `APPCONFIG:OPENID:URL` to override the OpenId URL.
