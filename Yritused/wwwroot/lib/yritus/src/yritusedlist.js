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

        editYritus($(this).find('td').find('span').html());
    });
    $('td[id^="Id_cell_"], td[id^="YrituseNimi_cell_"], td[id^="YrituseAeg_cell_"], td[id^="YrituseKoht_cell_"], td[id^="Lisainfo_cell_"], td[id^="Osavotjaid_cell_"], td[id^="Loodud_cell_"], td[id^="Muudetud_cell_"]').on ('mousedown', function (e) {
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

    $('td[id^="Id_cell_"], td[id^="YrituseNimi_cell_"], td[id^="YrituseAeg_cell_"], td[id^="YrituseKoht_cell_"], td[id^="Lisainfo_cell_"], td[id^="Osavotjaid_cell_"], td[id^="Loodud_cell_"], td[id^="Muudetud_cell_"]').on('focusout', function (e) {
        var cellId = $(this).attr('id').replace('cell_', '');

        $('#span_' + cellId).hide();
        $('#' + cellId).show();
    });

    $('td[id^="Id_cell_"], td[id^="YrituseNimi_cell_"], td[id^="YrituseAeg_cell_"], td[id^="YrituseKoht_cell_"], td[id^="Lisainfo_cell_"], td[id^="Osavotjaid_cell_"], td[id^="Loodud_cell_"], td[id^="Muudetud_cell_"]').on('keyup', function (e) {
        if (e.keyCode == 13) {
            Cookies.set('filter', 'yritused');
            window.location.href = getFilterHref();
        } else if (e.keyCode == 27 || e.keyCode == 9) {
            $(this).trigger('focusout');
        }
    });

    $('#yritus-submit').on('click', function () {
        var error = false;

        if ($('#yritus-yritusenimi').val() === '') {
            $('#yritus-yritusenimi').removeClass('is-valid').addClass('is-invalid');

            error = true;
        }

        if ($('#yritus-yrituseaeg').val() === '') {
            $('#yritus-yrituseaeg').removeClass('is-valid').addClass('is-invalid');

            error = true;
        }

        if ($('#yritus-yritusekoht').val() === '') {
            $('#yritus-yritusekoht').removeClass('is-valid').addClass('is-invalid');

            error = true;
        }

        if (error) return;

        const yritus = {
            Id: $('#yritus-id').val(),
            YrituseNimi: $('#yritus-yritusenimi').val(),
            YrituseAeg: formatdatetimeback($('#yritus-yrituseaeg').val()),
            YrituseKoht: $('#yritus-yritusekoht').val(),
            Lisainfo: $('#yritus-lisainfo').val()
        };

        $.ajax({
            type: 'POST',
            url: 'Yritus/SaveYritus',
            dataType: 'JSON',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(yritus)
        }).done(function (result) {
            if (result === 'OK') {
                $('#yritus-detail').modal('hide');
                location.reload(true);
            }
            else {
                $('#yritus-error').html(result);
            }
        });
    });
    $('#yesno-submit').on('click', function () {
        window.location.href = deleteUrl + '?Id=' + yritusId + '&pageNr=' + pageNr;
    })
    $('#yritus-yritusenimi').on('blur', function () {
        if ($('#yritus-yritusenimi').val() !== '') {
            $('#yritus-yritusenimi').removeClass('is-invalid').addClass('is-valid');
        } else {
            $('#yritus-yritusenimi').removeClass('is-valid').addClass('is-invalid');
        }
    })
    $('#yritus-yrituseaeg').on('blur', function () {
        if ($('#yritus-yrituseaeg').val() !== '') {
            $('#yritus-yrituseaeg').removeClass('is-invalid').addClass('is-valid');
        } else {
            $('#yritus-yrituseaeg').removeClass('is-valid').addClass('is-invalid');
        }
    })
    $('#yritus-yritusekoht').on('blur', function () {
        if ($('#yritus-yritusekoht').val() !== '') {
            $('#yritus-yritusekoht').removeClass('is-invalid').addClass('is-valid');
        } else {
            $('#yritus-yritusekoht').removeClass('is-valid').addClass('is-invalid');
        }
    })
})
function editYritus(yritusid) {
    const options = { options: { backdrop: true, keyboard: true, focus: true, show: true } };

    emptyyritusform();

    $.ajax({
        type: 'GET',
        url: 'Yritus/GetYritus',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: {
            'Id': yritusid        }
    }).done(function (result) {
        $('#yritus-id').val(result.id);
        $('#yritus-yritusenimi').val(result.yrituseNimi);
        if ($('#yritus-yritusenimi').val() !== '') {
            $('#yritus-yritusenimi').removeClass('is-invalid').addClass('is-valid');
        } else {
            $('#yritus-yritusenimi').removeClass('is-valid').addClass('is-invalid');
        }

        $('#yritus-yrituseaeg').val(formatdate(result.yrituseAeg));
        if ($('#yritus-yrituseaeg').val() !== '') {
            $('#yritus-yrituseaeg').removeClass('is-invalid').addClass('is-valid');
        } else {
            $('#yritus-yrituseaeg').removeClass('is-valid').addClass('is-invalid');
        }

        $('#yritus-yritusekoht').val(result.yrituseKoht);
        if ($('#yritus-yritusekoht').val() !== '') {
            $('#yritus-yritusekoht').removeClass('is-invalid').addClass('is-valid');
        } else {
            $('#yritus-yritusekoht').removeClass('is-valid').addClass('is-invalid');
        }

        $('#yritus-lisainfo').val(result.lisainfo);
    });

    $('#yritus-detail-title').html('ÜRITUSE DETAILVAADE');

    $('#yritus-detail').modal(options).modal('show');
}
function emptyyritusform() {
    $('#yritus-id').val('');
    $('#yritus-yritusenimi').val('');
    $('#yritus-yritusenimi').removeClass('is-invalid').addClass('is-valid');
    $('#yritus-yrituseaeg').val('');
    $('#yritus-yrituseaeg').removeClass('is-invalid').addClass('is-valid');
    $('#yritus-yritusekoht').val('');
    $('#yritus-yritusekoht').removeClass('is-invalid').addClass('is-valid');
    $('#yritus-lisainfo').val('');
    $('#yritus-error').html('');
}
function formatdate(datetime) {
    if (datetime === '') return;

    const date = datetime.split('T')[0];
    const time = datetime.split('T')[1];

    const year = date.split('-')[0];
    const month = date.split('-')[1];
    const day = date.split('-')[2];

    const hours = time.split(':')[0];
    const minutes = time.split(':')[1];

    return day + '.' + month + '.' + year + ' ' + hours + ':' + minutes;
}
function formatdatetimeback(datetime) {
    if (datetime === '') return;

    const date = datetime.split(' ')[0];
    const time = datetime.split(' ')[1];

    const day = date.split('.')[0];
    const month = date.split('.')[1];
    const year = date.split('.')[2];

    const hours = time.split(':')[0];
    const minutes = time.split(':')[1];
    const seconds = '00';

    return year + '-' + month + '-' + day + 'T' + hours + ':' + minutes + ':' + seconds;
}
function kustutaYritus(id) {
    const options = { options: { backdrop: true, keyboard: true, focus: true, show: true } };

    emptyyesno();
    yritusId = id;

    $('#yesnomodallabel').html('Ürituse kustutamine');
    $('#yesnomodalcontent').html('Kas kustutada üritus ' + id + '?');

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
