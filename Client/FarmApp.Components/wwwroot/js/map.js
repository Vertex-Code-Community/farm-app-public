let map = null;

function initMap() {

    let protocol = new pmtiles.Protocol();
    maplibregl.addProtocol("pmtiles", protocol.tile);

    map = new maplibregl.Map({
        container: 'map-id', // container id
        // style: 'https://demotiles.maplibre.org/style.json', // style URL
        // style: mapStyleUrls[0], // style URL
        style: ukraineBrightMapConfig,
        // style: 'https://api.maptiler.com/maps/satellite/style.json?key=YOUR-MAPTILER-KEY-HERE', // style URL
        // style: 'https://api.maptiler.com/maps/topo-v2/style.json?key=YOUR-MAPTILER-KEY-HERE', // style URL
        // style: 'https://demotiles.maplibre.org/style.json',
        zoom: 11,
        // center: [37.8311198693919, 49.2996703481021]
        center: [36.233640, 49.985983]
    });

    map.on('load', function () {
        console.log('map load');
        
        setTimeout(() => {
            // DotNet.invokeMethodAsync("FarmApp.Services", 'OnMapLoaded');
        })
    });

    map.on('zoom', (e) => {
        const zoom = map.getZoom();
        console.log('zoom:', zoom);
    });

    map.on('error', e => {
        console.error('MAPBOX ERROR');
        console.log(e);
        // for (let key in e) {
        //     console.log(key + " : " + e[key]);
        // }
    });

    steadsInteractionInitialize();
    polygonDrawingInitialize();
    customSteadsInitialize();
    propertyEditorInitialize();
}

window.farmAppMapApplyView = function (lng, lat, zoom) {
    if (!map) return;
    try {
        map.flyTo({ center: [lng, lat], zoom: zoom, essential: true });
    } catch (e) {
        console.warn('farmAppMapApplyView', e);
    }
};