
function printContent(el) {

    var restorePage = document.body.innerHTML;

    var url = "/Reception/PrintTable";
    $.ajax({
        url: url,
        type: "GET",
        async: false,
        dataType: "json",
        data: {},
        success: function (data) {
            try {

                //alert(data);
                var myTable = "<table style='border:1px solid black; border-collapse: collapse;'><thead style='background-color: gray; color: black;'><tr><th align='left'>COMPTEUR</th><th>REF RACCOR</th><th>ID ABON</th><th>PERIODE FAC</th><th>DATE RELEVE</th><th>INDEX NUIT</th><th>INDEX POINTE</th><th>INDEX JOUR</th><th>INDEX HORAIRE</th><th>INDEX REACTIF 1</th><th>INDEX IMA1</th><th>INDEX IMA2</th><th>INDEX IMA3</th><th>INDEX MONO1</th><th>INDEX MONO2</th><th>INDEX MONO3</th><th>SITE</th><th>EXPLOITATION</th></tr>";
                myTable += "<tbody>";
                // var row = "";
                $.each(data, function (index, item) {

                    var milli = item.DateReleve.replace(/\/Date\((-?\d+)\)\//, '$1');
                    var dtJS = new Date(parseInt(milli)).toLocaleDateString();
                    //alert(item.NumeroCompteur+" ---ladate  : "+dtJS);

                    var row = "<tr><td align='left'>" + $.trim(item.NumeroCompteur) + "</td><td align='middle'>" + $.trim(item.ReferenceRaccordement) + "</td><td align='middle'>" + $.trim(item.IdentifiantAbonne) + "</td>";
                    row += "<td align='middle'>" + $.trim(item.PeriodeFacturation) + "</td><td align='middle'>" + dtJS + "</td><td align='middle'>" + $.trim(item.IndexNuit) + "</td>";
                    row += "<td align='middle'>" + $.trim(item.IndexPointe) + "</td><td align='middle'>" + $.trim(item.IndexJour) + "</td><td>" + $.trim(item.IndexHoraire) + "</td>";
                    row += "<td align='middle'>" + $.trim(item.IndexReactif1) + "</td><td align='middle'>" + $.trim(item.IndexIma1) + "</td><td>" + $.trim(item.IndexIma2) + "</td>";
                    row += "<td align='middle'>" + $.trim(item.IndexIma3) + "</td><td align='middle'>" + $.trim(item.IndexConsoMonop1) + "</td><td>" + $.trim(item.IndexConsoMonop2) + "</td>";
                    row += "<td align='middle'>" + $.trim(item.IndexConsoMonop3) + "<td align='middle'>" + $.trim(item.CodeSite) + "</td><td align='middle'>" + $.trim(item.CodeExploitation) + "</td>";
                    row += "</tr>";

                    myTable += row;
                });
                myTable += "</tbody></table>";
                //alert(myTable);

                var printContent = myTable;
                var frame = document.createElement('iframe');
                document.body.appendChild(frame);
                frame.contentWindow.document.write(printContent);
                frame.contentWindow.print();
                document.body.removeChild(frame);


            }
            catch (e) {

                alert("erreur exception : " + e);
                //--rollback...
                document.body.innerHTML = restorePage;
            }

        },
        error: function (errorData) {
            alert("error return : " + errorData);
            //---rollback
            document.body.innerHTML = restorePage;
        }
    });



}