const dropZone = document.getElementById('drop-zone');
const fileInput = document.getElementById('file-input');
const periodInput = document.getElementById('period-input');
const statusDiv = document.getElementById('upload-status');
const dropZoneText = document.getElementById('drop-zone-text');
const rfzoForm = document.getElementById('rfzo-form');
const submitBtn = document.getElementById('submit-btn');

document.addEventListener("DOMContentLoaded", function () {
    const danas = new Date();
    const godina = danas.getFullYear();
    const mesec = String(danas.getMonth() + 1).padStart(2, '0');
    periodInput.value = godina + '-' + mesec;
});

['dragenter', 'dragover', 'dragleave', 'drop'].forEach(function (eventName) {
    dropZone.addEventListener(eventName, function (e) {
        e.preventDefault();
        e.stopPropagation();
    }, false);
    document.body.addEventListener(eventName, function (e) {
        e.preventDefault();
        e.stopPropagation();
    }, false);
});

['dragenter', 'dragover'].forEach(function (eventName) {
    dropZone.addEventListener(eventName, function () {
        dropZone.classList.add('drag-over');
    }, false);
});

['dragleave', 'drop'].forEach(function (eventName) {
    dropZone.addEventListener(eventName, function () {
        dropZone.classList.remove('drag-over');
    }, false);
});

dropZone.addEventListener('drop', function (e) {
    const dt = e.dataTransfer;
    const files = dt.files;
    if (files.length > 0) {
        fileInput.files = files;
        prikaziIzabraniFajl(files[0].name);
    }
});

fileInput.addEventListener('change', function () {
    if (fileInput.files.length > 0) {
        prikaziIzabraniFajl(fileInput.files[0].name);
    }
});

function prikaziIzabraniFajl(imeFajla) {
    dropZoneText.innerText = 'Изабран фајл: ' + imeFajla;
    statusDiv.className = "alert alert-info mb-3";
    statusDiv.innerText = 'Спремно за слање: ' + imeFajla;
    statusDiv.classList.remove('d-none');
}

rfzoForm.addEventListener('submit', function () {
    submitBtn.disabled = true;
    submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Учитавање је у току... Сачекајте...';
});
