function izveziUTabeluExcel(idTabele, nazivFajla) {
    var tabela = document.getElementById(idTabele);
    var html = tabela.outerHTML;

    var url = 'data:application/vnd.ms-excel;charset=utf-8,' + encodeURIComponent(
        '<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://w3.org">' +
        '<head><meta charset="UTF-8"><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet>' +
        '<x:Name>Лист1</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:Worksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--></head>' +
        '<body>' + html + '</body></html>'
    );

    var cistNaziv = (nazivFajla || 'Izvoz_ZU').replace(/\.(xlsx|xls)$/i, '');
    var linkZaPreuzimanje = document.createElement("a");
    document.body.appendChild(linkZaPreuzimanje);

    linkZaPreuzimanje.href = url;
    linkZaPreuzimanje.download = cistNaziv + '.xls';
    linkZaPreuzimanje.click();

    document.body.removeChild(linkZaPreuzimanje);
}
