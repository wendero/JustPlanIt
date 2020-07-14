// Write your JavaScript code.
$(document).ready(function () {
    //$('[data-toggle="tooltip"]').tooltip();
    // $('body').tooltip({
    //     selector: '[data-toggle=tooltip]'
    // });

    // $('.hint').tooltipster({
    //     theme: 'tooltipster-borderless'
    // });
});
Date.prototype.ToTanderaFormat = function (format) {
    if(!this.IsDate())
        return "-";

    let dt = this;
    let newFormat = format ? format : "yyyy-MM-dd HH:mm:ss";

    let yyyy = dt.getFullYear();
    let MM = (dt.getMonth() + 1).toString().padStart(2, '0');
    let dd = (dt.getDate()).toString().padStart(2, '0');
    let HH = (dt.getHours()).toString().padStart(2, '0');
    let mm = (dt.getMinutes()).toString().padStart(2, '0');
    let ss = (dt.getSeconds()).toString().padStart(2, '0');

    return newFormat.replace("yyyy", yyyy).replace("MM", MM).replace("dd", dd).replace("HH", HH).replace("mm", mm).replace("ss", ss);
}
Date.prototype.IsDate = function () {
    return (this instanceof Date && !isNaN(this.valueOf()));
}
String.prototype.ToTanderaFormat = function (format) {
    let newFormat = format ? format : "yyyy-MM-dd HH:mm:ss";
    let value = this;
    if (!value) return '-';
    var rx = new RegExp('^(\\d{4})-(\\d{2})-(\\d{2}) (\\d{2}).(\\d{2}).(\\d{2}).*$');
    var mx = rx.exec(value);

    let yyyy = mx[1];
    let MM = mx[2];
    let dd = mx[3];
    let HH = mx[4];
    let mm = mx[5];
    let ss = mx[6];

    return newFormat.replace("yyyy", yyyy).replace("MM", MM).replace("dd", dd).replace("HH", HH).replace("mm", mm).replace("ss", ss);
}
function NewID() {
    var text = "";
    var possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    for (var i = 0; i < 8; i++)
        text += possible.charAt(Math.floor(Math.random() * possible.length));

    return text;
}
function GetProperty(obj, key, returnKeyIfNotFound) {
    return key.split('.').reduce((nestedObject, key) => {
        if (nestedObject && key in nestedObject) {
            return nestedObject[key];
        }
        if (returnKeyIfNotFound)
            return key;
        return undefined;
    }, obj);
}