FROM scratch

LABEL maintainer="2017fighting"

COPY root/ /
COPY trailer-preview.js /app/emby/system/dashboard-ui/modules/custom/trailer-preview.js 
