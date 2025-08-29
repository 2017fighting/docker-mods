## Buildstage ##
FROM ghcr.io/linuxserver/baseimage-alpine:3.20 AS buildstage

## download ede.js online
RUN apk add --no-cache curl
COPY root/ /root-layer/

RUN mkdir -p /root-layer/app/emby/system/dashboard-ui/modules/custom/ && curl -o /root-layer/app/emby/system/dashboard-ui/modules/custom/crx.js -L \
  "https://raw.githubusercontent.com/2017fighting/emby-crx/refs/heads/master/dist/bundle.js"

## Single layer deployed image ##
FROM scratch

LABEL maintainer="2017fighting"

# Add files from buildstage
COPY --from=buildstage /root-layer/ /
