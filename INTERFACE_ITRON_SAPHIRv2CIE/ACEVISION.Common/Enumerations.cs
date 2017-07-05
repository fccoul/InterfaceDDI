using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACEVISION.Common
{
    
       
        #region Enumérés Concernant la synchronisation

        public enum EnumTypeSynchronisation
        {
            Totale = 0,
            Differentielle = 1
        }
        public enum EnumSynchroEtapeEnCours
        {
            DemandeInitiee = 0,
            EmissionBordereaux=1,
            ReceptionIndex =2,
            SynchroEnCoursTerminee = 90,
            SynchronisationInterrompueParUser = 91,
            SynchronisationTerminee_CauseErreurAccesGesabel = 92,
            SynchronisationTerminee_CauseException = 93,
            PRESynchro_Interrompue_CauseException = 94,
            SynchronisationTerminee_CauseErreurAccesBaseSynchro = 95
        }
        public enum EnumSynchroCodeFinDeSynchro
        {
            TraitementOK = 0,
            TraitementOKAvecDesErreurs = 1,
            TraitementInterrompuParLUtilisateur = 2,
            TraitementInterrompuSuiteAUneException = 3
        }
        public enum EnumPeriodiciteSynchroAutomatique
        {
            Journalier = 1,
            Hebdomadaire = 2,
            Mensuel = 3
        }
        public enum EnumJoursDelaSemaine
        {
            Dimanche = 0,
            Lundi = 1,
            Mardi = 2,
            Mercredi = 3,
            Jeudi = 4,
            Vendredi = 5,
            Samedi = 6
        }
        public enum EnumServiceSynchroCustomCommand
        {
            StartAutomaticSynchro = 128,
            StartManualSynchro = 129,
            StopSynchro = 130
        }
        public enum EnumStatutExtractBordereaux
        {
            Disponible = 0,
            Extrait = 1
        }

        public static class GetValuesEnum
        {
            public static string GetLibelleEnumCodeFinSynchronisation(EnumSynchroCodeFinDeSynchro leResultat)
            {
                string libelle = string.Empty;

                switch (leResultat)
                {
                    case EnumSynchroCodeFinDeSynchro.TraitementOK:
                        libelle = "Ok";
                        break;
                    case EnumSynchroCodeFinDeSynchro.TraitementOKAvecDesErreurs:
                        libelle = "Ok - Avec des rejets";
                        break;
                    case EnumSynchroCodeFinDeSynchro.TraitementInterrompuParLUtilisateur:
                        libelle = "Interrompu par l'utilisateur";
                        break;
                    case EnumSynchroCodeFinDeSynchro.TraitementInterrompuSuiteAUneException:
                        libelle = "KO";
                        break;
                    default:
                        break;
                }

                return libelle;
            }
            public static string GetLibelleEnumPeriodiciteSynchoAutomatique(EnumPeriodiciteSynchroAutomatique lEnum)
            {
                string libelle = string.Empty;

                switch (lEnum)
                {
                    case EnumPeriodiciteSynchroAutomatique.Journalier:
                        libelle = "Une fois par Jour";
                        break;
                    case EnumPeriodiciteSynchroAutomatique.Hebdomadaire:
                        libelle = "Une fois par semaine";
                        break;
                    case EnumPeriodiciteSynchroAutomatique.Mensuel:
                        libelle = "Une fois par mois";
                        break;
                    default:
                        break;
                }

                return libelle;
            }
            public static string GetLibelleEnumEtapeEnCoursServiceSynchronisation(EnumSynchroEtapeEnCours lEtape)
            {
                string libelle = string.Empty;

                switch (lEtape)
                {
                    case EnumSynchroEtapeEnCours.SynchroEnCoursTerminee:
                        libelle = "Synchronisation terminée";
                        break;
                    case EnumSynchroEtapeEnCours.SynchronisationInterrompueParUser:
                        libelle = "Synchronisation interrompue par l'utilisateur";
                        break;
                    case EnumSynchroEtapeEnCours.SynchronisationTerminee_CauseErreurAccesGesabel:
                        libelle = "Erreur d'accès aux bases Source. Synchronisation interrompue";
                        break;
                    case EnumSynchroEtapeEnCours.SynchronisationTerminee_CauseException:
                        libelle = "Synchronisation interrompue suite à une exception";
                        break;
                    case EnumSynchroEtapeEnCours.SynchronisationTerminee_CauseErreurAccesBaseSynchro:
                        libelle = "Erreur d'accès à la base de synchro. Synchronisation interrompue";
                        break;
                    default:
                        break;
                }

                return libelle;
            }
            public static string GetLibelleEnumEtapeEnCoursListViewSynchroEnCours(EnumSynchroEtapeEnCours lEtape)
            {
                string libelle = "";

                switch (lEtape)
                {
                    case EnumSynchroEtapeEnCours.SynchroEnCoursTerminee:
                        libelle = "Synchro terminée";
                        break;
                    case EnumSynchroEtapeEnCours.SynchronisationInterrompueParUser:
                        libelle = "Synchro interrompue";
                        break;
                    case EnumSynchroEtapeEnCours.SynchronisationTerminee_CauseErreurAccesGesabel:
                        libelle = "Arrêt - Erreur accès Base source";
                        break;
                    case EnumSynchroEtapeEnCours.SynchronisationTerminee_CauseException:
                        libelle = "Arrêt - Exception enregistrée";
                        break;
                    case EnumSynchroEtapeEnCours.PRESynchro_Interrompue_CauseException:
                        libelle = "Arrêt PRE Synchro - Exception enregistrée";
                        break;
                    case EnumSynchroEtapeEnCours.SynchronisationTerminee_CauseErreurAccesBaseSynchro:
                        libelle = "Arrêt - Erreur accès Base synchro";
                        break;
                    default:
                        break;
                }
                return libelle;
            }
            public static string GetLibelleEnumJourDeLaSemaine(EnumJoursDelaSemaine LeJour)
            {
                string libelle = "";

                switch (LeJour)
                {
                    case EnumJoursDelaSemaine.Dimanche:
                        libelle = "Dimanche";
                        break;
                    case EnumJoursDelaSemaine.Lundi:
                        libelle = "Lundi";
                        break;
                    case EnumJoursDelaSemaine.Mardi:
                        libelle = "Mardi";
                        break;
                    case EnumJoursDelaSemaine.Mercredi:
                        libelle = "Mercredi";
                        break;
                    case EnumJoursDelaSemaine.Jeudi:
                        libelle = "Jeudi";
                        break;
                    case EnumJoursDelaSemaine.Vendredi:
                        libelle = "Vendredi";
                        break;
                    case EnumJoursDelaSemaine.Samedi:
                        libelle = "Samedi";
                        break;
                    default:
                        break;
                }
                return libelle;
            }
        }

        #endregion
}
