// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function showModal() {
    document.getElementById("modal").display = "flex";
    document.getElementById("modal").position = "absolute";

    document.getElementById("backdrop").display = "block";

    console.log('anal');
}

function hideModal() {
    document.getElementById("modal").display = "none";

    document.getElementById("backdrop").display = "none";

    console.log('anal');
}