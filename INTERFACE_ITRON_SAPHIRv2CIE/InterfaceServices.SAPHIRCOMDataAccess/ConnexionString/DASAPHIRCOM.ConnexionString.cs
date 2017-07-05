using ACEVISION.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterfaceServices.DASAPHIRCOM 
{
    public class DASAPHIRCOMConnexionString
    {
        private static string _SAPHIRCOMConnexionString = string.Empty;
        private static bool _ChaineCryptee = false;
        private static string GetConnectionString(string connectionStringName)
        {
            //return ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            if (string.IsNullOrEmpty(_SAPHIRCOMConnexionString))
            {
                _SAPHIRCOMConnexionString = SAPHIRCOMDataAccess.SAPHIRCOMDataAccess.GetConnectionString(_ChaineCryptee, connectionStringName);
            }

            return _SAPHIRCOMConnexionString;
        }

        public static bool EstChaineDeConnexionALaBaseValide(ref string Error)
        {
            Error = string.Empty;
            _ChaineCryptee = false;
            string ChaineDeConnexion = DatabaseConnectionString;
            if (!SAPHIRCOMDataAccess.SAPHIRCOMDataAccess.EstConnexionStringValide(ChaineDeConnexion, ref Error))
            {
                //- Vérifier que la chaîne de connexion est cryptée.
                Crypteur _Crypteur = new Crypteur();
                string ChaineDecryptee = string.Empty;
                try
                {
                    ChaineDecryptee = Crypteur.DecrypterText(ChaineDeConnexion);
                    if (SAPHIRCOMDataAccess.SAPHIRCOMDataAccess.EstConnexionStringValide(ChaineDecryptee, ref Error))
                    {
                        _SAPHIRCOMConnexionString = ChaineDecryptee;
                        _ChaineCryptee = true;
                        Error = string.Empty;
                        return true;
                    }
                    else
                    {
                        Error = "Chaîne de connexion décryptée. Erreur d'accès : \n" + Error;
                        return false;
                    }
                }
                catch
                {
                    Error = "Chaîne de connexion non cryptée. Erreur d'accès : \n" + Error;
                    return false;
                }
            }

            return true;
        }
        public static string DatabaseConnectionString
        {
            get
            {
                return GetConnectionString(Constantes.SAPHIRCOMConnexionStringName);
            }
        }
    }
}
