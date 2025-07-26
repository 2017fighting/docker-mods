# Emby Crack - Docker mod for Emby

> 破解Emby Premiere，破解方法参考自[这篇文章](https://yubanmei.com/archives/133.html)

## 使用方法
1. emby镜像必须是`linuxserver/emby`的
2. 自建一个emby的[认证服务](#自建认证服务)
3. 添加环境变量
```diff
services:
  emby:
    image: lscr.io/linuxserver/emby:latest
    environment:
+      DOCKER_MODS: 272567571/mods:emby-crack
+      EMBY_CRACK_URL: https://embycrack.sample.com # 替换成你自建的地址
```
4. 如果你已经用到了DOCKER_MODS，可以使用`|`分割多个mod
```diff
services:
  emby:
    image: lscr.io/linuxserver/emby:latest
    environment:
-      DOCKER_MODS: other-docker-mod
+      DOCKER_MODS: other-docker-mod|272567571/mods:emby-crack
+      EMBY_CRACK_URL: https://embycrack.sample.com # 替换成你自建的地址
```

## 自建认证服务
下面是[caddy](https://caddyserver.com/)的示例
```Caddyfile
(cors) {
        @cors_preflight{args[0]} method OPTIONS
        @cors{args[0]} header Origin {args[0]}
        handle @cors_preflight{args[0]} {
                header {
                        Access-Control-Allow-Origin {args[0]}
                        Access-Control-Allow-Methods "GET, POST, PUT, PATCH, DELETE, OPTIONS"
                        Access-Control-Allow-Headers *
                        Access-Control-Max-Age 3600
                        defer
                }
                respond 204
        }
        handle @cors{args[0]} {
                header {
                        Access-Control-Allow-Origin {args[0]}
                        Access-Control-Expose-Headers *
                        defer
                }
        }
}

(cors_any) {
        @cors_preflight method OPTIONS

        header {
                Access-Control-Allow-Origin "{header.origin}"
                Vary Origin
                Access-Control-Expose-Headers "Authorization"
                Access-Control-Allow-Credentials "true"
        }

        handle @cors_preflight {
                header {
                        Access-Control-Allow-Methods "GET, POST, PUT, PATCH, DELETE"
                        Access-Control-Max-Age "3600"
                }
                respond "" 204
        }
}

# 替换成你的域名
embycrack.sample.com {
    # 跨域 二选一
    import cors_any
    #import cors "https://emby.sample.com" 

    respond /admin/service/registration/validateDevice `{"cacheExpirationDays":3650,"message":"Device Valid","resultCode":"GOOD"}`

    respond /admin/service/registration/validate `{"featId":"MBSupporter","registered":true,"expDate":"2099-01-01","key":"114514"}`

    respond /admin/service/registration/getStatus `{"deviceStatus":"","planType":"Lifetime","subscriptions":{}}`

    respond /admin/service/appstore/register `{"featId":"","registered":true,"expDate":"2099-01-01","key":""}`

    respond /emby/Plugins/SecurityInfo `{"SupporterKey":"","IsMBSupporter":true}`
}
```