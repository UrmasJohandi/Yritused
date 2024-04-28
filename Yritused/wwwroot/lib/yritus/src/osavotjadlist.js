
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
