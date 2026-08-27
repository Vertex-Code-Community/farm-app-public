const data = {
    "data": [
        {
            "location": [2.35, 48.85], // Paris, France (Center location)
            "vertices": [
                [2.33032, 48.84492],
                [2.46976, 48.84492],
                [2.46976, 48.90167],
                [2.33032, 48.90167],
                [2.33032, 48.84492]
            ],
            "color": "blue"
        },
        {
            "location": [13.4050, 52.5200], // Berlin, Germany (Center location)
            "vertices": [
                [13.0894, 52.4224],
                [13.0894, 52.5622],
                [13.7687, 52.5622],
                [13.7687, 52.4224],
                [13.0894, 52.4224]
            ],
            "color": "green"
        }
    ]
};

function initMap() {

    // mapboxgl.accessToken = 'YOUR-MAPBOX-ACCESS-TOKEN-HERE';

    // const map = new mapboxgl.Map({
    //     container: 'map-id', // container ID
    //     style: 'mapbox://styles/mapbox/streets-v12', // style URL
    //     center: [2.35, 48.85], // starting position [lng, lat]
    //     zoom: 9, // starting zoom
    // });

    const map = new maplibregl.Map({
        container: 'map-id', // container id
        // style: 'https://demotiles.maplibre.org/style.json', // style URL
        style: 'https://api.maptiler.com/maps/streets/style.json?key=get_your_own_OpIi9ZULNHzrESv6T2vL', // style URL
        // center: [0, 0], // starting position [lng, lat]
        // zoom: 1 // starting zoom

        // zoom: 10,
        // center: [-87.622088, 41.878781]        

        zoom: 16,
        center: [30.3488542385979, 50.5039335292147]
    });

    // const coordinates = [
    //     [-42.0, -18.0], // Vertex 1
    //     [-38.0, -18.0], // Vertex 2
    //     [-40.0, -22.0]  // Vertex 3
    // ];
    // map.on('load', function () {
    //     map.addLayer({
    //         id: 'polygon',
    //         type: 'fill',
    //         source: {
    //             type: 'geojson',
    //             data: {
    //                 type: 'Feature',
    //                 geometry: {
    //                     type: 'Polygon',
    //                     coordinates: [coordinates]
    //                 }
    //             }
    //         },
    //         paint: {
    //             'fill-color': 'red', // Triangle color (red in this case)
    //             'fill-opacity': 1.0  // Opacity (0 to 1)
    //         }
    //     });
    // });

    map.on('load', function () {

        // map.addSource('mapillary', {
        //     'type': 'vector',
        //     'tiles': [
        //         'https://tiles.mapillary.com/maps/vtp/mly1_public/2/{z}/{x}/{y}?access_token=YOUR-MAPILLARY-ACCESS-TOKEN-HERE'
        //     ],
        //     'minzoom': 6,
        //     'maxzoom': 14
        // });
        //
        // map.addLayer(
        //     {
        //         'id': 'mapillary', // Layer ID
        //         'type': 'line',
        //         'source': 'mapillary', // ID of the tile source created above
        //         // Source has several layers. We visualize the one with name 'sequence'.
        //         'source-layer': 'sequence',
        //         'layout': {
        //             'line-cap': 'round',
        //             'line-join': 'round'
        //         },
        //         'paint': {
        //             'line-opacity': 0.6,
        //             'line-color': 'rgb(53, 175, 109)',
        //             'line-width': 2
        //         }
        //     }
        // );

        map.addSource('test-polygon-points-source', {
            'type': 'geojson',
            'data': {
                'type': 'FeatureCollection',
                'features': [
                    // {
                    //     'type': 'Feature',
                    //     'geometry': {
                    //         'type': 'Polygon',
                    //         'coordinates': [
                    //             [
                    //                 [30.3488542385979, 50.5039335292147],
                    //                 [30.349015935349, 50.5039420994819],
                    //                 [30.3490249185019, 50.5038992481304],
                    //                 [30.3488587301743, 50.5038906778554],
                    //                 [30.3488542385979, 50.5039335292147]
                    //             ]
                    //         ]
                    //     }
                    // },

                    {
                        'type': 'Feature',
                        'geometry': {
                            'type': 'Point',
                            'coordinates': [30.3488542385979, 50.5039335292147]
                        }
                    },

                    {
                        'type': 'Feature',
                        'geometry': {
                            'type': 'Point',
                            'coordinates': [30.349015935349, 50.5039420994819]
                        }
                    },

                    {
                        'type': 'Feature',
                        'geometry': {
                            'type': 'Point',
                            'coordinates': [30.3490249185019, 50.5038992481304]
                        }
                    },

                    {
                        'type': 'Feature',
                        'geometry': {
                            'type': 'Point',
                            'coordinates': [30.3488587301743, 50.5038906778554]
                        }
                    },
                ]
            }
        });



        map.addSource('stead-polygons-source', {
            'type': 'vector',
            'tiles': [
                'https://YOUR-VECTOR-TILES-HOST-HERE/farm-app-vector-tiles/{z}/tile_{z}_{x}_{y}.mvt'
            ],
            'minzoom': 6,
            'maxzoom': 17
        });

        map.addLayer({
            'id': 'park-volcanoes',
            'type': 'circle',
            // 'source': 'marker-points-source',
            'source': 'test-polygon-points-source',
            // 'source-layer': 'marker-points-layer',
            'paint': {
                'circle-radius': 6,
                'circle-color': '#B42222'
            },
            'filter': ['==', '$type', 'Point']
        });

        map.addLayer({
            'id': 'stead-polygons-id',
            'type': 'fill',
            'source': 'stead-polygons-source',
            'source-layer': 'stead-polygons-layer',
            'paint': {
                'fill-color': '#888888',
                'fill-opacity': 0.4
            },
            'filter': ['==', '$type', 'Polygon']
        });

        map.on('zoom', (e) => {
            // console.log('event:', e);

            const zoom = map.getZoom();
            console.log('zoom:', zoom);

            // console.log('event type:', e.type);
        });
    });

    // map.addControl(new mapboxgl.NavigationControl());
}

