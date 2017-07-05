$(document).ready(function () {

    //alert("hello");

    $(function () {
        $('body').removeClass('fade-out');

    });

    $('#BtnConnexionss').click(function () {

       // alert("click");
        var LoginSaisi = $('#txtUser').val();
        //alert(LoginSaisi);
        var PwdSaisi = $('#txtMoDp').val();

        if (LoginSaisi == '') {
            textbox.setCustomValidity('Veuillez saisir le login');
        }

        //alert(LoginSaisi);
        //alert("ok");
        //alert(url);
        //var valReturn = '';
        //var _params = -1;
        //var valparams = "";
        //if (code === 1) {
        //    valparams = LoginSaisi;
        //    _params = 0;
        //}
        //else {
        //    valparams = PwdSaisi;
        //    _params = 1;
        //}

        _params = 0;
        valparams = LoginSaisi;

        var url = "/Connexion/CheckAuthentication";
        //alert(url);
        $.ajax({
            cache: false,
            type: "GET",
            url: url,
            async: false,
            //dataType: "json",
            //contentType: 'application/json; charset=utf-8',
            //async: false,
            //data: "{ _params :" + _params + ", _valParams :'" + valparams + "' }",
            //data: "{ _params :'" + test + "', _valParams :'" + valparams + "' }",
            data: {},
            success: function (data) {
                //alert(data);
          

                //console.log(data);
                //console.log("succes");

                window.location.pathname = "/Home/Index";
                /*
                valReturn = data.d;
                // alert(data.d);
                if (valReturn === 0) {
                    if (_params === 0)
                        textbox.setCustomValidity('Veuillez saisir le login');
                    else
                        textbox.setCustomValidity('Veuillez saisir le Mot de passe');
                }
                    //else if(textbox.validity.typeMismatch){
                else if (valReturn === 2) {
                    if (_params === 0)
                        textbox.setCustomValidity('Login inexistant');
                    else
                        textbox.setCustomValidity('Mot de passe incorrect');

                }
                else {
                    // textbox.setCustomValidity('');
                    alert("your else!");
                }
                */
                //-*------------end @me
            },
            error: function (errorData) {
                alert(">>erreur !!!" + errorData);                
                console.log("failed");           
                console.log(errorData);
            }
        });
        

    });

});


function InvalidMsgLogin(textbox,code) {

    //alert("ok");
    var LoginSaisi = $('#txtUser').val();
    //alert(LoginSaisi);
    var PwdSaisi = $('#txtMoDp').val();

    if (LoginSaisi == '') {
        textbox.setCustomValidity('Veuillez saisir le login');
    }
 
    alert("ok");
    //alert(url);
    var valReturn = '';
    var _params = -1;
    var valparams = "";
    if (code === 1) {
        valparams = LoginSaisi;
        _params = 0;
    }
    else {
        valparams = PwdSaisi;
        _params = 1;
    }

    //alert(valparams + "------------" + _params);
    var test="ok";
    
    /*
    var dfd = $.Deferred();

    var promiseAjax = $.ajax({
        cache:false,
        type: "GET",
        url: url,
        //async: true,
        //dataType: "json",
        //async: false,
        //data: "{ _params :" + _params + ", _valParams :'" + valparams + "' }",
        data: "{ _params :'cool', _valParams :'" + valparams + "' }",
        success: function (data) {
            //alert(data);


            valReturn = data.d;
            // alert(data.d);
            if (valReturn === 0) {
                if (_params === 0)
                    textbox.setCustomValidity('Veuillez saisir le login');
                else
                    textbox.setCustomValidity('Veuillez saisir le Mot de passe');
            }
                //else if(textbox.validity.typeMismatch){
            else if (valReturn === 2) {
                if (_params === 0)
                    textbox.setCustomValidity('Login inexistant');
                else
                    textbox.setCustomValidity('Mot de passe incorrect');

            }
            else {
                textbox.setCustomValidity('');

            }
            //-*------------end @me
        },
        error: function (errorData)
        {
            alert("erreur !!!" + errorData);
        }
    });

    promiseAjax.done(function () {
        dfd.notify("Les paramètres ont été vérifiés.");
        console.log("L'opération a réussi.");
    });

    */

    /*
    var deferred = $.Deferred();

    deferred.done(function (value) {
        //alert(value);
        console.log("La veification est terminée avec succès et la réponse est : " + value);
   
    })
    */
    /*
    La méthode « resolve » lancera l’exécution des callbacks « done » et « always ».
La méthode « reject » lancera l’exécution des callbacks « fail » et « always ».
*/
    //deferred.resolve(function (value) {

    

    //});
 
 
}


