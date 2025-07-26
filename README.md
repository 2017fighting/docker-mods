## Emby-CRX

感谢 `https://github.com/Nolovenodie/emby-crx`

自动给`dashboad-ui/index.html`打补丁

## 使用方法

1. 必须是`linuxserver/emby`的镜像
2. 添加环境变量
```diff
services:
  emby:
    image: lscr.io/linuxserver/emby:latest
    environment:
+      DOCKER_MODS: 272567571/mods:emby-crx
+      EMBY_CRX_PARENTID: 3 # 可选，如果传入则只轮播这些媒体库，进入媒体库后url里的parentId，不支持多个
+      EMBY_HIDETITLE: true # 可选，影响hover媒体库卡片时是否隐藏标题，默认为false，不隐藏，传入true则隐藏
```
3. 如果你已经使用了DOCKER_MODS，可以使用`|`分割多个mods
```diff
services:
  emby:
    image: lscr.io/linuxserver/emby:latest
    environment:
+      DOCKER_MODS: other-docke-mod|272567571/mods:emby-crx
```
