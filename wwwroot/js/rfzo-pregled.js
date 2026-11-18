function izveziUExcel() {
    const tabela = document.getElementById("tabela-rfzo");
    if (!tabela) return;

    let html = tabela.outerHTML;
    const style = '<style> td { white-space:nowrap; } </style>';
    const meta = '<meta http-equiv="content-type" content="application/vnd.ms-excel; charset=UTF-8">';
    const template = '<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://w3.org">' + meta + '<head>' + style + '</head><body>' + html + '</body></html>';

    const blob = new Blob([template], {type: "application/vnd.ms-excel"});
    const url = URL.createObjectURL(blob);

    const a = document.createElement("a");
    a.href = url;

    const datum = new Date().toISOString().slice(0, 10);
    a.download = 'Pregled_Uvoza_RFZO_' + datum + '.xls';

    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
}
