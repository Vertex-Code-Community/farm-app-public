let drawingMode = false;
let translateMode = false;
let polygonPoints = [];
let polygonDrawn = false;
let polygonMoving = false;
let modifyingSteadId = null;
let modifyingCustomSteadId = null;

const polygonSourceId = 'draw-polygon-source';
const polygonLayerId = 'draw-layer-id';
const lineStringLayerId = 'line-string-points-layer-id';
const verticesLayerId = 'vertex-points-layer-id';
const verticesLayerRenderedId = 'vertex-points-layer-rendered-id';
const additionalVerticesLayerId = 'additional-vertex-points-layer-id';
const additionalVerticesLayerRenderedId = 'additional-vertex-points-layer-rendered-id';

const pointTapZoneRadius = 15; //pixels

let selectedPointFeatureId = -1;
let movingPointFeatureId = -1;
let prevLatLng = null;
let closeMarker = null;

let prevVertexNumber = -1;
let prevSelectedPointFeatureId = -1;

let vertexPointsList = [];

const polygonFeature = {
    type: 'Feature',
    geometry: {
        type: 'Polygon',
        coordinates: [polygonPoints]
    }
};

const combinedFeatureCollection = {
    type: 'FeatureCollection',
    features: [],
};

function polygonDrawingInitialize() {

    map.on('mouseenter', verticesLayerId, function () {
        map.getCanvas().style.cursor = 'pointer';
    });

    map.on('mouseleave', verticesLayerId, function () {
        map.getCanvas().style.cursor = '';
    });

    map.on('mouseenter', additionalVerticesLayerId, function () {
        map.getCanvas().style.cursor = 'pointer';
    });

    map.on('mouseleave', additionalVerticesLayerId, function () {
        map.getCanvas().style.cursor = '';
    });
    
    map.on('click', function (event) {
        onClickOnMap(event.point, event.lngLat);
    });
    
    map.on('mousedown', additionalVerticesLayerId, function (event) {
        onMouseDownAdditionalVerticesLayer(event.point, event.lngLat);
    });

    map.on('mousedown', verticesLayerId, function (event) {
        onMouseDownVerticesLayer(event.point, event.lngLat);
    });
    
    map.on('mousedown', polygonLayerId, (event) => {
        onMouseDownPolygonLayer(event.point, event.lngLat);
    });

    map.on('touchstart', additionalVerticesLayerId, (event) => {
        if (event.points.length > 1) {
            onPolygonEditorUp();
            return;
        }
        
        onMouseDownAdditionalVerticesLayer(event.points[0], event.lngLats[0]);
    });

    map.on('touchstart', verticesLayerId, (event) => {
        if (event.points.length > 1) {
            onPolygonEditorUp();
            return;
        }
        
        onMouseDownVerticesLayer(event.points[0], event.lngLats[0]);
    });
    
    map.on('touchstart', polygonLayerId, (event) => {
        if (event.points.length > 1) {
            onPolygonEditorUp();
            return;
        }
        
        onMouseDownPolygonLayer(event.points[0], event.lngLats[0]);
    });

    map.on('mousemove', (event) => {
        onMouseMoveOverMap(event.lngLat);
    });

    map.on('touchmove', (event) => {
        if (event.points.length > 2) {
            if (polygonMoving || movingPointFeatureId !== -1) onPolygonEditorUp();
            return;
        }
        
        onMouseMoveOverMap(event.lngLats[0]);
    });
    
    map.on('zoom', (event) => {
        if (selectedPointFeatureId !== -1) {
            selectedPointFeatureId = -1;
            hideCloseMarker();
            updateModifiedPolygon();
        }
    });
}

function onClickOnMap(point, lngLat) {
    if (!drawingMode) return;

    if (polygonDrawn) {
        const vertexFeature = getFeatureByLocation(map, point, verticesLayerId);

        if (!vertexFeature) {
            selectedPointFeatureId = -1;
            hideCloseMarker();
            updateModifiedPolygon();
        }

        return;
    }

    //polygonDrawn = true;
    polygonPoints.length = 0;
    polygonPoints.push(...getSquare(lngLat.toArray(), 50));
    updateModifiedPolygon();
}

function onMouseDownVerticesLayer(point, lngLat) {
    const vertexFeature = getFeatureByLocation(map, point, verticesLayerId);
    if (!vertexFeature) return;
    
    const  vertexIndex = vertexFeature.properties.index;
    
    movingPointFeatureId = vertexIndex;
    selectedPointFeatureId = vertexIndex;    
    prevLatLng = lngLat;

    const vertexCoords = vertexFeature.geometry.coordinates;
    
    hideCloseMarker();
    showCloseMarker(new maplibregl.LngLat(vertexCoords[0], vertexCoords[1]));

    map.dragRotate.disable();
    map.dragPan.disable();
    map.scrollZoom.disable();
    map.touchZoomRotate['_tapDragZoom']['_enabled'] = false;

    updateModifiedPolygon();
}

