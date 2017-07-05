
    function DisplayProgressMessage(ctl, msg) {
        //$("#ResultOperation").html('');

        if ($("#valPwd").val() == '') {
            $("#ResultOperation").html('<span style="color:red">Mot de Passe d\'ouverture du fichier non renseigné !</span>');
            return false;
        }
        //---------------------------
        if (typeof ($("#file").val()) !== "undefined") {
            if ($("#file").val() != '') {
                var ext=$("#file").val().split('.').pop();
                console.log(ext);
                if (ext!="xls")
                {
                    console.log("aucun fichier selectionné");
                    $("#ResultOperation").html('<span style="color:red">Fichier non reconnu ,Veuillez choisir un fichier Excel .xls !</span>');
                    return false;
                }
                else
                {
                    $("#ResultOperation").html('');
                    $(ctl).prop("disabled", true).text(msg);
                    $(".submit-progress").removeClass("hidden");
                    $("body").addClass("submit-progress-bg");
                    $("body").css("cursor", "progress");
                    // $("#iDViewBag").hide();
                    return true;
                }
            }
            else {
                console.log("aucun fichier selectionné");
                $("#ResultOperation").html('<span style="color:red">aucun fichier selectionné</span>');
                return false;
            }
        }
        else {
            console.log("value not found");
            return false;
        }

        
    }

$("#btnAfficher").click(function(){
    // console.log('displaying');
    console.log($("#file").val());
    if (typeof($("#file").val()) !== "undefined")
    {
        if ($("#file").val() != '')
        {
            var _formFile = new FormData();
            _formFile.append("file", document.getElementById('file').files[0]);
            DisplayItemsFile(_formFile);
        }
        else
            console.log("aucun fichier selectionné");
    }
    else
        console.log("value not found");
        
});

function DisplayItemsFile(file) {
         
    //console.log(file);
    var trHTML = '';
    var file = file;
    //var url = "/Params/DisplayContent?&file=" + file;
    var url = "/Params/DisplayContent";
    //file = "hello";
    //$.post(url, { file: file }, function (dataJSON) {
    //}, 'json');
    $.ajax({
        url: url,
        type: 'POST',
        data: file,
        dataType:'json',
        cache: false,
        processData: false,
        contentType: false
            
        /*,error: function (failed) {
            console.log(failed);
        })*/
    }).done(function (dataJSON) {

        //alert(dataJSON);
        console.log(dataJSON);
        //var result = dataJSON.Resultat;
        //console.log(dataJSON.Resultat);
        console.log(dataJSON.detailsError);
        console.log(dataJSON.detailsResultat[0]);
           
        if (dataJSON.Resultat == "OK")
        {

            //---en mode kendo 
            var requestsDatasource = new kendo.data.DataSource({
                data: dataJSON.detailsResultat,
                //data: dataJSON,
                pageSize: 10
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
                            field: "SERIALNUMBER", title: "SERIAL NUMBER",
                            // template: "#if (Client != null && Client != 'null' && Client != '')  { ##= Client ##  } # ",
                            //hidden: true, width: 120
                            filterable: {
                                ui: CtrFilter
                            },
                            width: 120
                        },
                         {
                             field: "PASSWORD_READER", title: "PASSWORD READER ",
                             width: 120
                         },
                        {
                            field: "PASSWORD_LABO", title: "PASSWORD LABO",
                            width: 120
                        }
                        ,
                        {
                            field: "TYPEMETER", title: "TYPE METER",
                            width: 120
                        },
                        {
                            field: "FIRMWARE", title: "FIRMWARE",
                            width: 120
                        }
                ]
            });

            function CtrFilter(element) {
                element.kendoAutoComplete({
                    dataSource: SERIALNUMBER
                });
            }
        }

    })
    .fail(function(failed)
    {
        console.log(failed);
    });
        
         
}

function DiplayDiv() {
    console.log("here");
    var url = "/Params/ManageFile";
    $.get(url, function () {
        console.log("redirect OK");
    })
}
 
$("#IDlink").click(function () {

    console.log("here");
    var url = "/Params/ManageFileReload?&reload=OK";
    $.get(url, function (data) {
        console.log("redirect OK");
        $("#OtherAction").hide();
    })
});

/*
function DisplayPrompt() {
     
        WinId = window.open('', 'newwin', 'width=100,height=100');
        if (!WinId.opener) WinId.opener = self;
        Text = '<form ';
        Text += 'onSubmit="opener.location=this.password.value + \'.html\'; self.close()">';
        Text += '<input type="password" name="password">';
        Text += '<\/form>';
        WinId.document.open();
        WinId.document.write(Text);
        WinId.document.close();
    
}
*/
function DisplayPrompt() {

   // $("#divPwdFile").modal('show');
    //$('#divPwdFile').modal({
    //    show: 'false'
    //});

    // $('#divPwdFile').modal('toggle');

    $('#divPwdFile').modal();
}
function setValue()
{
    var _val = $("#pwdFile").val();
    if (_val != '')
    {
        $("#valPwd").val(_val);
        $('#divPwdFiles').modal('hide');
    }
    else
    { 
        //alert("aucune  valeur saisie !");
        $("#divEmpty").html("aucune  valeur saisie !");
        console.log("aucune  valeur saisie !");
        return false;
     }
     
}

function resetValue() {
    $("#valPwd").val("");
}

function test() {
    //alert('arrggghh');
    //$('#divPwdFiles').addClass("modalWindow");
    //$('#divPwdFiles').modal();
   // $('#divPwdFiles').addClass("modalWindow");
    // $('#divPwdFiles').
    //$('#divPwdFiles').modal("show");
    $('#divPwdFiles').modal();
    //$('#divPwdFiles').modal({
    //    show: 'false'
    //});

 
    //$("#divPw").dialog('open');
  
}
$('#IdivPwdFiles').click(function () {
    test();
  //  
});
$("#file").change(function () {

    if ($("#file").val() != '') {
        console.log("run");
        $("#pwdFile").val('');

        var ext=$("#file").val().split('.').pop();
        console.log(ext);
        if (ext != "xls") {
            console.log("aucun fichier selectionné");
            $("#ResultOperation").html('<span style="color:red">Fichier non reconnu ,Veuillez choisir un fichier Excel .xls !</span>');
            return false;
        }
        else {
            $("#ResultOperation").html('');
            $('#IdivPwdFiles').trigger('click');
        }
    }
});


$("#pwdFile").keypress(function () {
    $("#divEmpty").html('');
});
 