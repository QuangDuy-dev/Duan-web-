// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

(function () {
    const menu = document.querySelector(".menu");
    const toggle = document.querySelector(".toggle");

    const MENU_HALF = 140;
    const BTN_HALF = 30;
    const MARGIN = 16;
    const SNAP_PX = 6;

    let isDragging = false;
    let hasMoved = false;
    let startX, startY, originLeft, originTop;

    let centerX, centerY;

    function clampCenter(cx, cy) {
        const minX = BTN_HALF + MARGIN;
        const maxX = window.innerWidth - BTN_HALF - MARGIN;
        const minY = BTN_HALF + MARGIN;
        const maxY = window.innerHeight - BTN_HALF - MARGIN;
        return {
            x: Math.max(minX, Math.min(cx, maxX)),
            y: Math.max(minY, Math.min(cy, maxY))
        };
    }

    function applyCenter(cx, cy, animate) {
        const c = clampCenter(cx, cy);
        centerX = c.x;
        centerY = c.y;
        if (animate) {
            menu.style.transition = 'left 0.3s cubic-bezier(0.25,0.46,0.45,0.94), top 0.3s cubic-bezier(0.25,0.46,0.45,0.94)';
            setTimeout(() => { menu.style.transition = ''; }, 320);
        } else {
            menu.style.transition = 'none';
        }
        menu.style.left = (centerX - MENU_HALF) + 'px';
        menu.style.top = (centerY - MENU_HALF) + 'px';
    }

    function snapToEdge() {
        const snappedX = window.innerWidth - BTN_HALF - MARGIN;
        applyCenter(snappedX, centerY, true);
    }

    function shiftForOpen() {
        const neededX = MENU_HALF + BTN_HALF + MARGIN;
        const shiftedX = Math.min(centerX, window.innerWidth - neededX);
        const neededY = MENU_HALF + BTN_HALF + MARGIN;
        const shiftedY = Math.max(neededY, Math.min(centerY, window.innerHeight - neededY));
        applyCenter(shiftedX, shiftedY, true);
    }

    function init() {
        centerX = window.innerWidth - BTN_HALF - MARGIN;
        centerY = window.innerHeight * 0.75;
        applyCenter(centerX, centerY, false);
    }

    toggle.addEventListener('pointerdown', function (e) {
        isDragging = true;
        hasMoved = false;
        startX = e.clientX;
        startY = e.clientY;
        originLeft = parseInt(menu.style.left) || 0;
        originTop = parseInt(menu.style.top) || 0;
        toggle.setPointerCapture(e.pointerId);
        menu.style.transition = 'none';
        e.preventDefault();
    });

    toggle.addEventListener('pointermove', function (e) {
        if (!isDragging) return;
        const dx = e.clientX - startX;
        const dy = e.clientY - startY;
        if (!hasMoved && Math.hypot(dx, dy) > SNAP_PX) hasMoved = true;
        if (!hasMoved) return;

        const newLeft = originLeft + dx;
        const newTop = originTop + dy;
        applyCenter(newLeft + MENU_HALF, newTop + MENU_HALF, false);
    });

    toggle.addEventListener('pointerup', function (e) {
        if (!isDragging) return;
        isDragging = false;
        toggle.releasePointerCapture(e.pointerId);

        if (!hasMoved) {
            const isOpening = !menu.classList.contains('active');
            menu.classList.toggle('active');
            if (isOpening) {
                shiftForOpen();
            } else {
                snapToEdge();
            }
        } else {
            menu.classList.remove('active');
            snapToEdge();
        }
    });

    document.addEventListener('pointerdown', function (e) {
        if (!menu.contains(e.target)) {
            menu.classList.remove('active');
        }
    });

    window.addEventListener('resize', function () {
        snapToEdge();
    });

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();

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

function updateDualSlider() {
            const minInput = document.getElementById('minPriceInput');
            const maxInput = document.getElementById('maxPriceInput');
            const minLabel = document.getElementById('minPriceLabel');
            const maxLabel = document.getElementById('maxPriceLabel');
            const fill = document.getElementById('sliderFill');
            
            const maxLimit = parseInt(minInput.max);
            let minVal = parseInt(minInput.value);
            let maxVal = parseInt(maxInput.value);

            // 1. Force min and max to stay on their respective sides
            if (minVal > maxVal) {
                // If they cross, swap values or push them
                let temp = minVal;
                minVal = maxVal;
                maxVal = temp;
            }

            // 2. Update Labels (Formatting with dots)
            minLabel.innerText = minVal.toLocaleString('vi-VN');
            maxLabel.innerText = maxVal.toLocaleString('vi-VN');

            // 3. Update the Blue Fill Bar
            // Calculate percentage positions
            const percentMin = (minVal / maxLimit) * 100;
            const percentMax = (maxVal / maxLimit) * 100;

            fill.style.left = percentMin + "%";
            fill.style.width = (percentMax - percentMin) + "%";
        }

        // Initialize on page load
        $(document).ready(function() {
            updateDualSlider();
        });