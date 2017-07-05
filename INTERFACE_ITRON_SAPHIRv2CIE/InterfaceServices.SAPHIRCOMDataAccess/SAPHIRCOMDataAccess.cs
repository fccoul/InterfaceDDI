using ACEVISION.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfaceServices.SAPHIRCOMDataAccess
{
    public class SAPHIRCOMDataAccess
    {
        public static string GetConnectionString(bool pChaineCryptee, string connectionStringName)
        {
            string Resultat = string.Empty;

            if (pChaineCryptee)
            {
                Resultat = Crypteur.DecrypterText(ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString);
            }
            else
                Resultat = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;

            return Resultat;
        }
        public static bool EstConnexionStringValide(string ConnexionString, ref string Error)
        {
            Error = "";
            //- Test de la connexion
            try
            {
                if (string.IsNullOrEmpty(ConnexionString))
                    return false;

                SqlConnection Connexion = new SqlConnection(ConnexionString);
                //- Tentative d'ouverture de la connexion
                Connexion.Open();
                //- Message réussite
                //- Test ok. Fermeture de la connexion
                Connexion.Close();

                return true;
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                return false;
            }
        }
    }
}
