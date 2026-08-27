let propertyCreatingMode = false;
const polygonSteadIds = [];
const polygonCustomSteadIds = [];
const propertiesList = [];

let pendingDrawnPolygonFeature = null;

const propertyPolygonsSourceId = "property-polygons-source";
const propertyPolygonsLayerId = "property-polygons-id";

const pendingFieldPreviewSourceId = "pending-field-preview-source";
const pendingFieldPreviewFillLayerId = "pending-field-preview-fill";
const pendingFieldPreviewLineLayerId = "pending-field-preview-line";

function syncPendingFieldPreviewLayer() {
    if (!map || typeof map.getSource !== "function") return;
    const src = map.getSource(pendingFieldPreviewSourceId);
    if (!src || typeof src.setData !== "function") return;
    src.setData({
        type: 'FeatureCollection',
        features: pendingDrawnPolygonFeature ? [pendingDrawnPolygonFeature] : []
    });
}

function propertyEditorInitialize() {
    map.on('load', function () {
        map.addSource(propertyPolygonsSourceId,
        {
            type: 'geojson',
            data: {
                type: 'FeatureCollection',
                features: []
            }
        });

        map.addLayer({
            'id': propertyPolygonsLayerId,
            'type': 'fill',
            'source': propertyPolygonsSourceId,
            'paint': {
                'fill-color': '#000088',
                'fill-opacity': 0.2
            }
        });

        map.addSource(pendingFieldPreviewSourceId, {
            type: 'geojson',
            data: { type: 'FeatureCollection', features: [] }
        });

        map.addLayer({
            id: pendingFieldPreviewFillLayerId,
            type: 'fill',
            source: pendingFieldPreviewSourceId,
            filter: ['==', '$type', 'Polygon'],
            paint: {
                'fill-color': '#2244cc',
                'fill-opacity': 0.28
            }
        }, propertyPolygonsLayerId);

        map.addLayer({
            id: pendingFieldPreviewLineLayerId,
            type: 'line',
            source: pendingFieldPreviewSourceId,
            filter: ['==', '$type', 'Polygon'],
            layout: { 'line-cap': 'round', 'line-join': 'round' },
            paint: {
                'line-color': '#2244cc',
                'line-width': 2,
                'line-dasharray': [2, 2]
            }
        }, propertyPolygonsLayerId);

        // map.setLayerZoomRange(propertyPolygonsLayerId, 11, 23);
        syncPendingFieldPreviewLayer();
    });
    
    map.on('click', steadPolygonsLayerId, function (event) {
        if (!propertyCreatingMode) return;

        const steadFeature = getFeatureByLocation(map, event.point, steadPolygonsLayerId);
        if (!steadFeature || isSteadFeaturePartOfProperty(steadFeature)) return;

        const isFeatureSelected = polygonSteadIds.includes(steadFeature.id);

        if (!isFeatureSelected) {
            polygonSteadIds.push(steadFeature.id);
        } else {
            polygonSteadIds.splice(polygonSteadIds.indexOf(steadFeature.id), 1);
        }

        map.setFeatureState({
                id: steadFeature.id,
                sourceLayer: steadPolygonsSourceLayer,
                source: steadPolygonsSourceId
            }, {selected: !isFeatureSelected}
        );
        notifyPropertyParcelSelectionChangedToDotNet();
    });

    map.on('click', customSteadPolygonsLayerId, function (event) {
        if (!propertyCreatingMode) return;

        const customSteadFeature = getFeatureByLocation(map, event.point, customSteadPolygonsLayerId);
        if (!customSteadFeature || isCustomSteadFeaturePartOfProperty(customSteadFeature)) return;

        const isFeatureSelected = polygonCustomSteadIds.includes(customSteadFeature.id);

        if (!isFeatureSelected) {
            polygonCustomSteadIds.push(customSteadFeature.id);
        } else {
            polygonCustomSteadIds.splice(polygonCustomSteadIds.indexOf(customSteadFeature.id), 1);
        }

        map.setFeatureState({
                id: customSteadFeature.id,
                source: customSteadPolygonsSourceId
            }, {selected: !isFeatureSelected}
        );
        notifyPropertyParcelSelectionChangedToDotNet();
    });

    map.on('click', propertyPolygonsLayerId, function (event) {
        if (propertyCreatingMode) return;
        
        const customSteadFeature = getFeatureByLocation(map, event.point, customSteadPolygonsLayerId);
        if (customSteadFeature) return;

        const steadFeature = getFeatureByLocation(map, event.point, steadPolygonsLayerId);
        if (steadFeature) return;

        const propertyFeature = getFeatureByLocation(map, event.point, propertyPolygonsLayerId);
        if (!propertyFeature) return;

        setModalPosition(event.lngLat);
        DotNet.invokeMethodAsync("FarmApp.Services", 'OnPropertyClickedFromJsAsync', propertyFeature.properties.propertyId);
    });
}

