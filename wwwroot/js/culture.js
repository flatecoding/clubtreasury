window.setCultureCookie = function (value) {
    document.cookie = value;
};

window.setCultureAndReload = function (cookieValue) {
    document.cookie = cookieValue;
    location.reload();
};
