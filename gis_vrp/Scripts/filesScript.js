

(function() {
    document.getElementById('files').addEventListener('change', handleFileSelect, false);
})();

function convertToISODate (date) {
    var result = new Date(date);
    result = result.toISOString().replace('Z', '');
    return result;
}

function handleFileSelect(evt) {
    var file = evt.target.files[0];
    var data = new FormData();
    data.append("UploadedFile", file);

    $.ajax({
        type: 'POST',
        contentType: false,
        processData: false,
        data: data,
        url: getPointsAction, // dodać zmienną
        success: function (data) {
            data = JSON.parse(data);
            for (var i = 0; i < data.length; i++) {
                data[i].OccurTime = convertToISODate(data[i].OccurTime);
                addPoint(data[i]);
            }
        }
    });
}

function exportPoints() {
    $.ajax({
        type: 'POST',
        contentType: false,
        processData: false,
        data: null,
        url: exportPointsAction,
        success: function (data) {
            
        }
    });
}

function addPointsToForm(points) {
    $('#exportedPoints').val(JSON.stringify(points));
}

