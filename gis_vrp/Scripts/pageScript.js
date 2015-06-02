
//---------------------EVENTS------------------------
(function() {
    $('#markerMode').change(function () {
        var val = $(this).is(':checked');
        if (val != true) {
            $('input[value="depot"]').prop('checked', true);
        } else {
            $('input[value="client"]').prop('checked', true);
        }
    });

    $('input[name="clickmode"]').change(function () {
        if ($(this).val() == "client") {
            $('#markerMode').prop('checked', true);
        } else {
            $('#markerMode').prop('checked', false);
        }
    });

    
})(); 
//--------------------------------------------------



function showDialog(id) {
    var dialog = $(id).data('dialog');
    dialog.open();
    $('#dialog').css('top', '50%');
    $('#dialog').css('left', '50%');
}

function hideDialog(id) {
    $(id+' .dialog-close-button').click();
}

function newRouteOccured() {
    var slideComp = $('#slider_component');
    var slideInput = $('#slider_input');

    slideComp.attr('max', parseInt(slideComp.attr('max')) + 1);
    slideInput.attr('max', parseInt(slideInput.attr('max')) + 1);
}

$('#slider_component').change(function () {
    var value = $(this).prop('value');
    $("#slider_input").val(value);
});

$('#slider_input').change(function () {
    var value = $(this).prop('value');
    $("#slider_component").val(value);
});

$('#showHide').click(function() {
    var mode = $(this).attr('data-mode');

    var animationTime = 1200;

    if (mode == 'show') {
        $(this).attr('data-mode', 'hide');

        $(this).switchClass("primary", "success", animationTime, "easeInOutBounce");

        $('#directPanel').animate({
            top: '95%'
        }, animationTime, function () { });

        $('#showHideContent').fadeOut(animationTime / 2, function () {
            $(this).text('Pokaz').fadeIn(animationTime / 2);
        });
        
        $('#showHideIcon').switchClass("mif-arrow-down", "mif-arrow-up", animationTime, "easeInOutBounce");
    } else {
        $(this).attr('data-mode', 'show');
        $(this).switchClass("success", "primary", animationTime, "easeInOutBounce");

        $('#directPanel').animate({
            top: '40%'
        }, animationTime, function () { });
        
        $('#showHideContent').fadeOut(animationTime / 2, function () {
            $(this).text('Ukryj').fadeIn(animationTime / 2);
        });
        
        $('#showHideIcon').switchClass("mif-arrow-up", "mif-arrow-down", animationTime, "easeInOutBounce");
    }   
});