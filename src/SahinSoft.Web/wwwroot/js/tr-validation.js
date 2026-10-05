// jQuery Validate varsayılan olarak "125,00" (Türkçe ondalık virgül) değerini sayı saymaz; uygulama kültürü
// tr-TR olduğundan formlarda sayılar "1.234,56" biçiminde görünür. Bu dosya number/min/max/range
// kurallarını Türkçe (ve nokta-ondalık) girişi kabul edecek şekilde değiştirir. Sunucu tarafı
// TurkishDecimalModelBinder aynı kuralla okur.
(function ($) {
    if (!$ || !$.validator) { return; }

    function parseTr(value) {
        var text = String(value == null ? '' : value).trim().replace(/\s/g, '');
        if (text === '') { return NaN; }
        if (text.indexOf(',') !== -1) {
            text = text.replace(/\./g, '').replace(',', '.');
        }
        return /^-?\d+(\.\d+)?$/.test(text) ? parseFloat(text) : NaN;
    }

    $.validator.methods.number = function (value, element) {
        return this.optional(element) || !isNaN(parseTr(value));
    };
    $.validator.methods.min = function (value, element, param) {
        return this.optional(element) || parseTr(value) >= param;
    };
    $.validator.methods.max = function (value, element, param) {
        return this.optional(element) || parseTr(value) <= param;
    };
    $.validator.methods.range = function (value, element, param) {
        var n = parseTr(value);
        return this.optional(element) || (n >= param[0] && n <= param[1]);
    };
})(window.jQuery);
