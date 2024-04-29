
$(() => {
    $('[data-toggle="tooltip"]').each(function () {
        var options = {
            html: true
        };

        if ($(this)[0].hasAttribute('data-type')) {
            options['template'] =
                '<div class="tooltip ' + $(this).attr('data-type') + '" role="tooltip">' +
                '	<div class="tooltip-arrow"></div>' +
                '	<div class="tooltip-inner"></div>' +
                '</div>';
        }

        $(this).tooltip(options);
    });

    var orderbys = orderBy.split(' ');
    var span = '#sp' + orderbys[0];
    $(span).addClass('darksalmon');

    $('tr').on('click', function (e) {
        if (e.target.localName === 'i') return;
        if (e.target.localName === 'th') return;
        if (e.target.localName === 'input') return;
        if (e.target.localName === 'span') return;

        editOsavotja($(this).find('td').find('span').html());
    });
    $('td[id^="Id_cell_"], td[id^="Eesnimi_cell_"], td[id^="Perenimi_cell_"], td[id^="Liik_cell_"], td[id^="Isikukood_cell_"], td[id^="Makseviis_cell_"], td[id^="Lisainfo_cell_"], td[id^="Yritusi_cell_"], td[id^="Loodud_cell_"], td[id^="Muudetud_cell_"]').on('mousedown', function (e) {
        var cellId = $(this).attr('id').replace('cell_', '');

        if ($('#input_' + cellId).is(':visible')) return;

        e.preventDefault();
        if (e.which == 3) {
            $('#' + cellId).hide();
            $('#span_' + cellId).show();
            $('#input_' + cellId).trigger('focus').trigger('select');

            activeElementId = cellId;
        }
    });

    $('td[id^="Id_cell_"], td[id^="Eesnimi_cell_"], td[id^="Perenimi_cell_"], td[id^="Liik_cell_"], td[id^="Isikukood_cell_"], td[id^="Makseviis_cell_"], td[id^="Lisainfo_cell_"], td[id^="Yritusi_cell_"], td[id^="Loodud_cell_"], td[id^="Muudetud_cell_"]').on('focusout', function (e) {
        var cellId = $(this).attr('id').replace('cell_', '');

        $('#span_' + cellId).hide();
        $('#' + cellId).show();
    });

    $('td[id^="Id_cell_"], td[id^="Eesnimi_cell_"], td[id^="Perenimi_cell_"], td[id^="Liik_cell_"], td[id^="Isikukood_cell_"], td[id^="Makseviis_cell_"], td[id^="Lisainfo_cell_"], td[id^="Yritusi_cell_"], td[id^="Loodud_cell_"], td[id^="Muudetud_cell_"]').on('keyup', function (e) {
        if (e.keyCode == 13) {
            Cookies.set('filter', 'yritused');
            window.location.href = getFilterHref();
        } else if (e.keyCode == 27 || e.keyCode == 9) {
            $(this).trigger('focusout');
        }
    });

    $('#osavotja-submit').on('click', function () {
        const osavotja = {
            Id: $('#osavotja-id').val(),
            Eesnimi: $('#osavotja-eesnimi').val(),
            Perenimi: $('#osavotja-perenimi').val(),
            Liik: $('#osavotja-liik').val(),
            Makseviis: $('#osavotja-makseviis').val(),
            Isikukood: $('#osavotja-isikukood').val(),
            Lisainfo: $('#osavotja-lisainfo').val()
        };

        $.ajax({
            type: 'POST',
            url: 'Osavotja/SaveOsavotja',
            dataType: 'JSON',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(osavotja)
        }).done(function (result) {
            if (result === 'OK') {
                $('#osavotja-detail').modal('hide');
                location.reload(true);
            }
            else {
                $('#osavotja-error').html(result);
            }
        });
    });
    $('#yesno-submit').on('click', function () {
        window.location.href = deleteUrl + '?Id=' + osavotjaId + '&pageNr=' + pageNr;
    })
});
function editOsavotja(osavotjaid) {
    const options = { options: { backdrop: true, keyboard: true, focus: true, show: true } };

    emptyosavotjaform();

    $.ajax({
        type: 'GET',
        url: 'Osavotja/GetOsavotja',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: {
            'Id': osavotjaid
        }
    }).done(function (result) {
        $('#osavotja-id').val(result.id);
        $('#osavotja-eesnimi').val(result.eesnimi);
        $('#osavotja-perenimi').val(result.perenimi);
        $('#osavotja-liik').val(result.liik);
        $('#osavotja-makseviis').val(result.makseviis);
        $('#osavotja-isikukood').val(result.isikukood);
        $('#osavotja-lisainfo').val(result.lisainfo);
    });

    $('#osavotja-detail-title').html('OSAVÕTJA DETAILVAADE');

    $('#osavotja-detail').modal(options).modal('show');
}
function emptyosavotjaform() {
    $('#osavotja-eesnimi').val('');
    $('#osavotja-perenimi').val('');
    $('#osavotja-liik').val('');
    $('#osavotja-makseviis').val('');
    $('#osavotja-isikukood').val('');
    $('#osavotja-lisainfo').val('');
    $('#osavotja-error').html('');
}
function kustutaOsavotja(id) {
    const options = { options: { backdrop: true, keyboard: true, focus: true, show: true } };

    emptyyesno();
    osavotjaId = id;

    $('#yesnomodallabel').html('Osavõtja kustutamine');
    $('#yesnomodalcontent').html('Kas kustutada osavõtja ' + id + '?');

    $('#yesnomodal').modal(options).modal('show');
}
function emptyyesno() {
    $('#yesnomodallabel').html('');
    $('#yesnomodalcontent').html('');
}
function getFilterHref() {
    var href = window.location.href.indexOf('?') != -1 ? window.location.href.split('?')[0] : window.location.href;
    var filterfield = $('#model_filterfield').val();
    var filtervalue = $('#model_filtervalue').val();

    filterfield = filterfield == '' ? activeElementId : filterfield + ';' + activeElementId;
    filtervalue = filtervalue == '' ? $('#' + 'input_' + activeElementId).val() : filtervalue + ';' + $('#' + 'input_' + activeElementId).val();

    return href + '?' + 'orderby=' + $('#model_orderby').val() + '&' + 'filterfield=' + filterfield + '&' + 'filtervalue=' + filtervalue + '&' + 'p=' + '1' + '&' + 's=' + $('#model_pagesize').val();
}
