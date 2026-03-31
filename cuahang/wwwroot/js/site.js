// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

const menu = document.querySelector(".menu");
const toggle = document.querySelector(".toggle");
toggle.addEventListener("click", () => {
    menu.classList.toggle("active");
})

function openCallModal(number) {
    const modal = document.getElementById('callModal');
    document.getElementById('phoneNumber').innerText = number;

    modal.classList.add('active');

    modal.querySelector('.ok').onclick = () => {
        window.location.href = 'tel:' + number;
    };

    modal.querySelector('.cancel').onclick = () => {
        modal.classList.remove('active');
    };
}