// function setMapStyle(styleIndex) {
//     // styleChanged = true;
//     // map.setStyle(mapStyleUrls[styleIndex]);
// }
//
// let styleChanged = false;
//
// function checkStyleStatus() {
//     if (!styleChanged) return;
//
//     if (map.isStyleLoaded()) {
//         styleChanged = false;
//         map.fire('styles_finally_loaded');
//     } else {
//         setTimeout(function () {
//             checkStyleStatus();
//         }, 200);
//     }
// }

// map.on('styledata', function (e) {
//     if (map.isStyleLoaded()) return;
//     checkStyleStatus();
// });
//
// map.on('styles_finally_loaded', function (e) {
//     console.log('styles_finally_loaded');
//     addSourcesAndLayers();
// });

// function updateSteadLayersPaints() {
//     map.setPaintProperty(steadPolygonsLayerId, 'fill-opacity', [
//         'case',
//         [
//             'any',
//             // ['boolean', ['feature-state', 'hover'], false],
//             ['in', ['get', 'SteadId'], ['literal', [...polygonSteadIds]]]
//         ],
//         0.8,
//         0.1
//     ]);
//
//     map.setPaintProperty(customSteadPolygonsLayerId, 'fill-opacity', [
//         'case',
//         ['in', ['get', 'customSteadId'], ['literal', [...polygonCustomSteadIds]]],
//         0.8,
//         0.1
//     ]);
// }

// map.addLayer({
//     id: 'custom-webgl-layer',
//     type: 'custom',
//     renderingMode: '3d',
//     onAdd: function (map, gl) {
//         const vertexSource = `#version 300 es
//
//             uniform mat4 u_matrix;
//             in vec2 a_pos;
//             void main() {
//                 gl_PointSize = 50.0;
//                 gl_Position = u_matrix * vec4(a_pos, 0.0, 1.0);
//             }`;
//
//         // create GLSL source for fragment shader
//         const fragmentSource = `#version 300 es
//
//             out highp vec4 fragColor;
//             void main() {
//                 fragColor = vec4(1.0, 0.0, 0.0, 1.0);
//             }`;
//
//         // create a vertex shader
//         const vertexShader = gl.createShader(gl.VERTEX_SHADER);
//         gl.shaderSource(vertexShader, vertexSource);
//         gl.compileShader(vertexShader);
//
//         // create a fragment shader
//         const fragmentShader = gl.createShader(gl.FRAGMENT_SHADER);
//         gl.shaderSource(fragmentShader, fragmentSource);
//         gl.compileShader(fragmentShader);
//
//         // link the two shaders into a WebGL program
//         this.program = gl.createProgram();
//         gl.attachShader(this.program, vertexShader);
//         gl.attachShader(this.program, fragmentShader);
//         gl.linkProgram(this.program);
//
//         this.aPos = gl.getAttribLocation(this.program, 'a_pos');
//
//         // define vertices of the triangle to be rendered in the custom style layer
//         const helsinki = maplibregl.MercatorCoordinate.fromLngLat({
//             lng: 25.004,
//             lat: 60.239
//         });
//         const berlin = maplibregl.MercatorCoordinate.fromLngLat({
//             lng: 13.403,
//             lat: 52.562
//         });
//         const kyiv = maplibregl.MercatorCoordinate.fromLngLat({
//             lng: 30.498,
//             lat: 50.541
//         });
//
//         // create and initialize a WebGLBuffer to store vertex and color data
//         this.buffer = gl.createBuffer();
//         gl.bindBuffer(gl.ARRAY_BUFFER, this.buffer);
//         gl.bufferData(
//             gl.ARRAY_BUFFER,
//             new Float32Array([
//                 helsinki.x,
//                 helsinki.y,
//                 berlin.x,
//                 berlin.y,
//                 kyiv.x,
//                 kyiv.y
//             ]),
//             gl.STATIC_DRAW
//         );
//     },
//     render: function (gl, matrix) {
//         gl.useProgram(this.program);
//         gl.uniformMatrix4fv(
//             gl.getUniformLocation(this.program, 'u_matrix'),
//             false,
//             matrix
//         );
//         gl.bindBuffer(gl.ARRAY_BUFFER, this.buffer);
//         gl.enableVertexAttribArray(this.aPos);
//         gl.vertexAttribPointer(this.aPos, 2, gl.FLOAT, false, 0, 0);
//         gl.enable(gl.BLEND);
//         gl.blendFunc(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA);
//         gl.drawArrays(gl.POINTS, 0, 3);
//     }
// });