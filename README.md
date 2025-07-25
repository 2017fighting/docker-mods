# Emby Crack - Docker mod for Emby
In linuxserver/emby docker arguments, set an environment variable `DOCKER_MODS=272567571/mods:emby-crack`

If adding multiple mods, enter them in an array separated by |, such as `DOCKER_MODS=272567571/mods:emby-crack|272567571/mods:emby-crack-2`

## Settings in Emby
you need to set some environment let emby-crack works.

`EMBY_CRACK_URL`: your selfhosted embycrack url, emby-crack will replace `https://mb3admin.com` to this, for example`https://embycrack.xxx.com`

`EMBY_PATH`: Optional. where is emby system, default is `/app/emby/system`.
