## Emby-Trailer-Preview

感谢 `https://github.com/XingyiHua2024/Emby-Javascript-Details/blob/main/list_page_trailer.js`

自动给`dashboad-ui/index.html`打补丁

## 使用方法

1. 必须是`linuxserver/emby`的镜像
2. 添加环境变量
```diff
services:
  emby:
    image: lscr.io/linuxserver/emby:latest
    environment:
+      DOCKER_MODS: 272567571/mods:emby-trailer-preview
```
3. 如果你已经使用了DOCKER_MODS，可以使用`|`分割多个mods
```diff
services:
  emby:
    image: lscr.io/linuxserver/emby:latest
    environment:
+      DOCKER_MODS: other-docke-mod|272567571/mods:emby-trailer-preview
```
