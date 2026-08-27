
function getMultiPolygonPictogram(polygonPixelCoordinates) {
    const base64 = getPictogramOfMultiPolygon(polygonPixelCoordinates);
    
    if (base64) {
        const blob = base64ToBlob(base64);
        return URL.createObjectURL(blob);
    }
    
    return null;
}

function getPictogramOfMultiPolygon(polygonList, width = 400, height = 400) {
    const canvas = document.getElementById('farm-app-pictogram-canvas');

    const ctx = canvas.getContext('2d');

    const transformedPolygonList = transformMultiPolygonCoordinates(polygonList, width, height);
    const transformedCoordinatesList = transformedPolygonList.flatMap(p => p);

    const backgroundColor = getCssVar('--surface-cards');

    canvas.width = Math.max(...transformedCoordinatesList.map(coord => coord[0]));
    canvas.height = Math.max(...transformedCoordinatesList.map(coord => coord[1]));

    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.fillStyle = backgroundColor;
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    for (const transformedCoordinates of transformedPolygonList) {
        // Draw a polygon on the canvas
        ctx.beginPath();
        ctx.moveTo(transformedCoordinates[0][0], transformedCoordinates[0][1]);

        for (let i = 1; i < transformedCoordinates.length; i++) {
            ctx.lineTo(transformedCoordinates[i][0], transformedCoordinates[i][1]);
        }

        ctx.closePath();
        ctx.fillStyle = '#FF99004D'; // Polygon fill color
        ctx.fill();

        ctx.strokeStyle = '#FF9900'; // Example: red border color
        ctx.lineWidth = 6; // Set the line width of the border
        ctx.stroke(); // Draw the border
    }

    return canvas.toDataURL('image/jpeg', 0.95);
}

function transformMultiPolygonCoordinates(polygonList, boundingBoxWidth, boundingBoxHeight) {
    const coordinates = polygonList.flatMap(p => p);
    
    const minX = Math.min(...coordinates.map(coord => coord[0]));
    const minY = Math.min(...coordinates.map(coord => coord[1]));

    // Scale the coordinates
    const scaleFactor = Math.min(boundingBoxWidth / (Math.max(...coordinates.map(coord => coord[0])) - minX),
        boundingBoxHeight / (Math.max(...coordinates.map(coord => coord[1])) - minY));

    return polygonList.map(p => p.map(coord => [
        (coord[0] - minX) * scaleFactor,
        (coord[1] - minY) * scaleFactor
    ]));
}


// ____________________________________________________________________________________________________________________
// ____________________________________________________________________________________________________________________
// ____________________________________________________________________________________________________________________

function getPropertiesPictograms(pixelCoordinates) {
    const pictograms = [];
    
    for (const propertyCoordinates of pixelCoordinates) {
        if (propertyCoordinates) {
            const base64 = getPictogramOfPolygon(propertyCoordinates);
            if (base64) {
                const blob = base64ToBlob(base64);
                const url = URL.createObjectURL(blob);
                
                pictograms.push(url);
            } else {
                pictograms.push(null);
            }
        } else {
            pictograms.push(null);
        }
    }
    
    return pictograms;
}

function getPictogramOfPolygon(propertyCoordinates, width = 400, height = 400) {
    const canvas = document.getElementById('farm-app-pictogram-canvas');
    console.log(canvas)
    
    const ctx = canvas.getContext('2d');

    // console.log('getPictogramOfPolygon', JSON.stringify(propertyCoordinates));
    
    const transformedCoordinates = transformCoordinates(propertyCoordinates, width, height);

    canvas.width = Math.max(...transformedCoordinates.map(coord => coord[0]));
    canvas.height = Math.max(...transformedCoordinates.map(coord => coord[1]));

    const backgroundColor = getCssVar('--surface-cards');

    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.fillStyle = backgroundColor;
    ctx.fillRect(0, 0, canvas.width, canvas.height);
    
    // Draw a polygon on the canvas
    ctx.beginPath();
    ctx.moveTo(transformedCoordinates[0][0], transformedCoordinates[0][1]);
    
    for (let i = 1; i < transformedCoordinates.length; i++) {
        ctx.lineTo(transformedCoordinates[i][0], transformedCoordinates[i][1]);
    }
    
    ctx.closePath();
    ctx.fillStyle = '#FF99004D'; // Polygon fill color
    ctx.fill();

    ctx.strokeStyle = '#FF9900'; // Example: red border color
    ctx.lineWidth = 6; // Set the line width of the border
    ctx.stroke(); // Draw the border

    return canvas.toDataURL('image/jpeg', 0.95);
}

function transformCoordinates(coordinates, boundingBoxWidth, boundingBoxHeight) {
    const minX = Math.min(...coordinates.map(coord => coord[0]));
    const minY = Math.min(...coordinates.map(coord => coord[1]));

    // Scale the coordinates
    const scaleFactor = Math.min(boundingBoxWidth / (Math.max(...coordinates.map(coord => coord[0])) - minX),
        boundingBoxHeight / (Math.max(...coordinates.map(coord => coord[1])) - minY));

    const scaledCoordinates = coordinates.map(coord => [
        (coord[0] - minX) * scaleFactor,
        (coord[1] - minY) * scaleFactor
    ]);

    return scaledCoordinates.map(coord => [
        coord[0],
        coord[1]
    ]);
}

function base64ToBlob(base64, mimeType) {
    let binary = atob(base64.split(',')[1]);

    let array = [];
    for(let i = 0; i < binary.length; i++) {
        array.push(binary.charCodeAt(i));
    }

    return new Blob([new Uint8Array(array)], {type: mimeType});
}

function getCssVar(name) {
    return getComputedStyle(document.body)
        .getPropertyValue(name)
        .trim();
}