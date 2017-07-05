using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACEVISION.Common
{
    public class ConnectionOBJ
    {
        [System.ComponentModel.DefaultValue(""),
        System.ComponentModel.DisplayName("Serveur"),
        System.ComponentModel.Description("Serveur de base de données"),
        System.ComponentModel.Category("Localisation de la Base de données"),
        System.ComponentModel.ReadOnly(true)]
        public string Data_Source { get; set; }

        [System.ComponentModel.DefaultValue(""),
        System.ComponentModel.DisplayName("Base de données"),
        System.ComponentModel.Description("NOEUD SAPHIRCOM - ACEVISION"),
        System.ComponentModel.Category("Localisation de la Base de données"),
        System.ComponentModel.ReadOnly(true)]
        public string Initial_Catalog { get; set; }

        [System.ComponentModel.DefaultValue(true),
        System.ComponentModel.DisplayName("Persist Security Info"),
        System.ComponentModel.Description("Persist Security Info")]
        public bool Persist_Security_Info { get; set; }

        [System.ComponentModel.DefaultValue("sa"),
        System.ComponentModel.DisplayName("Compte Utilisateur"),
        System.ComponentModel.Description("Compte à utiliser"),
        System.ComponentModel.Category("Paramètres d'authentification"),
        System.ComponentModel.ReadOnly(true)]
        public string User_ID { get; set; }

        [System.ComponentModel.DefaultValue(""),
        System.ComponentModel.DisplayName("Mot de Passe"),
        System.ComponentModel.Description("Mot de passe du compte utilisateur"),
        System.ComponentModel.PasswordPropertyText(true),
        System.ComponentModel.Category("Paramètres d'authentification"),
        System.ComponentModel.ReadOnly(true)]
        public string Password { get; set; }

        public ConnectionOBJ()
        {
        }

        public void SetConnectionString(string connect)
        {
            if (string.IsNullOrEmpty(connect))
                return;

            string[] Infos = connect.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            Data_Source = Infos[0].Substring(Infos[0].IndexOf('=') + 1);
            Initial_Catalog = Infos[1].Substring(Infos[1].IndexOf('=') + 1);
            Persist_Security_Info = Infos[2].Substring(Infos[2].IndexOf('=') + 1).ToLower() == "true" ? true : false;
            User_ID = Infos[3].Substring(Infos[3].IndexOf('=') + 1);
            if (Infos.Length > 4)
                Password = Infos[4].Substring(Infos[4].IndexOf('=') + 1);


        }
        public string GetConnectionString()
        {
            string result = "";

            result += "Data Source=" + Data_Source;
            result += ";Initial Catalog=" + Initial_Catalog;
            result += ";Persist Security Info=" + (Persist_Security_Info ? "true" : "false");
            result += ";User ID=" + User_ID;
            //if (Password.Length > 0)
            //    result += ";Password=" + Password;
            result += ";Password=" + (!string.IsNullOrEmpty(Password) ? Password : string.Empty);

            return result;
        }
    }
}
