$(() => {

    context.init({ preventDoubleContext: false });

    context.attach('.form-control-sm', [
        {
            text: 'Filter',
            action: function () {
                var cookieValue = Cookies.get('menu');
                if (cookieValue === 'yritused') {
                    Cookies.set('filter', 'yritused');
                } else if (cookieValue === 'osavotjad') {
                    Cookies.set('filter', 'osavotjad');
                } else if (cookieValue === 'yritusedosavotjad') {
                    Cookies.set('filter', 'yritusedosavotjad');
                }

                var href = window.location.href.indexOf('?') != -1 ? window.location.href.split('?')[0] : window.location.href;
                var filterfield = $('#model_filterfield').val();
                var filtervalue = $('#model_filtervalue').val();

                if (activeElementId) {
                    var addFilterValue = $('#' + 'input_' + activeElementId).val() === undefined ? $('#' + 'cmb_' + activeElementId).val() : $('#' + 'input_' + activeElementId).val();

                    if (filterfield === '') {
                        filterfield = activeElementId;
                        filtervalue = addFilterValue;
                    } else {
                        filterfield = filterfield + ';' + activeElementId;
                        filtervalue = filtervalue + ';' + addFilterValue;
                    }
                }

                window.location.href = href + '?' + 'orderby=' + $('#model_orderby').val() + '&' + 'filterfield=' + filterfield + '&' + 'filtervalue=' + filtervalue + '&' + 'p=' + '1' + '&' + 's=' + $('#model_pagesize').val();
            }
        }
    ]);
});
