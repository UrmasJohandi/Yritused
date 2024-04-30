
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

        editYritusOsavotja($(this).find('td').find('span').html());
    });
    $('td[id^="Id_cell_"], td[id^="YrituseNimi_cell_"], td[id^="YrituseAeg_cell_"], td[id^="YrituseKoht_cell_"], td[id^="OsavotjaTaisnimi_cell_"], td[id^="OsavotjaIsikukood_cell_"], td[id^="Loodud_cell_"], td[id^="Muudetud_cell_"]').on('mousedown', function (e) {
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

    $('td[id^="Id_cell_"], td[id^="YrituseNimi_cell_"], td[id^="YrituseAeg_cell_"], td[id^="YrituseKoht_cell_"], td[id^="OsavotjaTaisnimi_cell_"], td[id^="OsavotjaIsikukood_cell_"], td[id^="Loodud_cell_"], td[id^="Muudetud_cell_"]').on('focusout', function (e) {
        var cellId = $(this).attr('id').replace('cell_', '');

        $('#span_' + cellId).hide();
        $('#' + cellId).show();
    });

    $('td[id^="Id_cell_"], td[id^="YrituseNimi_cell_"], td[id^="YrituseAeg_cell_"], td[id^="YrituseKoht_cell_"], td[id^="OsavotjaTaisnimi_cell_"], td[id^="OsavotjaIsikukood_cell_"], td[id^="Loodud_cell_"], td[id^="Muudetud_cell_"]').on('keyup', function (e) {
        if (e.keyCode == 13) {
            Cookies.set('filter', 'yritused');
            window.location.href = getFilterHref();
        } else if (e.keyCode == 27 || e.keyCode == 9) {
            $(this).trigger('focusout');
        }
    });

    $('#yritusosavotja-yritusenimi').autocomplete({
        serviceUrl: 'Yritus/GetAutocompleteByYrituseNimi',
        paramName: 'yrituseNimi_fr',
        maxHeight: 1000,
        width: 768,
        minChars: 0,
        transformResult: function (result) {
            return { suggestions: JSON.parse(result) }
        },
        onSelect: function (suggestion) {
            $.ajax({
                type: 'GET',
                url: 'Yritus/GetYritusByYrituseNimi',
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
                data: {
                    'yrituseNimi': suggestion.value
                }
            }).done(function (result) {
                $('#y-yritusenimi').html(result.yrituseNimi);
                $('#y-yrituseaeg').html(formatdate(result.yrituseAeg));
                $('#y-yritusekoht').html(result.yrituseKoht);
                $('#yritusosavotja-yritus-id').val(result.id);

                if ($('#yritusosavotja-yritusenimi').val() !== '') {
                    $('#yritusosavotja-yritusenimi').removeClass('is-invalid').addClass('is-valid');
                } else {
                    $('#yritusosavotja-yritusenimi').removeClass('is-valid').addClass('is-invalid');
                }
            });
        }
    });

    $('#yritusosavotja-osavotja').autocomplete({
        serviceUrl: 'Osavotja/GetAutocompleteByOsavotjaNimi',
        paramName: 'taisnimi_fr',
        maxHeight: 1000,
        width: 768,
        minChars: 0,
        transformResult: function (result) {
            return { suggestions: JSON.parse(result) }
        },
        onSelect: function (suggestion) {
            $.ajax({
                type: 'GET',
                url: 'Osavotja/GetOsavotjaByNimi',
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
                data: {
                    'taisnimi': suggestion.value
                }
            }).done(function (result) {
                $('#yritusosavotja-isikukood').val(result.isikukood + ' ' + result.taisnimi);
                $('#o-eesnimi').html(result.eesnimi);
                $('#o-perenimi').html(result.perenimi);
                $('#o-isikukood').html(result.isikukood);
                $('#o-liik').html(result.liik === 'F' ? 'Füüsiline isik' : result.liik === 'J' ? 'Juriidiline isik' : '');
                $('#o-makseviis').html(result.makseviis === 'P' ? 'Pangaülekandega' : result.makseviis === 'S' ? 'Sularahas' : '');
                $('#yritusosavotja-osavotja-id').val(result.id);

                $('#yritusosavotja-osavotja').val(result.taisnimi + ' ' + result.isikukood);
                if ($('#yritusosavotja-osavotja').val() !== '') {
                    $('#yritusosavotja-osavotja').removeClass('is-invalid').addClass('is-valid');
                } else {
                    $('#yritusosavotja-osavotja').removeClass('is-valid').addClass('is-invalid');
                }

                $('#yritusosavotja-isikukood').val(result.isikukood + ' ' + result.taisnimi);
                if ($('#yritusosavotja-isikukood').val() !== '') {
                    $('#yritusosavotja-isikukood').removeClass('is-invalid').addClass('is-valid');
                } else {
                    $('#yritusosavotja-isikukood').removeClass('is-valid').addClass('is-invalid');
                }
            });
        }
    });

    $('#yritusosavotja-isikukood').autocomplete({
        serviceUrl: 'Osavotja/GetAutocompleteByIsikukood',
        paramName: 'isikukood_fr',
        maxHeight: 1000,
        width: 768,
        minChars: 0,
        transformResult: function (result) {
            return { suggestions: JSON.parse(result) }
        },
        onSelect: function (suggestion) {
            $.ajax({
                type: 'GET',
                url: 'Osavotja/GetOsavotjaByIsikukood',
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
                data: {
                    'isikukood': suggestion.value
                }
            }).done(function (result) {
                $('#yritusosavotja-osavotja').val(result.taisnimi + ' ' + result.isikukood);
                $('#o-eesnimi').html(result.eesnimi);
                $('#o-perenimi').html(result.perenimi);
                $('#o-isikukood').html(result.isikukood);
                $('#o-liik').html(result.liik === 'F' ? 'Füüsiline isik' : result.liik === 'J' ? 'Juriidiline isik' : '');
                $('#o-makseviis').html(result.makseviis === 'P' ? 'Pangaülekandega' : result.makseviis === 'S' ? 'Sularahas' : '');
                $('#yritusosavotja-osavotja-id').val(result.id);

                $('#yritusosavotja-osavotja').val(result.taisnimi + ' ' + result.isikukood);
                if ($('#yritusosavotja-osavotja').val() !== '') {
                    $('#yritusosavotja-osavotja').removeClass('is-invalid').addClass('is-valid');
                } else {
                    $('#yritusosavotja-osavotja').removeClass('is-valid').addClass('is-invalid');
                }

                $('#yritusosavotja-isikukood').val(result.isikukood + ' ' + result.taisnimi);
                if ($('#yritusosavotja-isikukood').val() !== '') {
                    $('#yritusosavotja-isikukood').removeClass('is-invalid').addClass('is-valid');
                } else {
                    $('#yritusosavotja-isikukood').removeClass('is-valid').addClass('is-invalid');
                }
            });
        }
    });
    $('#yritusosavotja-submit').on('click', function () {
        var error = false;

        if ($('#yritusosavotja-yritusenimi').val() === '') {
            $('#yritusosavotja-yritusenimi').removeClass('is-valid').addClass('is-invalid');

            error = true;
        }

        if ($('#yritusosavotja-osavotja').val() === '') {
            $('#yritusosavotja-osavotja').removeClass('is-valid').addClass('is-invalid');

            error = true;
        }

        if ($('#yritusosavotja-isikukood').val() === '') {
            $('#yritusosavotja-isikukood').removeClass('is-valid').addClass('is-invalid');

            error = true;
        }

        if (error) return;

        const yritusosavotja = {
            Id: $('#yritusosavotja-id').val(),
            Yritus_Id: $('#yritusosavotja-yritus-id').val(),
            Osavotja_id: $('#yritusosavotja-osavotja-id').val(),
        };
        $.ajax({
            type: 'POST',
            url: 'YritusOsavotja/SaveYritusOsavotja',
            dataType: 'JSON',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(yritusosavotja)
        }).done(function (result) {
            if (result === 'OK') {
                $('#yritusosavotja-detail').modal('hide');
                location.reload(true);
            } else {
                $('#yritusosavotja-error').html(result);
            }

        });
    });
    $('#yesno-submit').on('click', function () {
        window.location.href = deleteUrl + '?Id=' + yritusOsavotjaId + '&pageNr=' + pageNr;
    })
    $('#yritusosavotja-yritusenimi').on('blur', function () {
        if ($('#yritusosavotja-yritusenimi').val() !== '') {
            $('#yritusosavotja-yritusenimi').removeClass('is-invalid').addClass('is-valid');
        } else {
            $('#yritusosavotja-yritusenimi').removeClass('is-valid').addClass('is-invalid');
        }
    })
    $('#yritusosavotja-osavotja').on('blur', function () {
        if ($('#yritusosavotja-osavotja').val() !== '') {
            $('#yritusosavotja-osavotja').removeClass('is-invalid').addClass('is-valid');
        } else {
            $('#yritusosavotja-osavotja').removeClass('is-valid').addClass('is-invalid');
        }

        if ($('#yritusosavotja-osavotja').val() === '') {
            $('#yritusosavotja-isikukood').val('');

            $('#o-eesnimi').html('');
            $('#o-perenimi').html('');
            $('#o-isikukood').html('');
            $('#o-liik').html('');
            $('#o-makseviis').html('');
        }
    })
    $('#yritusosavotja-isikukood').on('blur', function () {
        if ($('#yritusosavotja-isikukood').val() !== '') {
            $('#yritusosavotja-isikukood').removeClass('is-invalid').addClass('is-valid');
        } else {
            $('#yritusosavotja-isikukood').removeClass('is-valid').addClass('is-invalid');
        }

        if ($('#yritusosavotja-isikukood').val() === '') {
            $('#yritusosavotja-osavotja').val('');

            $('#o-eesnimi').html('');
            $('#o-perenimi').html('');
            $('#o-isikukood').html('');
            $('#o-liik').html('');
            $('#o-makseviis').html('');
        }
    })
});
function editYritusOsavotja(yritusosavotjaid) {
    const options = { options: { backdrop: true, keyboard: true, focus: true, show: true } };

    emptyyritusosavotjaform();

    $.ajax({
        type: 'GET',
        url: 'YritusOsavotja/GetYritusOsavotjaListViewModel',
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        data: {
            'Id': yritusosavotjaid
        }
    }).done(function (result) {
        $('#yritusosavotja-yritusenimi').val(result.yritus.yrituseNimi + ' ' + formatdate(result.yritus.yrituseAeg) + ' ' + result.yritus.yrituseKoht);
        if ($('#yritusosavotja-yritusenimi').val() !== '') {
            $('#yritusosavotja-yritusenimi').removeClass('is-invalid').addClass('is-valid');
        } else {
            $('#yritusosavotja-yritusenimi').removeClass('is-valid').addClass('is-invalid');
        }

        $('#yritusosavotja-osavotja').val(result.osavotja.taisnimi + ' ' + result.osavotja.isikukood);
        if ($('#yritusosavotja-osavotja').val() !== '') {
            $('#yritusosavotja-osavotja').removeClass('is-invalid').addClass('is-valid');
        } else {
            $('#yritusosavotja-osavotja').removeClass('is-valid').addClass('is-invalid');
        }

        $('#yritusosavotja-isikukood').val(result.osavotja.isikukood + ' ' + result.osavotja.taisnimi);
        if ($('#yritusosavotja-isikukood').val() !== '') {
            $('#yritusosavotja-isikukood').removeClass('is-invalid').addClass('is-valid');
        } else {
            $('#yritusosavotja-isikukood').removeClass('is-valid').addClass('is-invalid');
        }

        $('#yritusosavotja-id').val(result.yritusOsavotja.id);
        $('#yritusosavotja-yritus-id').val(result.yritusOsavotja.yritus_Id);
        $('#yritusosavotja-osavotja-id').val(result.yritusOsavotja.osavotja_Id);

        $('#y-yritusenimi').html(result.yritus.yrituseNimi);
        $('#y-yrituseaeg').html(formatdate(result.yritus.yrituseAeg));
        $('#y-yritusekoht').html(result.yritus.yrituseKoht);

        $('#o-eesnimi').html(result.osavotja.eesnimi);
        $('#o-perenimi').html(result.osavotja.perenimi);
        $('#o-isikukood').html(result.osavotja.isikukood);
        $('#o-liik').html(result.osavotja.liik === 'F' ? 'Füüsiline isik' :
            result.osavotja.liik === 'J' ? 'Juriidiline isik' : '');
        $('#o-makseviis').html(result.osavotja.makseviis === 'P' ? 'Pangaülekandega' : 
            result.osavotja.makseviis === 'S' ? 'Sularahas' : '');
    });

    $('#yritusosavotja-detail-title').html('ÜRITUSEST OSAVÕTJA DETAILVAADE');

    $('#yritusosavotja-detail').modal(options).modal('show');
}
function emptyyritusosavotjaform() {
    $('#yritusosavotja-yritusenimi').val('');
    $('#yritusosavotja-yritusenimi').removeClass('is-invalid').addClass('is-valid');
    $('#yritusosavotja-osavotja').val('');
    $('#yritusosavotja-osavotja').removeClass('is-invalid').addClass('is-valid');
    $('#yritusosavotja-isikukood').val('');
    $('#yritusosavotja-isikukood').removeClass('is-invalid').addClass('is-valid');
    $('#yritusosavotja-error').html('');
    $('#yritusosavotja-error').html('');
}
function formatdate(datetime) {
    const date = datetime.split('T')[0];
    const time = datetime.split('T')[1];

    const year = date.split('-')[0];
    const month = date.split('-')[1];
    const day = date.split('-')[2];

    const hours = time.split(':')[0];
    const minutes = time.split(':')[1];

    return day + '.' + month + '.' + year + ' ' + hours + ':' + minutes;
}
function kustutaYritusOsavotja(id) {
    const options = { options: { backdrop: true, keyboard: true, focus: true, show: true } };

    emptyyesno();
    yritusOsavotjaId = id;

    $('#yesnomodallabel').html('Üritusest osavõtja kustutamine');
    $('#yesnomodalcontent').html('Kas kustutada üritusest osavõtja ' + id + '?');

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