function onMouseDownPolygonLayer(point, lngLat) {
    if (!translateMode) return;
    
    const vertexFeature = getFeatureByLocation(map, point, verticesLayerId);
    const additionalVertexFeature = getFeatureByLocation(map, point, additionalVerticesLayerId);

    if (vertexFeature || additionalVertexFeature) return;

    polygonMoving = true;
    prevLatLng = lngLat;

    map.dragRotate.disable();
    map.dragPan.disable();
    map.scrollZoom.disable();
    map.touchZoomRotate['_tapDragZoom']['_enabled'] = false;
}

function onMouseDownAdditionalVerticesLayer(point, lngLat) {
    const midPointFeature = getFeatureByLocation(map, point, additionalVerticesLayerId);
    if (!midPointFeature) return;
    
    const midPointIndex = midPointFeature.properties.index;
    const newVertexIndex = midPointIndex + 1;
    
    polygonPoints.splice(newVertexIndex, 0, midPointFeature.geometry.coordinates);

    movingPointFeatureId = newVertexIndex;
    selectedPointFeatureId = newVertexIndex;
    prevLatLng = lngLat;

    hideCloseMarker();
    showCloseMarker(lngLat);

    map.dragRotate.disable();
    map.dragPan.disable();
    map.scrollZoom.disable();
    map.touchZoomRotate['_tapDragZoom']['_enabled'] = false;

    updateModifiedPolygon();
}

function onMouseMoveOverMap(lngLat) {
    const deltaX = prevLatLng ? lngLat.lng - prevLatLng.lng : 0;
    const deltaY = prevLatLng ? lngLat.lat - prevLatLng.lat : 0;

    if (movingPointFeatureId !== -1) {
        const point = polygonPoints[movingPointFeatureId];
        prevLatLng = lngLat;

        point[0] += deltaX;
        point[1] += deltaY;

        if (movingPointFeatureId === 0) {
            const lastPoint = polygonPoints[polygonPoints.length - 1];

            lastPoint[0] += deltaX;
            lastPoint[1] += deltaY;
        }

        updateModifiedPolygon();
    }

    if (polygonMoving) {
        prevLatLng = lngLat;

        polygonPoints.map(point => {
            point[0] += deltaX;
            point[1] += deltaY;
        });

        updateModifiedPolygon();
    }

    if ((movingPointFeatureId !== -1 || polygonMoving) && (closeMarker && closeMarker.isOpen())) {
        const popupPos = closeMarker.getLngLat();
        closeMarker.setLngLat(new maplibregl.LngLat(popupPos.lng + deltaX, popupPos.lat + deltaY))
    }
}

function onPolygonEditorUp() {
    polygonMoving = false;
    prevLatLng = null;
    movingPointFeatureId = -1;

    map.dragRotate.enable();
    map.dragPan.enable();
    map.scrollZoom.enable();
    map.touchZoomRotate['_tapDragZoom']['_enabled'] = true;
}



function updateModifiedPolygon() {
    if (prevVertexNumber !== polygonPoints.length || prevSelectedPointFeatureId !== selectedPointFeatureId) {
        vertexPointsList = [];
        
        for (let i = 0; i < polygonPoints.length - 1; i ++) {
            const coord = polygonPoints[i];
            
            vertexPointsList.push({
                type: 'Feature',
                geometry: {
                    type: 'Point',
                    coordinates: coord
                },
                properties: {
                    index: i,
                    isSelected: i === selectedPointFeatureId,
                    vertex: true
                }
            });
        }
    }
    
    const medianPoints = calculateMedianPoints(polygonPoints.slice(0, -1));
    const midpointFeatures = medianPoints.map((coord, index) => {
        return {
            type: 'Feature',
            geometry: {
                type: 'Point',
                coordinates: coord
            },
            properties: {
                index: index,
                midpoint: true
            }
        }
    });
    
    combinedFeatureCollection.features.length = 0;
    combinedFeatureCollection.features.push(polygonFeature);
    combinedFeatureCollection.features.push(...vertexPointsList);
    combinedFeatureCollection.features.push(...midpointFeatures);
    
    map.getSource(polygonSourceId).setData(combinedFeatureCollection);

    prevVertexNumber = polygonPoints.length;
    prevSelectedPointFeatureId = selectedPointFeatureId;
}

function showCloseMarker(position) {
    if (closeMarker && closeMarker.isOpen()) return;

    const htmlContent = `
      <div onclick="onRemoveVertexClicked()" class="remove-vertex-icon">&#10006;</div>
    `;

    closeMarker = new maplibregl.Popup({ closeOnClick: false, closeButton: false })
        .setLngLat(position)
        .setHTML(htmlContent)
        .addTo(map);

    closeMarker.getElement().classList.add('vertex-polygon-selected-point');
}

function hideCloseMarker() {
    if (closeMarker && closeMarker.isOpen()) {
        closeMarker.remove();
        closeMarker = null;
    }
}

function onRemoveVertexClicked() {
    if(polygonPoints.length <= 4) return;
    
    polygonPoints.splice(selectedPointFeatureId,1);
    
    if (selectedPointFeatureId === 0) {
        polygonPoints.splice(polygonPoints.length - 1,1);
        const firstPoint = polygonPoints[0];
        polygonPoints.push([firstPoint[0], firstPoint[1]]);
    }

    selectedPointFeatureId = -1;
    movingPointFeatureId = -1;
    prevLatLng = null;
    
    hideCloseMarker();
    updateModifiedPolygon();
}

