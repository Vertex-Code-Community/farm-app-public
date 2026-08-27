"""One-off: extract ne_10m admin-0 polygons by ISO3 into Client/.../CountryBorders/."""
import json
import sys
import urllib.request

COUNTRIES_URL = (
    "https://raw.githubusercontent.com/nvkelso/natural-earth-vector/master/"
    "geojson/ne_10m_admin_0_countries.geojson"
)
SUBUNITS_URL = (
    "https://raw.githubusercontent.com/nvkelso/natural-earth-vector/master/"
    "geojson/ne_10m_admin_0_map_subunits.geojson"
)
OUT = "Client/FarmApp.Mobile/Resources/Raw/CountryBorders"
ISO_FILES = {
    "UKR": "UKR.geo.json",
    "USA": "USA.geo.json",
    "ITA": "ITA.geo.json",
    "POL": "POL.geo.json",
    "DEU": "DEU.geo.json",
}

# Subunits to fold into the main feature regardless of NE's ADM0_A3 (which reflects
# effective control). Map: target ISO -> SUBUNIT names to merge.
EXTRA_SUBUNITS = {
    "UKR": {"Crimea"},  # NE puts Crimea under ADM0_A3=RUS; merge into Ukraine.
}


def fetch(url: str):
    print(f"Downloading {url.split('/')[-1]}...", flush=True)
    with urllib.request.urlopen(url, timeout=300) as r:
        return json.load(r)


def coords_of(geom: dict) -> list:
    """Return list of polygons (each a list of rings) — works for Polygon and MultiPolygon."""
    if geom["type"] == "MultiPolygon":
        return geom["coordinates"]
    if geom["type"] == "Polygon":
        return [geom["coordinates"]]
    raise ValueError(f"Unsupported geometry type: {geom['type']}")


def main() -> int:
    countries = fetch(COUNTRIES_URL)
    subunits = None  # lazy-load only if needed

    for iso, fname in ISO_FILES.items():
        feats = [f for f in countries["features"] if f.get("properties", {}).get("ADM0_A3") == iso]
        if not feats:
            print(f"ERROR: no feature for {iso}", file=sys.stderr)
            return 1
        if len(feats) != 1:
            print(f"WARN: {iso} has {len(feats)} features; using first", file=sys.stderr)

        main_feat = feats[0]
        main_geom = main_feat["geometry"]
        polys = coords_of(main_geom)

        extras = EXTRA_SUBUNITS.get(iso, set())
        if extras:
            if subunits is None:
                subunits = fetch(SUBUNITS_URL)
            for sub_name in extras:
                sub_feats = [f for f in subunits["features"]
                             if f.get("properties", {}).get("SUBUNIT") == sub_name]
                if not sub_feats:
                    print(f"WARN: subunit {sub_name!r} not found", file=sys.stderr)
                    continue
                for sf in sub_feats:
                    polys.extend(coords_of(sf["geometry"]))
                print(f"  + merged subunit {sub_name!r} into {iso}", flush=True)

        merged = {
            "type": "Feature",
            "properties": {"name": main_feat.get("properties", {}).get("NAME", iso)},
            "geometry": {"type": "MultiPolygon", "coordinates": polys},
        }
        out = {"type": "FeatureCollection", "features": [merged]}
        path = f"{OUT}/{fname}"
        with open(path, "w", encoding="utf-8") as w:
            json.dump(out, w, ensure_ascii=False, separators=(",", ":"))
        with open(path, encoding="utf-8") as w:
            n = len(w.read())
        print(f"  {iso} -> {fname} ({n:,} bytes, polygons={len(polys)})", flush=True)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
