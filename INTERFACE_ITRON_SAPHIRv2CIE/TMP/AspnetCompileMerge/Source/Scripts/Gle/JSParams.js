function ToastFailed(message) {
    //alert("ko");
    var priority = 'danger';
    var title = 'Echec';
    //var message = 'les mots de passe ne correspondent pas !';
    // alert(message);
    $('#pwdError').html(message);
    $.toaster({ priority: priority, title: title, message: message });

}

function isEmpty(str) {
    return (!str || 0 === str.length);
}

function isBlank(str) {
    return (!str || /^\s*$/.test(str));
}

//String.prototype.isEmpty = function () {
//    return (this.length === 0 || !this.trim());
//};
var CurrentRole;
var roleAdminFileExcel = "Administrateur Excel";

$(document).ready(function () {

     CurrentRole = $('#lblRole').val();
    //alert(CurrentRole);
    
    var IDParamsCurrent = $('#txtIDParams').val();;
    //alert(IDParamsCurrent);

    if (CurrentRole == roleAdminFileExcel)
    {
        $("#areaAdmin").css("visibility", "hidden").slideUp("slow");
        $("#areaAdminExcel").css("visibility", "visible").slideDown("slow");
        }
    else
    {
        $("#areaAdmin").css("visibility", "visible").slideDown("slow");
        $("#areaAdminExcel").css("visibility", "hidden").slideUp("slow");
       
        }

    $("#btnFile").click(function () {
        //alert("btn");
        var url = "/Params/Upload";
        var pathFile = $('#filename').val();
        var pwd = "";
        //console.log($('#PasswordFile').val());
        //console.log($('#ConfirmPasswordFile').val());

        //alert(isEmpty(pathFile));
        //alert(isBlank(pathFile));

        //if (pathFile == "") {
        //alert(CurrentRole);
        if (CurrentRole == roleAdminFileExcel)
        {
            var valuePath = $("#lblPath").text();
            //alert("la value :" + valuePath);

            if (!isEmpty(valuePath) && !isBlank(valuePath))
            {

                if ($('#PasswordFile').val() != $('#ConfirmPasswordFile').val()) {
                    var message = 'les mots de passe ne correspondent pas !';
                    ToastFailed(message);
                    //alert("merde oops!");
                }
                else {
                    var pwd = $('#PasswordFile').val();

                    if (!isEmpty(pwd) && !isBlank(pwd))
                    {
                        $.ajax({
                            cache: false,
                            type: "POST",
                            contentType: "application/x-www-form-urlencoded",
                            url: url,
                            async: false,
                            //data: "{ _params :" + _params + ", _valParams :'" + valparams + "' }",
                            data: { _pathFile: pathFile, Password: pwd, _IDParamsCurrent: IDParamsCurrent },
                            success: function (data) {

                                //alert(data);
                                var msg = "Enregistrement réussi avec succès !";
                                $('#pwdError').html('');

                                if (data == "succes") {
                                    $("#resultFile").html("<span style='color:green'>" + msg + "</span>");
                                    

                                }
                                else
                                    $("#resultFile").html("<span style='color:red'> Echec d'enregistrement : " + data + "</span>");


                            },
                            error: function (errorData) {
                                alert(">>erreur !!!" + errorData);
                                console.log("failed");
                                console.log(errorData);
                            }
                        });
                    }
                    else
                    {
                        var message = 'Veuillez saisir le mot de passe !';
                        ToastFailed(message);
                    }
                    
                }

            }
            else
            {
                var message = 'Le repertoire du fichier n\'est pas configuré !';
                ToastFailed(message);
            }
        }
        else {
            if (!isEmpty(pathFile) && !isBlank(pathFile)) {

                var pwd = '';
                 
                    $.ajax({
                        cache: false,
                        type: "POST",
                        contentType: "application/x-www-form-urlencoded",
                        url: url,
                        async: false,
                        //data: "{ _params :" + _params + ", _valParams :'" + valparams + "' }",
                        data: { _pathFile: pathFile, Password: pwd, _IDParamsCurrent: IDParamsCurrent },
                        success: function (data) {

                            //alert(data);
                            var msg = "Enregistrement réussi avec succès !";
                            if (data == "succes") {
                                $('#pwdError').html('');
                                $("#resultFile").html("<span style='color:green'>" + msg + "</span>");
                                //---path
                                var url = "/Params/GetPathFile";
                                $.get(url, function (data) {
                                    //alert(data);
                                    $("#lblPath").text(data);
                                    //$("#lblPath").html(data);
                                    //$("#txtPath").val(data);
                                });

                            }
                            else
                                $("#resultFile").html("<span style='color:red'> Echec d'enregistrement : " + data + "</span>");


                        },
                        error: function (errorData) {
                            alert(">>erreur !!!" + errorData);
                            console.log("failed");
                            console.log(errorData);
                        }
                    });
              
            }
            else {
                var message = 'Veuillez saisir le chemin d\'acces au fichier !';
                ToastFailed(message);
            }
        }
    });
});