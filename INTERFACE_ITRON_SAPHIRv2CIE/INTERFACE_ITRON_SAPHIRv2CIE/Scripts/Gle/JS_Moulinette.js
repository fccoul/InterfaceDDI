$(document).ready(function () {

    //----remove All in Cache Browser......
    $.ajaxSetup({ cache: false });
    //--------------

    //alert("hello");
    $("#btnAfficher").hide();
    $("#loading").hide();
    $("#succesPartial").hide();
    //$('#btnImport').hide();

    //$("#divGrid").hide();
    //$("#myModal").modal("hide");
    //$("#myModal").modal({ show: false });
    //$(function () {
    //    $("#myModal").modal({ show: false });
    //});
});

$('#btnImport').click(function () {

    $('#btnImport').hide();
    $("#loading").show();
    $("#fileUpload").hide();
    //alert("wharssss");
    //$("#loading").css("visibility", "visible");
    //10112016
    $("body").css("cursor", "progress");

    //$("#loading").show();
    //$("#divlstDI").css("visibility", "hidden").slideUp("slow");

    var xx = 12;
    //----------------------------
    var url = "/Account/Integrate?n=1";
    var ajaxCallBack=$.ajax({
        url: url,
        type: "GET",
        dataType:"json",
       // async: false,
        async: true,
        data: {},
        success: function (data) {
            try {

                $("#divContent").hide();
                //alert(data);
                if (data.search("KO") != -1) {

                   // $("body").css("cursor", "default");

                    var msg = data.split('#');
                   //    $("#loading").hide();
                    $("#divlstDI").css("visibility", "visible").slideDown("slow");

                    $("#ResultUpload").html("<h4><span style='color:red'>" + msg[1] + "</span></h4>");
                    $("#divBtnIntegrate").show();

                    if (data.search("succes") != -1)//affiche des succès sur les echec
                    {
                        $("#succesPartial").show();
                    }
                    else {
                        $("#succesPartial").hide();
                    }

                }
                else {
                   // $("#loading").hide();
                    var tab=data.split('#');
                    var msg = "Integration effectuée avec succès - "+tab[1]+" clients pris en compte !";
                    $("#ResultUpload").html("<span style='color:green'>" + msg + "</span>");
                    $("#divBtnIntegrate").hide();
                }
            }
            catch (e) {
                //alert(e)
                $("#ResultUpload").html("<span style='color:red'> Erreur d\'execution : " + e.statusText + "</span>");
                $("body").css("cursor", "default");
               // $("#loading").hide();
            }

        },
        error: function (errorData) {
 
            $("#ResultUpload").html("<span style='color:red'> Erreur d\'execution : " + errorData.statusText + "</span>");
 
            $("body").css("cursor", "default");
             
            $("#divlstDI").css("visibility", "visible").slideDown("slow");
           // $("#loading").hide();
        }

    });
    ajaxCallBack.always(
        function () {
            $("#loading").hide();
            $("body").css("cursor", "default");
            $('#btnImport').show();
            $("#fileUpload").show();
        }
        );

    //$("#loading").hide();
    //$("#loading").css("visibility", "hidden");
  
});

$("#btnAfficher").click(function () {
    $("#btnAfficher").hide();
    $('#btnImport').show();

    $("#divGrid").css("visibility", "visible").slideDown("slow");
});

$("#file").change(function ()
{
    $("#btnAfficher").show();
    $("#divContent").hide();
    $("#divBtnIntegrate").hide();
});

function test() {
    alert("debug");
}

function DisplayProgressMessage(ctl, msg) {
    $(ctl).prop("disabled", true).text(msg);
    $(".submit-progress").removeClass("hidden");
    $("body").addClass("submit-progress-bg");
    $("body").css("cursor", "progress");
    $("#iDViewBag").hide();
    return true;
}

function DisplayItemsInserted()
{
    //alert("hello");
    //jQuery.noConflict();
    //$("#myModal").modal("hide");

    //idTable
    var trHTML = '';

    var url = "/Account/DisplayItemsIntegrated?n=1";
    $.getJSON(url, function (data) {

        //-----------OK
        //$.each(data, function (i, item) {
        //    trHTML += '<tr><td>' + item.Client + '</td><td>' + item.Compteur + '</td></tr>';
        //});
        //$('#tableResult').append(trHTML);

        //---en mode kendo 
        var requestsDatasource = new kendo.data.DataSource({
            data: data,
            pageSize: 5
        });

        $("#GridResult").kendoGrid({
            dataSource: requestsDatasource,
            pageable: {
                refresh: true,
                pageSizes: true,
                autoBind: false,
                previousNext: false,
                numeric: true
            },
            sortable: true,
            //filterable: true,
            filterable: {
                extra: false,
                operators: {
                    string: {
                        startswith: "Commence par",
                        eq: "est égal à",
                        //neq: "Is not equal to"
                    }
                }
            },
            columnMenu: true,
            resizable: true,
            columns: [

                    {
                        field: "Client", title: "Client",
                       // template: "#if (Client != null && Client != 'null' && Client != '')  { ##= Client ##  } # ",
                        //hidden: true, width: 120
                        filterable: {
                            ui: ClientFilter
                        },
                         width: 120
                    },
                     {
                         field: "IDABON", title: "Identifiant ",
                        // template: "#if (IDABON != null && IDABON != 'null' && IDABON != '')  { ##= IDABON ##  } # ",
                         width: 120
                     },
                    {
                        field: "Compteur", title: "Compteur",
                       // template: "#if (Compteur != null && Compteur != 'null' && Compteur != '')  { ##= Compteur ##  } # ",
                        width: 120
                    }
            ]
        });

        function ClientFilter(element) {
            element.kendoAutoComplete({
                dataSource: Client
            });
        }

       
    });

 
}

 