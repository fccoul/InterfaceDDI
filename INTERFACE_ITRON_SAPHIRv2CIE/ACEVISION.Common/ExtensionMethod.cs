using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACEVISION.Common
{
    public static class ExtensionMethod
    {

        #region cryptage - periode facturation

                static Dictionary<char, int> DictionnaireDeCorrespondance = new Dictionary<char, int>();
                private static void CorrespondanceCaractereCode()
                {
                    if (DictionnaireDeCorrespondance == null || DictionnaireDeCorrespondance.Count == 0)
                    {
                        DictionnaireDeCorrespondance.Add('A', 38);
                        DictionnaireDeCorrespondance.Add('B', 37);
                        DictionnaireDeCorrespondance.Add('C', 45);
                        DictionnaireDeCorrespondance.Add('D', 41);
                        DictionnaireDeCorrespondance.Add('E', 47);
                        DictionnaireDeCorrespondance.Add('F', 58);
                        DictionnaireDeCorrespondance.Add('G', 124);
                        DictionnaireDeCorrespondance.Add('H', 62);
                        DictionnaireDeCorrespondance.Add('I', 111);
                        DictionnaireDeCorrespondance.Add('J', 91);
                        DictionnaireDeCorrespondance.Add('K', 93);
                        DictionnaireDeCorrespondance.Add('L', 94);
                        DictionnaireDeCorrespondance.Add('M', 95);
                        DictionnaireDeCorrespondance.Add('N', 63);
                        DictionnaireDeCorrespondance.Add('O', 105);
                        DictionnaireDeCorrespondance.Add('P', 123);
                        DictionnaireDeCorrespondance.Add('Q', 44);
                        DictionnaireDeCorrespondance.Add('R', 125);
                        DictionnaireDeCorrespondance.Add('S', 126);
                        DictionnaireDeCorrespondance.Add('T', 40);
                        DictionnaireDeCorrespondance.Add('U', 46);
                        DictionnaireDeCorrespondance.Add('V', 108);
                        DictionnaireDeCorrespondance.Add('W', 60);
                        DictionnaireDeCorrespondance.Add('X', 92);
                        DictionnaireDeCorrespondance.Add('Y', 64);
                        DictionnaireDeCorrespondance.Add('Z', 59);
                        DictionnaireDeCorrespondance.Add('0', 27);
                        DictionnaireDeCorrespondance.Add('1', 28);
                        DictionnaireDeCorrespondance.Add('2', 29);
                        DictionnaireDeCorrespondance.Add('3', 30);
                        DictionnaireDeCorrespondance.Add('4', 31);
                        DictionnaireDeCorrespondance.Add('5', 42);
                        DictionnaireDeCorrespondance.Add('6', 33);
                        DictionnaireDeCorrespondance.Add('7', 43);
                        DictionnaireDeCorrespondance.Add('8', 35);
                        DictionnaireDeCorrespondance.Add('9', 36);
                        DictionnaireDeCorrespondance.Add('!', 48);
                        DictionnaireDeCorrespondance.Add('#', 49);
                        DictionnaireDeCorrespondance.Add('$', 50);
                        DictionnaireDeCorrespondance.Add('%', 51);
                        DictionnaireDeCorrespondance.Add('&', 52);
                        DictionnaireDeCorrespondance.Add('(', 53);
                        DictionnaireDeCorrespondance.Add(')', 54);
                        DictionnaireDeCorrespondance.Add('*', 55);
                        DictionnaireDeCorrespondance.Add('+', 56);
                        DictionnaireDeCorrespondance.Add(',', 57);
                        DictionnaireDeCorrespondance.Add('-', 97);
                        DictionnaireDeCorrespondance.Add('.', 98);
                        DictionnaireDeCorrespondance.Add('/', 106);
                        DictionnaireDeCorrespondance.Add(':', 99);
                        DictionnaireDeCorrespondance.Add(';', 100);
                        DictionnaireDeCorrespondance.Add('<', 101);
                        DictionnaireDeCorrespondance.Add('>', 102);
                        DictionnaireDeCorrespondance.Add('?', 103);
                        DictionnaireDeCorrespondance.Add('@', 104);
                        DictionnaireDeCorrespondance.Add('[', 107);
                        DictionnaireDeCorrespondance.Add('\\', 109);
                        DictionnaireDeCorrespondance.Add(']', 110);
                        DictionnaireDeCorrespondance.Add('{', 112);
                        DictionnaireDeCorrespondance.Add('|', 113);
                        DictionnaireDeCorrespondance.Add('}', 114);
                        DictionnaireDeCorrespondance.Add('~', 115);
                        DictionnaireDeCorrespondance.Add('^', 116);
                        DictionnaireDeCorrespondance.Add('_', 117);
                    }
                }
                public static string Cryptage(this string LaChaine)
                {


                    string Result = string.Empty;
                    try
                    {
                        CorrespondanceCaractereCode();
                        //20022017
                        LaChaine = LaChaine.ToUpper();

                        if (!string.IsNullOrEmpty(LaChaine))
                        {
                            for (int i = 0; i < LaChaine.Length; i++)
                            {
                                char ch = LaChaine[i];
                                if (ch != null)
                                {
                                    int lecode = 0;
                                    DictionnaireDeCorrespondance.TryGetValue(ch, out lecode);
                                    if (lecode != null)
                                    {
                                        char LeCaractereCrypte = Convert.ToChar(lecode);
                                        if (LeCaractereCrypte != null)
                                            Result += LeCaractereCrypte;
                                    }
                                    else
                                        Result += ch;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }

                    return Result;
                }
                public static string DeCryptage(this string LaChaine)
                {
                    string Result = string.Empty;
                    try
                    {
                        CorrespondanceCaractereCode();
                        if (!string.IsNullOrEmpty(LaChaine))
                        {
                            for (int i = 0; i < LaChaine.Length; i++)
                            {
                                char ch = LaChaine[i];
                                if (ch != null)
                                {
                                    int lecode = (int)ch;
                                    if (DictionnaireDeCorrespondance.ContainsValue(lecode))
                                    {
                                        KeyValuePair<char, int> carac = DictionnaireDeCorrespondance.Where(p => p.Value == lecode).FirstOrDefault();
                                        if (carac.Key != null)
                                        {
                                            Result += carac.Key;
                                        }
                                        else
                                            Result += ch;//20022017
                                    }
                                    else
                                        Result += ch;//20022017

                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    return Result;
                }

                public static string GetPeriodeFacturation(this DateTime ladate)
                {
                    string Result = string.Empty;
                    //DateTime temp = ladate.AddMonths(-1);
                    //DateTime temp = ladate.AddMonths(-1);
                    //Result = temp.Month.ToString("00") + temp.Year.ToString("0000");

                    //19112015 :RG : periode facturation=periode consommation(mois) +1                
                    Result = ladate.Month.ToString("00") + ladate.Year.ToString("0000");


                    return Result;
                }


        /// <summary>
        /// retourne la date de but - date fin en fonction de la date d'execution du programme...
        /// </summary>
        /// <param name="ladate"></param>
        /// <param name="DateDebut"></param>
        /// <param name="DateFin"></param>
        /// <returns></returns>
                public static bool GetPeriodeRecherche(this DateTime ladate, ref DateTime? DateDebut, ref DateTime? DateFin)
                {
                    bool Result = false;
                    try
                    {
                        /*
                        DateTime temp = ladate.AddMonths(1);
                        DateDebut = new DateTime(ladate.Year, ladate.Month - 1, 1);
                        DateFin = new DateTime(temp.Year, temp.Month - 1, 1);
                        Result = true;
                         * */
                      
                            DateTime temp = ladate.AddMonths(1);
                            DateTime temp1 = ladate.AddMonths(-1);

                            int dernierjour = new DateTime(ladate.Year, ladate.Month, 1).AddDays(-1).Day;
                            DateDebut = new DateTime(temp1.Year, temp1.Month, dernierjour);
                            DateFin = new DateTime(temp.Year, temp.Month, 1);
                            Result = true;
                         
                         
                    }
                    catch (Exception ex)
                    {
                        Result = false;
                        throw ex;
                    }


                    return Result;
                }

        
        #endregion

        #region Types de demandes
                //--type des demandes
        /// <summary>
        /// 2:Raccordeme/Abonenment
        /// 3:Abonnement simple
        /// 4:Ré-Abonnement
        /// 5:Mutation
        /// 7:Resiliation à la demande
        /// 9:Modicfication commerciale
        /// 24:Augmentation PS SMC
        /// 36:Dimunition PS SMC
        /// 39:Variation Puissance
        /// 41:Rempl.CtrSET
        /// 08:Rempl.ctrSVC
        /// </summary>
                public enum enumTypeDI 
                {
                    Raccordement_Abonnement = 2,
                    Abonnement_Simple = 3,
                    Ré_Abonnement = 4,
                    Mutation = 5,
                    Résiliation_à_la_demande = 7,
                    Modification_commerciale = 9,
                    Augmentation_PS_SMC = 24,
                    Diminution_PS_SMC = 36,
                    Augmentation_PS_AMC = 38,
                    Variation_Puissance = 39,
                    Rempl_Ctr_SET = 41,
                    Rempl_Ctr_SVC = 8
                    //RG :23022017
                    ,Dimunition_PS_AMC=54

                }
        #endregion
    }
}
