document.addEventListener("DOMContentLoaded", function () {
    const selectAllFiles = document.getElementById("selectAll");
    const fileCheckboxes = document.querySelectorAll(".file-checkbox");
    const filesCountSpan = document.getElementById("selectedCount");

    const selectAllReports = document.getElementById("selectAllReports");
    const reportCheckboxes = document.querySelectorAll(".report-checkbox");
    const reportsCountSpan = document.getElementById("selectedReportsCount");

    const submitBtn = document.getElementById("btnKreiraj");

    function updateState() {
        const checkedFiles = document.querySelectorAll(".file-checkbox:checked").length;
        const checkedReports = document.querySelectorAll(".report-checkbox:checked").length;

        filesCountSpan.textContent = checkedFiles;
        reportsCountSpan.textContent = checkedReports;

        submitBtn.disabled = !(checkedFiles > 0 && checkedReports > 0);

        if (selectAllFiles) {
            selectAllFiles.checked = checkedFiles === fileCheckboxes.length && fileCheckboxes.length > 0;
        }
        if (selectAllReports) {
            selectAllReports.checked = checkedReports === reportCheckboxes.length && reportCheckboxes.length > 0;
        }

        reportCheckboxes.forEach(cb => {
            const card = cb.closest(".report-card");
            if (card) {
                if (cb.checked) {
                    card.classList.add("border-primary", "bg-light");
                } else {
                    card.classList.remove("border-primary", "bg-light");
                }
            }
        });
    }

    if (selectAllFiles) {
        selectAllFiles.addEventListener("change", function () {
            fileCheckboxes.forEach(cb => cb.checked = selectAllFiles.checked);
            updateState();
        });
    }

    if (selectAllReports) {
        selectAllReports.addEventListener("change", function () {
            reportCheckboxes.forEach(cb => cb.checked = selectAllReports.checked);
            updateState();
        });
    }

    fileCheckboxes.forEach(cb => cb.addEventListener("change", updateState));
    reportCheckboxes.forEach(cb => cb.addEventListener("change", updateState));

    document.querySelectorAll("tbody tr").forEach(row => {
        row.addEventListener("click", function (e) {
            if (e.target.type !== "checkbox") {
                const cb = this.querySelector(".file-checkbox");
                if (cb) {
                    cb.checked = !cb.checked;
                    updateState();
                }
            }
        });
    });

    document.querySelectorAll(".report-card").forEach(card => {
        card.addEventListener("click", function (e) {
            if (e.target.type !== "checkbox" && e.target.tagName !== "LABEL") {
                const cb = this.querySelector(".report-checkbox");
                if (cb) {
                    cb.checked = !cb.checked;
                    updateState();
                }
            }
        });
    });
});
