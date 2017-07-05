var oTable;

var thread = function () {

    //$('#lstDI tbody td img').click(function () {
         
    //    //alert("whatsss"); 

    //    var nTr = this.parentNode.parentNode;
    //    if (this.src.match('Minus')) {
    //        /* This row is already open - close it */

    //        this.src = "/Content/images/Plus.png";
    //        try {
    //            oTable.fnClose(nTr);
    //        }
    //        catch (e)
    //        {
    //            alert(e);
    //        }
    //    }
    //    else
    //    {


    //        this.src = "/Content/images/Minus.png";
    //        var serialNumber = $(this).attr("rel");

    //        //alert("else : le ctr est : " +serialNumber);
    //        var url = "/Emission/getDemandes_FromType_SAPHIR_Detail?NumCTR=" + serialNumber;

    //        $.get(url, function (Index) {
    //            try {
    //                oTable.fnOpen(nTr, Index, 'details');
    //            }
    //            catch (e)
    //            {
    //                alert("details : " + e);
    //            }
    //        });
    //    }
    //});

    $('#btnEmission').click(function () {

        //10112016
        $("body").css("cursor", "progress");

        $("#loading").show();
        $("#divlstDI").css("visibility", "hidden").slideUp("slow");
        //----------------------------
        var url = "/Emission/getData_FromSAPHIR";//dateDebut=" + DateDebut + "&dateFin=" + DateFin;
        $.ajax({
            url: url,
            type: "GET",
            //async: false,
            async: true,
            data: {},
            success: function (data) {
                try {

                    //alert(data);
                    //$('#ResultUpload').html(data);

                    //10112016
                    //21122016
                    if (data.search("Erreur")!=-1)
                    {
                      
                        $("body").css("cursor", "default");

                        $("#loading").hide();
                        $("#divlstDI").css("visibility", "visible").slideDown("slow");

                        if (data.search("succès") != -1)//affiche des succès sur les echec
                        {
                            $("#divlstDISucces").css("visibility", "visible");
                            var failedSuccess = data.split("#");

                            $("#divlstDISucces").html("<span style='color:green'>" + failedSuccess[1] + "</span>");
                            $("#ResultUpload").html("<span style='color:red'>" + failedSuccess[0] + "</span>");
                        }
                        else
                        {
                            $("#divlstDISucces").css("visibility", "hidden");
                            $("#divlstDISucces").html("");
                            $("#ResultUpload").html("<span style='color:red'>" + data + "</span>");
                        }
                        
                    }
                    else
                       { 
                          $("#loading").hide();
                        $("#ResultUpload").html("<span style='color:green'>" + data + "</span>");
                        $("#divlstDI").css("visibility", "hidden").slideUp("slow");
                        }
                }
                catch (e) {
                    //alert(e)
                    $("#ResultUpload").html("<span style='color:red'> Erreur d\'execution : " + e.statusText + "</span>");
                    $("body").css("cursor", "default");
                }

            },
            error: function (errorData) {
                //alert(errorData);
                // $('#ResultUpload').html(errorData);
                //04112016
                $("#ResultUpload").html("<span style='color:red'> Erreur d\'execution : " + errorData.statusText + "</span>");
                // $('#ResultUpload').css('color', 'red');

                //10112016
                $("body").css("cursor", "default");
                $("#loading").hide();
                $("#divlstDI").css("visibility", "visible").slideDown("slow");
            }
        });

    });
 
   
}

$(document).ready(function () {
    //19032017 ---cache
    $.ajaxSetup({ cache: false });
   //
    $("#loading").hide();

    ////////////////////
    $('#lstDI tbody td img').click(function () {

        //alert("whatsss"); 

        var nTr = this.parentNode.parentNode;
        if (this.src.match('Minus')) {
            /* This row is already open - close it */

            this.src = "/Content/images/Plus.png";
            try {
                oTable.fnClose(nTr);
            }
            catch (e) {
                alert(e);
            }
        }
        else {


            this.src = "/Content/images/Minus.png";
            var serialNumber = $(this).attr("rel");

            //alert("else : le ctr est : " +serialNumber);
            var url = "/Emission/getDemandes_FromType_SAPHIR_Detail?NumCTR=" + serialNumber;

            $.get(url, function (Index) {
                try {
                    oTable.fnOpen(nTr, Index, 'details');
                }
                catch (e) {
                    alert("details : " + e);
                }
            });
        }
    });
    ////////////////////////
    oTable = $('#lstDI').dataTable({
        "bJQueryUI": false,
        "aoColumns": [
        {
            "bSortable": false,
            "bSearchable": false

        },
        null,
        null,
        null,
        null,
        null,
        null,
        null
        ]

    });

    thread();
    var cpt = $('#cptLstDI').val();
    // alert("holl---->" + cpt);
   // cpt = 0;
    if(cpt==0)
        $("#btnEmission").css("visibility", "hidden");
    else
        $("#btnEmission").css("visibility", "visible");
    
});




function printContent(el) {

    var restorePage = document.body.innerHTML;

    var url = "/Emission/PrintTable";
    $.ajax({
        url: url,
        type: "GET",
        async: false,
        dataType: "json",
        data: {},
        success: function (data)
        {
            try {

                //alert(data);
                var myTable = "<table style='border:1px solid black; border-collapse: collapse;'><thead style='background-color: gray; color: black;'><tr><th align='left'>CLIENT</th><th>ID ABON</th><th>REF RACCOR</th><th>COMPTEUR</th><th>TYPE DI</th><th>ANC RACCOR</th><th>ANC IDABON</th><th>ANC COMPTEUR</th><th>ADRESSE</th><th>SITE</th><th>EXPLOITATION</th></tr>";
                myTable += "<tbody>";
               // var row = "";
                $.each(data, function (index, item) {
                    var row = "<tr><td align='left'>" + $.trim(item.Client) + "</td><td align='middle'>" + $.trim(item.IdentifiantAbonne) + "</td><td align='middle'>" + $.trim(item.ReferenceRaccordement) + "</td>";
                    row += "<td align='middle'>" + $.trim(item.NumeroCompteur) + "</td><td align='middle'>" + $.trim(item.LibelleTypeDemande) + "</td><td align='middle'>" + $.trim(item.OLD_ReferenceRaccordement) + "</td>";
                    row += "<td align='middle'>" + $.trim(item.OLD_IdentifiantAbonne) + "</td><td align='middle'>" + $.trim(item.OLD_NumeroCompteur) + "</td><td>" + $.trim(item.Address) + "</td>";
                    row += "<td align='middle'>" + $.trim(item.CodeSite) + "</td><td align='middle'>" + $.trim(item.CodeExploitation) + "</td>";
                    row += "</tr>";

                    myTable+=row;
                });
                myTable += "</tbody></table>";
                //alert(myTable);

                var printContent = myTable;
                var frame = document.createElement('iframe');
                document.body.appendChild(frame);
                frame.contentWindow.document.write(printContent);
                //frame.contentWindow.print();
                var browserName = navigator.userAgent.toLowerCase();
                if (browserName.indexOf("msie") != -1) {
                    frame.print();
                } else if (browserName.indexOf("trident") != -1) { //IE 11
                    frame.contentWindow.document.execCommand('print', false, null);
                }
                else
                    frame.contentWindow.print();

                document.body.removeChild(frame);

    
            }
            catch (e)
            {

                alert("erreur exception : " + e);
                //--rollback...
                document.body.innerHTML = restorePage;
            }

        },
        error: function (errorData)
        {
            alert("error return : " + errorData);
            //---rollback
            document.body.innerHTML = restorePage;
        }
    });



}

