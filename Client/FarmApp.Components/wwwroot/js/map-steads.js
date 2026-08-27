let hoveredSteadFeatureId = 0;
let firstMoveEventSent = 0;

const steadPolygonsLayerId = "stead-polygons-id";
const steadLinesLayerId = 'stead-lines-id-layer';
const steadPolygonsSourceId = "stead-polygons-source";
const steadPolygonsSourceLayer = "stead-polygons-layer";

function steadsInteractionInitialize() {
    map.on('load', function () {
        map.addSource(steadPolygonsSourceId, {
            'type': 'vector',
            'tiles': [
                'https://YOUR-VECTOR-TILES-HOST-HERE/farm-app-vector-tiles/{z}/tile_{z}_{x}_{y}.mvt'
            ],
            'minzoom': 11,
            'maxzoom': 14
        });

        map.addLayer({
            'id': steadPolygonsLayerId,
            'type': 'fill',
            'source': steadPolygonsSourceId,
            'source-layer': steadPolygonsSourceLayer,
            'paint': {
                'fill-color': '#008800',
                'fill-opacity': [
                    'case',
                    [
                        'any',
                        ['boolean', ['feature-state', 'hover'], false],
                        ['boolean', ['feature-state', 'selected'], false],
                    ],
                    [
                        'case',
                        ['boolean', ['feature-state', 'selected'], false],
                        0.8,
                        0.4
                    ],
                    0.1
                ]
            },
            'filter': ['==', '$type', 'Polygon']
        });

        map.addLayer({
            'id': steadLinesLayerId,
            'type': 'line',
            'source': steadPolygonsSourceId,
            'source-layer': steadPolygonsSourceLayer,
            'paint': {
                'line-color': 'rgba(0, 140, 0, 1)',
                'line-width': 0.5
            }
        });
    });

    map.on('click', steadPolygonsLayerId, function (event) {
        if (drawingMode || propertyCreatingMode) return;

        const steadFeature = getSteadFeatureByEvent(map, event);
        if (!steadFeature) return;

        const steadId = steadFeature.properties.steadId;
        const property = propertiesList.find(x => x.propertySteads.some(p => p.steadId === steadId));

        setModalPosition(event.lngLat);
        DotNet.invokeMethodAsync("FarmApp.Services", 'OnSteadClickedFromJsAsync', steadId,
            property ? property.id : null);
    });

    // ['case', ['boolean', ['feature-state', 'selected'], false], 0.8, 0.4]
    
    // map.on('mousemove', steadPolygonsLayerId, (event) => {
    //     if (drawingMode) return;
    //
    //     const steadFeature = getSteadFeatureByEvent(map, event);
    //     if (!steadFeature) return;
    //
    //     setSteadFeatureHoveredState(false);
    //     if (propertyCreatingMode && isSteadFeaturePartOfProperty(steadFeature)) return;
    //    
    //     hoveredSteadFeatureId = steadFeature.id;
    //     setSteadFeatureHoveredState(true);
    // });

    // map.on('mouseleave', steadPolygonsLayerId, () => {
    //     setSteadFeatureHoveredState(false);
    //     hoveredSteadFeatureId = 0;
    // });
    
    map.on('movestart', (event) => {
        if (firstMoveEventSent && (event.originalEvent && 
            (event.originalEvent.type === 'mousemove' || event.originalEvent.type === 'touchmove'))) {
            
            firstMoveEventSent = false;
            // DotNet.invokeMethodAsync("FarmApp.Services", 'OnMapFirstMoveFromJsAsync');
        }
    });

    map.on('moveend', () => {
        firstMoveEventSent = true;
    });

    map.on('mouseup', () => {
        firstMoveEventSent = true;
    });

    map.on('touchend', () => {
        firstMoveEventSent = true;
    });
}

function setSteadFeatureHoveredState(hovered) {
    if (!hoveredSteadFeatureId) return;

    map.setFeatureState(
        {source: steadPolygonsSourceId, sourceLayer: steadPolygonsSourceLayer, id: hoveredSteadFeatureId},
        {hover: hovered}
    );
}

function editSteadOnMap(steadId) {
    closeMapModal();

    const steadFeatures = map.querySourceFeatures(steadPolygonsSourceId, {
        sourceLayer: steadPolygonsSourceLayer,
        filter: ["==", "steadId", steadId]
    });

    if (!steadFeatures || !steadFeatures.length) return;

    const combinedFeature = combineGeometries(steadFeatures);
    const coordinates = combinedFeature.geometry.coordinates[0];

    if (coordinates.length < 4) return;

    onStartDrawingWithCoordinates(coordinates, steadId);
}