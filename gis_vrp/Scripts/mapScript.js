
//---------GLOBAL VARIABLE---------------------
var map = null;
var customMarker = "../Styles/Images/marker.png";
var points = [];
var markers = [];
var geocoder = new window.google.maps.Geocoder();
var directionsDisplay = new window.google.maps.DirectionsRenderer();
var directionsService = new window.google.maps.DirectionsService();
var drawRouteResult = [];
var resultRoute;
var lock;

var drawingManager;
var isGenerationEnabled = false;
var numberOfPointsToGenerate;
//-----------------------------------------------




//-----------------EVENTS------------------------

(function () {
    window.google.maps.event.addDomListener(window, 'load', mapInitialize);

    $(document).on('change', '[client-id]', function () {
        var id = $(this).attr('client-id');
        var capacity = $('input.capacity[client-id="' + id + '"]').val();
        var occurTime = $('input.occur-time[client-id="' + id + '"]').val();
        var unloadTime = $('input.unload-time[client-id="' + id + '"]').val();

        points[id].Capacity = capacity;
        points[id].OccurTime = occurTime;
        points[id].UnloadTime = unloadTime;
    });
    $('#exportBtn').click(function (evt) {
        addPointsToForm(points);
        $('#exPointsForm').submit();
    });

    $(document).on('change', '#slider_input', function() {
        routesDr[$(this).val() - 1].draw();
    });

    $(document).on('change', '#slider_component', function () {
        routesDr[$(this).val() - 1].draw();
    });
    $("#generationForm").submit(function (e) {
        numberOfPointsToGenerate = $("#pointsNumberToGenerateInput").val();
        hideDialog('#generateDialog');
        $("#pointsNumberToGenerateInput").val("");
        toggleGeneration();
        return false;
    });
})();

//-----------------------------------------------




//---------------MAP START-----------------------

function mapInitialize() {
    var mapOptions = {
        zoom: 11,
        center: new window.google.maps.LatLng(52.222, 21.014)
    };

    this.map = new window.google.maps.Map(document.getElementById('map-canvas'), mapOptions);

    window.google.maps.event.addListener(map, 'click', addMarkerEventHandler);
    directionsDisplay.setMap(map);

    initDravingManager();

    window.google.maps.event.addListener(drawingManager, 'overlaycomplete', randomizePoints);

}
//-----------------------------------------------



//--------------MAP EVENT HANDLERS---------------

function addMarkerEventHandler(e) {
    var clientModeIsSelected = $('input[value="client"]').is(':checked');
    var point = {
        Lat: e.latLng.A,
        Lng: e.latLng.F,
        Capacity: null,
        OccurTime: null,
        UnloadTime: null,
        Type: clientModeIsSelected ? 0 : 1
    };
    addPoint(point);
}
//-----------------------------------------------



//------------------GENERATE---------------------
function toggleGeneration() {
    isGenerationEnabled = !isGenerationEnabled;
    if (isGenerationEnabled) {
        //numberOfPointsToGenerate = prompt("Type number of points to generate and draw a rectangle by dragging.", "");
        drawingManager.setDrawingMode(window.google.maps.drawing.OverlayType.RECTANGLE);
    } else
        drawingManager.setDrawingMode(null);
}
function initDravingManager() {
    drawingManager = new window.google.maps.drawing.DrawingManager({
        drawingMode: window.google.maps.drawing.OverlayType.MARKER,
        drawingControl: true,
        drawingControlOptions: {
            position: window.google.maps.ControlPosition.TOP_CENTER,
            drawingModes: [
                //window.google.maps.drawing.OverlayType.CIRCLE,
                //window.google.maps.drawing.OverlayType.POLYGON,
                window.google.maps.drawing.OverlayType.RECTANGLE
            ]
        },
        rectangleOptions: {
            clickable: false,
        }
    });
    drawingManager.setMap(map);
    drawingManager.setOptions({
        drawingControl: false,
    });
    drawingManager.set('drawingMode', null);
}

function randomizePoints(e) {
    if (e.type == window.google.maps.drawing.OverlayType.RECTANGLE) {
        toggleGeneration();
        var bounds = e.overlay.getBounds();

        var latMin = bounds.getSouthWest().lat();
        var latRange = bounds.getNorthEast().lat() - latMin;
        var lngMin = bounds.getSouthWest().lng();
        var lngRange = bounds.getNorthEast().lng() - lngMin;
        var point;
        var point2;
        if (numberOfPointsToGenerate > 0){
            point = new window.google.maps.LatLng(
               latMin + (Math.random() * latRange),
               lngMin + (Math.random() * lngRange)
               );
            //addMarker(point, 1, 0);
            point2 = { Lat: point.A, Lng: point.F, Type: 1 };
            addPoint(point2);
            console.log(point + " generated");
        }
        for (var i = 1; i < numberOfPointsToGenerate; i++) {
            point = new window.google.maps.LatLng(
                latMin + (Math.random() * latRange),
                lngMin + (Math.random() * lngRange)
                );
            //addMarker(point, 0, i);
            point2 = { Lat: point.A, Lng: point.F, Type: 0 };
            addPoint(point2);
            console.log(point + " generated");
        }
    }
    e.overlay.setMap(null);
}
//-----------------------------------------------



