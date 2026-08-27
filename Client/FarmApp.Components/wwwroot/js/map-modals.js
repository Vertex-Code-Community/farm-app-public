let mapModalInfo = null;
let mapModalPosition = [];
let updateModalToken = null;

function closeMapModal() {
    updateModalToken = null;
    
    if (mapModalInfo) {
        mapModalInfo.remove();
    }
}

function setModalPosition(position) {
    mapModalPosition = position;
}

function showMapModal(html, updateToken = null) {
    closeMapModal();
    updateModalToken = updateToken;
    
    mapModalInfo = new maplibregl.Popup()
        .setLngLat(mapModalPosition)
        .setHTML(html)
        .addTo(map);
}

function updateModalHtml(template, updateToken = null) {
    if (mapModalInfo && updateModalToken === updateToken) {
        mapModalInfo.setHTML(template);
    }
}