// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.getElementById('dm-switch').addEventListener('click', function () {
    document.body.classList.toggle('dm');
    const dm = document.body.classList.contains('dm');
    localStorage.setItem('dm', 'body');
});