function drawPropertiesBasedOnSteadData() {
    // const steadFeatures = map.querySourceFeatures(steadPolygonsSourceId, {
    //     sourceLayer: steadPolygonsSourceLayer,
    //     filter: ["in", "steadId", ...propertiesList.map(x => {
    //         return `${x}`;
    //     })]
    // });

    // console.log('steadFeatures', propertiesList, steadFeatures);
}

function drawPropertiesOnMap(properties) {
    for (const property of propertiesList) {
        if (property.marker) property.marker.remove();
    }
    
    propertiesList.length = 0;
    propertiesList.push(...properties);
    drawPropertiesOnMapFromList();
}

function drawPropertyOnMap(property) {
    propertiesList.push(property);
    drawPropertiesOnMapFromList();
}

function removePropertyFromMap(propertyId) {
    const property = propertiesList.find(x => x.id === propertyId);
    if (!property) return;
    if (property.marker) property.marker.remove();
    
    const properties = propertiesList.filter(x => x.id !== propertyId);
    propertiesList.length = 0;
    propertiesList.push(...properties);
    drawPropertiesOnMapFromList();
}

function drawPropertiesOnMapFromList() {
    for (const property of propertiesList) {
        if (property.hasNotes && property.marker) {
            property.marker.remove();
        }
    }
    
    const featuresList = [];

    for (const property of propertiesList) {
        const polygonFeature = JSON.parse(property.multipolygonSerialized);
        polygonFeature.properties = { propertyId: property.id };
        
        featuresList.push(polygonFeature);
        if (!property.hasNotes) continue;

        const markerPoint = getSouthWestPointOfPolygon(polygonFeature);

        const markerElement = document.createElement('div');
        markerElement.className = 'property-has-notes-marker';
        const propertyId = property.id;

        markerElement.addEventListener('click', (e) => {
            e.stopPropagation();
            setModalPosition(markerPoint);
            DotNet.invokeMethodAsync("FarmApp.Services", 'OnPropertyNotesClickedFromJsAsync', propertyId);
        });
        
        property.marker = new maplibregl.Marker({ element: markerElement })
            .setLngLat(markerPoint)
            .addTo(map);
    }
    
    map.getSource(propertyPolygonsSourceId).setData({
        type: 'FeatureCollection',
        features: featuresList
    });
}

function removePropertyById(propertyId) {
    DotNet.invokeMethodAsync("FarmApp.Services", 'OnPropertyRemoveClickedFromJsAsync', propertyId);
}

function farmAppSetPendingDrawnPolygonForProperty(coordinatesJson) {
    const ring = JSON.parse(coordinatesJson);
    pendingDrawnPolygonFeature = {
        type: 'Feature',
        geometry: { type: 'Polygon', coordinates: [ring] },
        properties: {}
    };
    syncPendingFieldPreviewLayer();
    notifyPropertyParcelSelectionChangedToDotNet();
}

function farmAppClearPendingDrawnPolygonForProperty() {
    pendingDrawnPolygonFeature = null;
    syncPendingFieldPreviewLayer();
    notifyPropertyParcelSelectionChangedToDotNet();
}

function notifyPropertyParcelSelectionChangedToDotNet() {
    const count = polygonSteadIds.length + polygonCustomSteadIds.length + (pendingDrawnPolygonFeature ? 1 : 0);
    DotNet.invokeMethodAsync("FarmApp.Services", "NotifyPropertyParcelSelectionChangedFromJs", count)
        .catch(function () { });
}

function onStartPropertyCreatingClicked() {
    closeMapModal();
    propertyCreatingMode = true;
    syncPendingFieldPreviewLayer();
    notifyPropertyParcelSelectionChangedToDotNet();
    // map.scrollZoom.disable();
    // map.touchZoomRotate['_tapDragZoom']['_enabled'] = false;
}

