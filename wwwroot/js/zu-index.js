document.getElementById('zuFajl').addEventListener('change', function (e) {
    if (e.target.files.length > 0) {
        document.getElementById('tekstFajla').textContent = "Изабран фајл: " + e.target.files[0].name;
    }
});
