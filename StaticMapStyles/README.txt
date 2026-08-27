Native MapLibre (MAUI) — planet on dark theme only

- osm-bright is always loaded from:
  https://tile.openstreetmap.org.ua/styles/osm-bright/style.json

1) Spaces-hosted dark style (planet pmtiles inside that JSON)
   - Upload at least:
     map/styles/dark-matter-gl-style/style.json
   - In DigitalOcean Spaces: these objects (or the whole prefix) must be public-read. If they are private, HTTP returns 403 and the native map will not load (blank).
   - planet.pmtiles can be public while style.json is still 403 — fix ACL / File listing policy for the style paths.
   - Set in appsettings: MapTiles:UseSpacesMapStyles = true and MapTiles:StyleBaseUrl = https://.../map (no trailing slash).

2) Default (UseSpacesMapStyles = false)
   - Native map uses tile.openstreetmap.org.ua style URLs (vector from OSM UA). Reliable without uploading files.
   - Blazor map still uses map-config.js (pmtiles://…/planet.pmtiles) when you keep that line — independent of MapTiles flags.

Debug checklist
- From a machine: curl -I https://…/map/styles/dark-matter-gl-style/style.json → expect 200, not 403 (only when UseSpacesMapStyles is true).
- curl -I https://…/planet.pmtiles → expect 200 (large file).
- Xcode / logcat: search for [MapStyleState] to see which URL mode is active.
