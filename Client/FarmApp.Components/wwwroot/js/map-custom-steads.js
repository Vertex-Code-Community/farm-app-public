const customSteadPolygonsSourceId = "custom-stead-polygons-source";
const customSteadPolygonsLayerId = "custom-stead-polygons-id";
const customSteadLinesLayerId = "custom-stead-lines-id";
let customSteadsList = [];
let hoveredCustomSteadFeatureId = 0;

// const customSteadPolygonsSourceLayer = "custom-stead-polygons-layer";

function customSteadsInitialize() {
    map.on('load', function () {
        map.addSource(customSteadPolygonsSourceId,
            {
                type: 'geojson',
                data: {
                    type: 'FeatureCollection',
                    features: []
                }
            });

        map.addLayer({
            'id': customSteadPolygonsLayerId,
            'type': 'fill',
            'source': customSteadPolygonsSourceId,
            'paint': {
                // 'fill-color': '#000088',
                'fill-color': '#008800',

            },
            // 'filter': ['==', '$type', 'Polygon']
        });

        map.addLayer({
            'id': customSteadLinesLayerId,
            'type': 'line',
            'source': customSteadPolygonsSourceId,
            'paint': {
                // 'line-color': 'rgba(0, 0, 140, 1)',
                'line-color': 'rgba(0, 140, 0, 1)',
                'line-width': 0.5
            }
        });

        // map.setLayerZoomRange(customSteadPolygonsLayerId, 11, 23);
        // map.setLayerZoomRange(customSteadLinesLayerId, 11, 23);
    });

    map.on('click', customSteadPolygonsLayerId, function (event) {
        if (drawingMode || propertyCreatingMode) return;

        const customSteadFeature = getFeatureByLocation(map, event.point, customSteadPolygonsLayerId);
        if (!customSteadFeature) return;

        const customSteadId = customSteadFeature.properties.customSteadId;
        const steadId = customSteadFeature.properties.steadId;
        
        const property = propertiesList.find(x => x.propertySteads.some(p => p.customSteadId === customSteadId));

        setModalPosition(event.lngLat);
        DotNet.invokeMethodAsync("FarmApp.Services", 'OnCustomSteadClickedFromJsAsync', customSteadId, steadId,
            property ? property.id : null);
    });

    map.on('mousemove', customSteadPolygonsLayerId, (event) => {
        if (drawingMode) return;

        const customSteadFeature = getFeatureByLocation(map, event.point, customSteadPolygonsLayerId);
        if (!customSteadFeature) return;

        setCustomSteadFeatureHoveredState(false);
        if (propertyCreatingMode && isCustomSteadFeaturePartOfProperty(customSteadFeature)) return;
        
        hoveredCustomSteadFeatureId = customSteadFeature.id;
        setCustomSteadFeatureHoveredState(true);
    });

    map.on('mouseleave', customSteadPolygonsLayerId, () => {
        setCustomSteadFeatureHoveredState(false);
        hoveredCustomSteadFeatureId = 0;
    });
}

function setCustomSteadFeatureHoveredState(hovered) {
    if (!hoveredCustomSteadFeatureId) return;

    map.setFeatureState(
        {source: customSteadPolygonsSourceId, id: hoveredCustomSteadFeatureId},
        {hover: hovered}
    );
}

function removeCustomSteadById(customSteadId) {
    // DotNet.invokeMethodAsync("FarmApp.Services", 'OnCustomSteadRemoveClickedFromJsAsync', customSteadId);
}

function drawCustomSteads(customSteads) {
    customSteadsList = customSteads;
    updateSteadLayersFilters();
    drawCustomSteadsOnMap();
}

function drawCustomStead(customStead) {
    customSteadsList = customSteadsList.filter(x => x.id !== customStead.id);
    customSteadsList.push(customStead);
    updateSteadLayersFilters();
    drawCustomSteadsOnMap();
}

function removeCustomStead(customSteadId) {
    customSteadsList = customSteadsList.filter(x => x.id !== customSteadId);
    drawCustomSteadsOnMap();
    updateSteadLayersFilters();
}

let customSteadIdCounter = 0;

function drawCustomSteadsOnMap() {
    const featureCollection = {
        type: 'FeatureCollection',
        features: customSteadsList.map((customStead, index) => {
            return {
                id: index + 1,
                type: 'Feature',
                geometry: {
                    type: 'Polygon',
                    coordinates: [JSON.parse(customStead.coordinates)]
                },
                properties: {
                    customSteadId: customStead.id,
                    steadId: customStead.steadId
                }
            }
        }),
    };

    map.getSource(customSteadPolygonsSourceId).setData(featureCollection);
}

function updateSteadLayersFilters() {
    const steadIds = customSteadsList.filter(x => x.steadId).map(x => x.steadId);
    const steadsPolygonLayer = map.getLayer(steadPolygonsLayerId);
    const steadsLinesLayer = map.getLayer(steadLinesLayerId);

    const filter = ["!in", "steadId", ...steadIds];

    if (steadsPolygonLayer) map.setFilter(steadPolygonsLayerId, filter);
    if (steadsLinesLayer) map.setFilter(steadLinesLayerId, filter);
}

function editCustomSteadOnMap(steadId, customSteadId) {
    closeMapModal();

    const customStead = customSteadsList.find(x => x.id === customSteadId);
    if (!customStead) return;

    const coordinates = JSON.parse(customStead.coordinates);
    onStartDrawingWithCoordinates(coordinates, steadId, customSteadId);
}