function onStopPropertyCreatingClicked() {
    // map.scrollZoom.enable();
    // map.touchZoomRotate['_tapDragZoom']['_enabled'] = true;

    for (let featureId of polygonSteadIds) {
        map.setFeatureState({
                id: featureId,
                sourceLayer: steadPolygonsSourceLayer,
                source: steadPolygonsSourceId
            }, {selected: false}
        );
    }

    for (let featureId of polygonCustomSteadIds) {
        map.setFeatureState({
                id: featureId,
                source: customSteadPolygonsSourceId
            }, {selected: false}
        );
    }

    propertyCreatingMode = false;
    polygonSteadIds.length = 0;
    polygonCustomSteadIds.length = 0;
    pendingDrawnPolygonFeature = null;
    syncPendingFieldPreviewLayer();
    notifyPropertyParcelSelectionChangedToDotNet();
}

function getPropertyEditorState() {
    const customSteadIds = [];

    for (let i = 0; i < polygonCustomSteadIds.length; i++) {
        const customStead = customSteadsList[polygonCustomSteadIds[i] - 1];
        customSteadIds.push(customStead.id);
    }

    const steadFeatures = map.querySourceFeatures(steadPolygonsSourceId, {
        sourceLayer: steadPolygonsSourceLayer,
        filter: ["in", "steadId", ...polygonSteadIds.map(x => {
            return `${x}`;
        })]
    });
    
    const customSteadFeatures = map.querySourceFeatures(customSteadPolygonsSourceId, {
        filter: ["in", "customSteadId", ...customSteadIds]
    });
    
    const areas = [...steadFeatures.map(x => turf.area(x)), ...customSteadFeatures.map(x => turf.area(x))];
    if (pendingDrawnPolygonFeature) areas.push(turf.area(pendingDrawnPolygonFeature));
    const area = areas.reduce((accumulator, currentValue) => accumulator + currentValue, 0);
    const areaHectares = area / 10000;
    
    const customSteadFeaturesSimplified = customSteadFeatures.map(f => {
        const feature = {
            ...f,
            geometry: turf.simplify(f.geometry)
        }
        
        return turf.buffer(feature, 0);
    });
    const steadFeaturesSimplified = steadFeatures.map(f => {
        const feature = {
            ...f,
            geometry: turf.simplify(f.geometry)
        }

        return turf.buffer(feature, 0);
    });

    const pieceFeatures = [...steadFeaturesSimplified, ...customSteadFeaturesSimplified];
    if (pendingDrawnPolygonFeature) {
        const simplified = turf.simplify(pendingDrawnPolygonFeature.geometry);
        pieceFeatures.push(turf.buffer({ type: 'Feature', geometry: simplified, properties: {} }, 0));
    }

    const combinedFeature = combineGeometries(pieceFeatures);
    const bufferedMultiPolygon = turf.buffer(combinedFeature, 50, 'meters');

    return {
        steadIds: polygonSteadIds.map(x => {
            return `${x}`
        }),
        customReportSteadIds: customSteadIds,
        multipolygonSerialized: JSON.stringify(bufferedMultiPolygon),
        area: areaHectares
    }
}

function isSteadFeaturePartOfProperty(steadFeature) {
    for (let property of propertiesList) {
        if (property.propertySteads.some(x => x.steadId === `${steadFeature.id}`)) return true;
    }
    
    return false;
}

function isCustomSteadFeaturePartOfProperty(customSteadFeature) {
    for (let property of propertiesList) {
        if (property.propertySteads.some(x => x.customSteadId === customSteadFeature.properties.customSteadId)) 
            return true;
    }

    return false;
}

function getSouthWestPointOfPolygon(polygonFeature) {
    const turfPolygon = turf.polygon(polygonFeature.geometry.coordinates);
    const centroid = turf.centroid(turfPolygon);

    return centroid.geometry.coordinates;
}

function getPropertySelectionCount() {
    return polygonSteadIds.length + polygonCustomSteadIds.length + (pendingDrawnPolygonFeature ? 1 : 0);
}

function getPropertyEditorStateForCreate() {
    if (polygonSteadIds.length === 0 && polygonCustomSteadIds.length === 0 && !pendingDrawnPolygonFeature) {
        return null;
    }
    const s = getPropertyEditorState();
    return {
        steadIds: s.steadIds,
        customSteadIds: s.customReportSteadIds,
        multipolygonSerialized: s.multipolygonSerialized,
        area: s.area,
        features: []
    };
}

window.farmAppMapStartPropertyCreating = onStartPropertyCreatingClicked;
window.farmAppMapStopPropertyCreating = onStopPropertyCreatingClicked;
window.farmAppMapGetPropertySelectionCount = getPropertySelectionCount;
window.farmAppMapGetPropertyEditorStateForCreate = getPropertyEditorStateForCreate;