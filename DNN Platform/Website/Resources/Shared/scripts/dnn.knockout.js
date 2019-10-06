ko.bindingHandlers.dnnDatePicker = {
    init: function (element, valueAccessor) {
        Sys.Application.add_load(function () {
            var picker = $find(element.id.replace('_wrapper', ''));
            if (!picker) {
                return;
            }
            var value = ko.utils.unwrapObservable(valueAccessor());
            picker.set_selectedDate(value);
            picker.add_dateSelected(function (sender, e) {
                var date = e.get_newDate();
                var observable = valueAccessor();
                if (observable().compare(date) !== 0) {
                    observable(date);
                }
            });
        });
    },
    update: function (element, valueAccessor) {
        var picker = $find(element.id.replace('_wrapper', ''));
        var value = ko.utils.unwrapObservable(valueAccessor());
        if (picker) {
            picker.set_selectedDate(value);
        }
    }
};