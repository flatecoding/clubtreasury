export function setCookie(name, value, days) {
    let expires = "";
    if (days) {
        const date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = `${name}=${encodeURIComponent(value || "")}${expires}; path=/`;
}

// Read a cookie
export function getCookie(name) {
    const nameEQ = name + "=";
    const cookies = document.cookie.split(';');
    for (let c of cookies) {
        c = c.trim();
        if (c.startsWith(nameEQ))
            return decodeURIComponent(c.substring(nameEQ.length));
    }
    return null;
}

// Delete a cookie
export function eraseCookie(name) {
    document.cookie = `${name}=; Max-Age=-99999999; path=/`;
}

// Update HTML background for theme switch
export function setThemeBackground(isDark) {
    const bg = isDark ? '#1e1e1e' : '#fff';
    document.documentElement.style.backgroundColor = bg;
    document.body.style.backgroundColor = bg;
    if (isDark) {
        document.documentElement.classList.add('dark');
    } else {
        document.documentElement.classList.remove('dark');
    }
}