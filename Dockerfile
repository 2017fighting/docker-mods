## Buildstage ##
FROM ghcr.io/linuxserver/baseimage-alpine:3.20 AS buildstage

## download ede.js online
RUN apk add --no-cache curl
COPY root/ /root-layer/
RUN curl -o /root-layer/app/emby/system/dashboard-ui/modules/custom/ede.js -L \
  "https://raw.githubusercontent.com/chen3861229/dd-danmaku/refs/heads/main/ede.js"

## Single layer deployed image ##
FROM scratch

LABEL maintainer="2017fighting"

# Add files from buildstage
COPY --from=buildstage /root-layer/ /