// Start drawing mode
function onStartDrawingClicked() {
    if (drawingMode) return;

    drawingMode = true;
    polygonPoints.length = 0

    addSourcesAndLayers();
}

// Stop drawing mode
function onStopDrawingClicked() {
    if (!drawingMode) return;
    drawingMode = false;
    polygonDrawn = false;
    selectedPointFeatureId = -1;
    polygonMoving = false;
    prevLatLng = null;
    movingPointFeatureId = -1;
    prevVertexNumber = -1;
    modifyingSteadId = null;
    modifyingCustomSteadId = null;

    hideCloseMarker();

    map.removeLayer(polygonLayerId);
    map.removeLayer(lineStringLayerId);
    map.removeLayer(verticesLayerId);
    map.removeLayer(verticesLayerRenderedId);
    map.removeLayer(additionalVerticesLayerId);
    map.removeLayer(additionalVerticesLayerRenderedId);

    map.removeSource(polygonSourceId);
}

function addSourcesAndLayers() {
    
    map.addSource(polygonSourceId, {type: 'geojson', data: {type: 'FeatureCollection', features: []}});
    
    map.addLayer({
        id: polygonLayerId,
        type: 'fill',
        source: polygonSourceId,
        filter: ['==', '$type', 'Polygon'],
        paint: {
            'fill-color': '#aa0000',
            'fill-opacity': 0.1
        }
    });
    
    map.addLayer({
        id: lineStringLayerId,
        type: 'line',
        source: polygonSourceId,
        filter: ['==', '$type', 'Polygon'],
        layout: {
            'line-cap': 'round',
            'line-join': 'round',
        },
        paint: {
            'line-color': '#aa0000b3',
            'line-width': 1,
            'line-dasharray': [2, 2], // Adjust the values to change the dash pattern
        },
    });

    map.addLayer({
        id: verticesLayerId,
        type: 'circle',
        source: polygonSourceId,
        filter: ['all', ['==', '$type', 'Point'], ['==', 'vertex', true]],
        paint: {
            'circle-radius': 15,
            'circle-color': '#00000000'
        }
    });

    map.addLayer({
        id: verticesLayerRenderedId,
        type: 'circle',
        source: polygonSourceId,
        filter: ['all', ['==', '$type', 'Point'], ['==', 'vertex', true]],
        paint: {
            'circle-radius': [
                'case',
                ['boolean', ['get', 'isSelected'], false],  // If isSelected is true
                7,  // Set a larger radius
                5   // Otherwise, set the default radius
            ],
            'circle-color': '#aa0000',
            'circle-stroke-width': 2,
            'circle-stroke-color': 'white'
        }
    });

    map.addLayer({
        id: additionalVerticesLayerId,
        type: 'circle',
        source: polygonSourceId,
        filter: ['all', ['==', '$type', 'Point'], ['==', 'midpoint', true]],
        paint: {
            'circle-radius': 15,
            'circle-color': '#00000000'
        }
    });

    map.addLayer({
        id: additionalVerticesLayerRenderedId,
        type: 'circle',
        source: polygonSourceId,
        filter: ['all', ['==', '$type', 'Point'], ['==', 'midpoint', true]],
        paint: {
            'circle-radius': 3,
            'circle-color': '#aa0000'
        }
    });
}

function onStartDrawingWithCoordinates(coordinates, steadId, customSteadId = null) {
    if (drawingMode) return;

    modifyingSteadId = steadId;
    modifyingCustomSteadId = customSteadId;
    drawingMode = true;
    polygonDrawn = true;
    polygonPoints.length = 0;
    polygonPoints.push(...coordinates);
    DotNet.invokeMethodAsync("FarmApp.Services", 'OnDrawingModeSwitched', true);

    addSourcesAndLayers();
    updateModifiedPolygon();
}

function getSteadEditorState() {
    if (!drawingMode) return { customSteadId: null, steadId: null, coordinates: '[]' };
    
    return { 
        customSteadId: modifyingCustomSteadId, 
        steadId: modifyingSteadId, 
        coordinates: JSON.stringify(polygonPoints) 
    };
}

function onSwitchTranslateMode(mode) {
    translateMode = mode;
}

function getPolygonPointIndex(polPoints, point) {
    for (let i = 0; i < polPoints.length; i ++) {
        const coord = polPoints[i];
        const polygonPoint = map.project(new maplibregl.LngLat(coord[0], coord[1]));

        if (isPointInsideCircle(polygonPoint, point, pointTapZoneRadius)) {
            return i;
        }
    }

    return -1;
}

function moveMapToProperty(polygonSerialized) {
    const feature = JSON.parse(polygonSerialized);
    const centroid = turf.centroid(feature);
    
    const point = centroid.geometry.coordinates;

    map.flyTo({
        center: point,
        zoom: 13,
        essential: true // animation is essential for the flyTo method
    });
}