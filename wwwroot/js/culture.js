globalThis.setCultureCookie = function (value) {
    document.cookie = value;
};

globalThis.setCultureAndReload = function (cookieValue) {
    document.cookie = cookieValue;
    location.reload();
};
