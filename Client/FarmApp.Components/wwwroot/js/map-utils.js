function findCenter(coordinates) {
    let sumLng = 0;
    let sumLat = 0;

    for (const coord of coordinates) {
        sumLng += coord[0];
        sumLat += coord[1];
    }

    const centerLng = sumLng / coordinates.length;
    const centerLat = sumLat / coordinates.length;

    return new maplibregl.LngLat(centerLng, centerLat);
}

function getSteadFeatureByEvent(map, event) {
    const features = map.queryRenderedFeatures(event.point);

    const steadFeatures = features.filter(x => x.layer && x.layer.id === steadPolygonsLayerId);
    if (steadFeatures.length === 0) return null;

    return steadFeatures[0];
}

function getFeatureByLocation(map, point, layerId) {
    const features = map.queryRenderedFeatures(point);

    const filteredFeatures = features.filter(x => x.layer && x.layer.id === layerId);
    if (filteredFeatures.length === 0) return null;

    return filteredFeatures[0];
}

function printObjectFields(o) {
    if(!o) return;

    for (let key in o) {
        console.log(key + " : " + o[key]);
    }
}

function getSquare(center, size) {
    const zoom = map.getZoom();
    const boundingBox = calculateBoundingBox(center, size, zoom);

    return [
        [boundingBox[0][0], boundingBox[0][1]],
        [boundingBox[1][0], boundingBox[0][1]],
        [boundingBox[1][0], boundingBox[1][1]],
        [boundingBox[0][0], boundingBox[1][1]],
        [boundingBox[0][0], boundingBox[0][1]],
    ];
}

function getSquare(center, size, zoom) {
    const boundingBox = calculateBoundingBox(center, size, zoom);

    return [
        [boundingBox[0][0], boundingBox[0][1]],
        [boundingBox[1][0], boundingBox[0][1]],
        [boundingBox[1][0], boundingBox[1][1]],
        [boundingBox[0][0], boundingBox[1][1]],
        [boundingBox[0][0], boundingBox[0][1]],
    ];
}

function calculateBoundingBox(center, size, zoom) {
    const halfSize = size / 2;

    // Convert size from pixels to degrees of latitude and longitude
    const lngLatSize = halfSize * (360 / (Math.pow(2, zoom) * 512));

    // Adjust for the aspect ratio at the given center point
    const aspectRatio = Math.cos((center[1] * Math.PI) / 180.0);
    const lngSize = lngLatSize / aspectRatio;

    return [
        [center[0] - lngSize, center[1] + lngLatSize],
        [center[0] + lngSize, center[1] - lngLatSize],
    ];
}

function calculateMedian(point1, point2) {
    // Calculate the median point between two points
    return [
        (point1[0] + point2[0]) / 2,
        (point1[1] + point2[1]) / 2
    ];
}

function calculateMedianPoints(pointsArray) {
    // Check if the array has at least two points
    if (pointsArray.length < 2) {
        throw new Error('At least two points are required');
    }

    const medianPoints = [];

    // Calculate median points for each pair of consecutive points
    for (let i = 0; i < pointsArray.length - 1; i++) {
        const currentPoint = pointsArray[i];
        const nextPoint = pointsArray[i + 1];
        const medianPoint = calculateMedian(currentPoint, nextPoint);
        medianPoints.push(medianPoint);
    }

    // Calculate and add the median point between the last and first points
    const firstPoint = pointsArray[0];
    const lastPoint = pointsArray[pointsArray.length - 1];
    const medianPointFirstLast = calculateMedian(lastPoint, firstPoint);
    medianPoints.push(medianPointFirstLast);

    return medianPoints;
}

function combineGeometries(features) {
    let combinedGeometry = features[0];

    for (let i = 1; i < features.length; i++) {
        combinedGeometry = turf.union(combinedGeometry, features[i]);
    }

    return combinedGeometry;
}

function combineGeometriesFromSerializedFeatures(serializedFeatures) {
    const features = serializedFeatures.map(x => JSON.parse(x));
    let combinedGeometry = features[0];

    for (let i = 1; i < features.length; i++) {
        combinedGeometry = turf.union(combinedGeometry, features[i]);
    }

    const bufferedMultiPolygon = turf.buffer(combinedGeometry, 3, 'meters');
    return JSON.stringify(bufferedMultiPolygon);
}

function isPointInsideCircle(center, point, radius) {
    const xDiff = point.x - center.x;
    const yDiff = point.y - center.y;
    
    const distanceSquared = xDiff * xDiff + yDiff * yDiff;
    
    return distanceSquared <= radius * radius;
}