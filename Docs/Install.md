Um den Robosort Renderer auf einem Server zu installieren werden die folgenden Packages benötigt:

`xvfb`
`mesa-vulkan-drivers`

# Ubuntu-Server

```bash
apt update
apt install xvfb mesa-vulkan-drivers
wget https://github.com/Aschmutz/Robosort/releases/download/v1.0/Robosort_Renderer_v1.0_Linux.zip
mkdir robosort-renderer
unzip Robosort_Renderer_v1.0_Linux.zip -d robosort-renderer
cd robosort-renderer
chmod +x Robosort.x86_64
MESA_LOADER_DRIVER_OVERRIDE=zink xvfb-run ./Robosort.x86_64 -logFile log.txt
```
Die heruntergeladene Version kommt mit einer Beispielkonfiguration sowie 4 Modelle im OBJ. Um bessere Konfiguration vornehmen zu können passen sie das `renderConfiguration.json` File an. Das Schema dieses Files ist in einer Beiliegenden HTML Dokumentation definiert. Bei Anpassungen bitte zuerst die Dokumentation und die Beispielkonfiguration ansehen.
