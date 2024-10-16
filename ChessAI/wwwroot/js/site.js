// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
}

function setCookie(name, value, days = 365) {
    const expires = new Date(Date.now() + days * 864e5).toUTCString();
    document.cookie = `${name}=${value}; expires=${expires}; path=/`;
}

function initializeDarkMode() {
    const dm = getCookie('dm');
    const body = document.body;
    const dmSwitch = document.getElementById('dm-switch');

    if (dm === 'true') {
        body.classList.add('dm');
        if (dmSwitch) {
            dmSwitch.checked = true;
        }
    } else {
        body.classList.remove('dm');
        if (dmSwitch) {
            dmSwitch.checked = false;
        }
    }
}

document.addEventListener('DOMContentLoaded', () => {
    initializeDarkMode();

    const dmSwitch = document.getElementById('dm-switch');
    if (dmSwitch) {
        dmSwitch.addEventListener('change', function () {
            document.body.classList.toggle('dm');
            const dm = document.body.classList.contains('dm');
            setCookie('dm', dm.toString());
        });
    }
});