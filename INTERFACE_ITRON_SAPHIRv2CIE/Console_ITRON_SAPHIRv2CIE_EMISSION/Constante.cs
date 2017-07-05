using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Console_ITRON_SAPHIRv2CIE_EMISSION
{
    public static class Constante
    {
        public const string MsgErr_CallAjax = "Erreur de communicateur Asynchrone...";
        public const string MsgErr_SQLServer_Empty = "SQL Server non renseigné";
        public const string MsgErr_NameDataBase_Empty = "Base de données non renseignée";

        public const string Msg_Test_connexion = "test de connexion réussi {0}";
        public const string Msg_Test_connexion_failed = "Echec - test de connexion {0}, \n {1}";

        public const string ServeurBDDRequired = "Serveur de base de données non renseigné";
        public const string BDDRequired = "base de données non selectionnée";
        public const string UrlSceWebRequired = "Adresse du serice Web non renseignée";
        public const string PortSceWebRRequired = "Port du serice Web non renseigné";
        public const string LengthPortSceWebRRequired = "le port du serice Web doit être composé d'au moins 2 chiffres et au plus 4 chiffres";
        public const string TypeIntPort = "la valeur doit etre numerique";
        public const string ZeroPort = "0 n'est pas valide comme permier chiffre pour le port du service Web ";


        //-----------
        public const string msgErrHeader_Resiliation = "Message : ECHEC RESILIATION  ";
        public const string msgErrHeader_Abonnement_Simple = "Message : ECHEC ABONNEMENT SIMPLE ";
        public const string msgErrHeader_VariationPuissance = "Message : ECHEC VARIATION PUISSANCE ";
        public const string msgErrHeader_Modification_commerciale = "Message : ECHEC MODIFICATION COMMERCIALE ";
        public const string msgErrHeader_Mutation = "Message : ECHEC MUTATION ";
        public const string msgErrHeader_Raccordement_Abonnement = "Message : ECHEC RACCORDEMENT/ABONNEMENT ";
        public const string msgErrHeader_Re_Abonnement = "Message : ECHEC RE-ABONNEMENT ";
        public const string msgErrHeader_Rempl_Ctr = "Message : ECHEC REMPLACEMENT COMPTEUR (S) ";

        //-----------
        public const string roleSuperAdmin = "Super Administrateur";
        public const string roleAdmin = "Administrateur";
        public const string roleExecutif = "Executant";
        public const string roleAdminFileExcel = "Administrateur Excel";
        public const string AccountSuperAdmin = "SuperAdmin";

        public const string PasswordDefaultAccount = "password";

        public const string NameFileExcel = "ListeCTR_ITRON.xls";

    }
}