//------------------ADD MARKER-------------------

function addPoint(point) {
    var latlng = { lat: point.Lat, lng: point.Lng };
    var id = points.length;
    addMarker(latlng, point.Type, id);
    point.Capacity = 40;
    point.OccurTime = "2015-01-01T00:00";
    point.UnloadTime = 12;
    points.push(point);
}

function addMarker(latLng, type, id) {
    var marker;
    if (type === 1) {
        marker = new window.google.maps.Marker({
            position: latLng,
            map: map
        });
    } else {
        marker = new window.google.maps.Marker({
            position: latLng,
            map: map,
            icon: customMarker
        });
    }

    markers.push(marker);

    geocoder.geocode({ 'latLng': latLng }, function (results, status) {
        if (status === window.google.maps.GeocoderStatus.OK) {
            if (results[0]) {
                var infowindow = buildInfoWindow(results[0].formatted_address, type, id);

                window.google.maps.event.addListener(marker, 'click', function () {
                    infowindow.open(map, marker);
                    var point = points[id];
                    $('input.capacity[client-id="' + id + '"]').val(point.Capacity);
                    $('input.occur-time[client-id="' + id + '"]').val(point.OccurTime);
                    $('input.unload-time[client-id="' + id + '"]').val(point.UnloadTime);
                });
            }
        }
    });
}
//-----------------------------------------------





//---------------Info Window-------------------
function buildInfoWindow(title, mode, id) {

    if (mode === 0) { //tworz dla klienta info window
        return new window.google.maps.InfoWindow({
            content: '<div>' +
                '<h3 style="color:green;">Klient</h3>' +
                '<h5>' + title + '</h5>' +
                '<div style="min-width: 460px;">' +
                    '<div class="grid">'+
                    '<div class="row cells5">'+
                        '<div class="cell colspan5">'+
                            '<label class="custom-gmaps-label">Pojemność</label>'+
                            '<div class="input-control text warning">'+
                                '<input type="number" min="1" value="1" class="capacity info-window-input" client-id="' + id + '" />'+
                            '</div>'+
                        '</div>'+
                    '</div>'+
                    '<div class="row cells5">'+
                        '<div class="cell colspan5">'+
                            '<label class="custom-gmaps-label">Czas pojawienia się </label>'+
                            '<div class="input-control text warning">'+
                                '<input type="datetime-local" value="2015-05-10T10:39:57" class="occur-time info-window-input" client-id="' + id + '" />'+
                           '</div>'+
                        '</div>'+
                    '</div>'+
                    '<div class="row cells5">'+
                        '<div class="cell colspan5">'+
                            '<label class="custom-gmaps-label">Czas rozładunku</label>'+
                            '<div class="input-control text warning">'+
                                '<input type="number" min="1" value="1" class="unload-time info-window-input" client-id="' + id + '" />'+
                            '</div>'+
                        '</div>'+
                    '</div>'+
                '</div>' +
                '</div>' +
                '</div>'
        });
    } else {
        return new window.google.maps.InfoWindow({
            content: '<div>' +
                '<h3 style="color:red;">Depozyt</h3>' +
                '<h5>' + title + '</h5>' +
                '</div>'
        });
    }
}

function validatePoints(points) {
    var check = false;
    var incorrectClientDataCount = 0;
    var depotCount = 0, clientCount = 0;
    for (var i = 0; i < points.length; i++) {
        if (points[i].Type === 0) {
            clientCount++;
            var capacity = points[i].Capacity;
            var occurTime = points[i].OccurTime;
            var unloadTime = points[i].UnloadTime;
            if (capacity == undefined || capacity === '' ||
                occurTime == undefined || occurTime === '' ||
                unloadTime == undefined || unloadTime === '') {
                window.google.maps.event.trigger(markers[i], 'click');
                check = true;
                incorrectClientDataCount++;
            }
        } else if (points[i].Type === 1) {
            depotCount++;
        }
    }

    if (check) {
        alert('Wypełnij poprawnie wszystkie pola, liczba depotów z niepoprawnie uzupełnionymi danymi: ' + incorrectClientDataCount);
        return false;
    }
    if (depotCount === 0) {
        alert("Na mapie musi być zaznaczony przynajmniej jeden depozyt");
        return false;
    }
    if (clientCount === 0) {
        alert("Na mapie musi być zaznaczony przynajmniej jeden klient");
        return false;
    }
    return true;
}
//---------------------------------------------


function showNotify() {
    $('#spinner').css('display', 'none');
    $.Notify({
        caption: 'Znaleziono nową trase',
        content: 'Możesz ja zobczyć przesuwając suwak',
        shadow: true,
        type: "success",
        timeout: 6000,
    });

    for (var i = 0; i < markers.length; i++) {
        markers[i].setMap(null);
    }
}