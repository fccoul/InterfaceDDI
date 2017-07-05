using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACEVISION.Common
{
    public class Constantes
    {
        //public const string ACEVSIONConnexionStringName = "ACENODE_ConnectionString";
        public const string SAPHIRCOMConnexionStringName = "SAPHIRCOM_ConnectionString";

        public const string cFullNameInterfaceServiceSynchro = "ACEVISION.Common.IServiceSynchro";

        public const int cIntervalleParDefaut_ServiceSynchro_EnMinutes = 5;
        public const int cIntervalleParDefaut_ServicePRESynchro_EnMinutes = 30;
        public const string cValeurModifSiteSynchroImpacte_False = "0";
        public const string cValeurModifSiteSynchroImpacte_True = "1";
        public const string paramSettingPeriodiciteCheckService = "Periodicite Check Service";
        public const string paramSettingCodeSiteServicePRESynchro = "Code site";
        public const string paramSettingNomSiteServicePRESynchro = "Nom site";
        public const string paramSettingModifSiteSynchroImpacte = "Modif site impacte";
        public const string paramSettingLogPathSynchro = "logPathSynchro";
        public const string paramSettingLogPathServiceSynchro = "logPathServiceSynchro";

        public const string paramSettings_EndPointAddress_SAPHIR_Synchro = "Uri SAPHIRCOM";


        public const string EndPointConfigName_ACEVISION_Synchro = "WSHttpBinding_IServiceNodeSynchro";


        public const string RollingFileAppender = "rollingFile";
        public const string AdoNetAppender = "DataBaseAppender";
        public const string SmtpAppender = "smtpAppender";
         
        public const string ExecutionMode_Manuel = "Mode Manuel";
        public const string ExecutionMode_Auto = "Mode Automatique";
        public const string TypeTraitementRec = "Reception - Flux sortant Ace Vision";
        public const string TypeTraitementEmi = "Emission - Flux entrant Ace Vision";
        //12032017
        public const string TypeTraitementIntegration = "Integration Site {0} dans Ace Vision";
        public const string TraitementIntegration = "Integration Site";
        public const string TraitementIntegrationCtr = "Integration CTR";

        public const string MsgErrDataWithoutPerFact = "Integration annulée : \n Données sans période de facturation ...";
        public const string MsgErrLoadingSite = "Echec ! chargement des sites";

        public const string sectionMonitoringConfig = "Monitoring";
        public const string sectionConnexionConfig = "connectionStrings";
        public const string ProviderCryptDecrypt = "DataProtectionConfigurationProvider";

        public const string MsgInfos_SucceedRecording= "Enregistrement réussi avec succès ";

        public const string SectionEndpointClient = "system.serviceModel/client";
        public const string NameChannelEndpoint= "IContracts";

        public const string DefaultPhoneNumber = "21 23 00 00";
        public const string DefaultPasseWord = "1234567890";
        public const string DefaultEmail = "MailDefaultClient@cie.ci";
        public const string DefaultComment = "Creation de Client dans ACEVISION ";//+ DateTime.Now.ToString();
        public const string LeGroup = "COMPTEURS NON FACTURABLES";

        public const string DefaultCustomer = "CIE / 0000000";
        public const string DefaultAddress = "Magasin General (MG) CIE COTE D'IVOIRE";
        public const string DefaultIdAbon = "CIE|00000000";
        public const string DefaultCommentCreateCltDefault = "Client CIE par defaut pour les nouveaux compteurs et compteurs resiliés";

        public const string ObjectNotification = "Notification Interface SAPHIR - ACE VISION";

    }
}
