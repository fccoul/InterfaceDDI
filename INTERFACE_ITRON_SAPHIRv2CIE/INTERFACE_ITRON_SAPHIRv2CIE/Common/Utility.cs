using InterfaceServices.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace INTERFACE_ITRON_SAPHIRv2CIE.Common
{
    public class Utility
    {
        public DataTable FormatList_ExportToExcel(List<string> _lstHeaderColumns, List<ActivationAbonneDTO> _lstInfos)
        {
            DataTable _dtInfos = new DataTable();
            foreach (var item in _lstHeaderColumns)
            {
                _dtInfos.Columns.Add(item);
            }

            int cpt = 0;
            foreach (ActivationAbonneDTO item in _lstInfos)
            {
                _dtInfos.Rows.Add();

                _dtInfos.Rows[cpt][0] = item.Client;
                _dtInfos.Rows[cpt][1] = item.IdentifiantAbonne;
                _dtInfos.Rows[cpt][2] = item.ReferenceRaccordement;
                _dtInfos.Rows[cpt][3] = item.NumeroCompteur;
                _dtInfos.Rows[cpt][4] = item.LibelleTypeDemande;
                _dtInfos.Rows[cpt][5] = item.OLD_ReferenceRaccordement;
                _dtInfos.Rows[cpt][6] = item.OLD_IdentifiantAbonne;
                _dtInfos.Rows[cpt][7] = item.OLD_NumeroCompteur;
                //_dtInfos.Rows[cpt][8] = item.Address;
                item.Address = item.Agglomeration.Trim() + ", " + item.RueBoulevardAvenue.Trim() + ", " + item.LotIlot.Trim();
                _dtInfos.Rows[cpt][8] = item.Address;
                _dtInfos.Rows[cpt][9] = item.CodeSite;
                _dtInfos.Rows[cpt][10] = item.CodeExploitation;


                cpt++;
            }

            return _dtInfos;

        }

        public DataTable FormatList_ExportToExcelREception(List<string> _lstHeaderColumns, List<ReceptionDTO> _lstInfos)
        {

            DataTable _dtInfos = new DataTable();
            foreach (var item in _lstHeaderColumns)
            {
                _dtInfos.Columns.Add(item);
            }

            int cpt = 0;
            foreach (ReceptionDTO item in _lstInfos)
            {
                _dtInfos.Rows.Add();

                _dtInfos.Rows[cpt][0] = item.NumeroCompteur;
                _dtInfos.Rows[cpt][1] = item.ReferenceRaccordement;
                _dtInfos.Rows[cpt][2] = item.IdentifiantAbonne;
                _dtInfos.Rows[cpt][3] = item.PeriodeFacturation;
                _dtInfos.Rows[cpt][4] = item.DateReleve.Value;
                _dtInfos.Rows[cpt][5] = item.IndexNuit;
                _dtInfos.Rows[cpt][6] = item.IndexPointe;
                _dtInfos.Rows[cpt][7] = item.IndexJour;
                _dtInfos.Rows[cpt][8] = item.IndexHoraire;
                _dtInfos.Rows[cpt][9] = item.IndexReactif1;
                _dtInfos.Rows[cpt][10] = item.IndexIma1;
                _dtInfos.Rows[cpt][11] = item.IndexIma2;
                _dtInfos.Rows[cpt][12] = item.IndexIma3;
                _dtInfos.Rows[cpt][13] = item.IndexConsoMonop1;
                _dtInfos.Rows[cpt][14] = item.IndexConsoMonop2;
                _dtInfos.Rows[cpt][15] = item.IndexConsoMonop3;
                _dtInfos.Rows[cpt][16] = item.CodeSite;
                _dtInfos.Rows[cpt][17] = item.CodeExploitation;



                cpt++;
            }

            return _dtInfos;


        }

        public void SendEMail(string emailid, string subject, string body,ref string msgErr)
        {
            try
            {
                SmtpClient client = new SmtpClient();
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                //client.EnableSsl = true;
                client.EnableSsl = false;
                client.Host = System.Configuration.ConfigurationManager.AppSettings["serverSMTP"];
                // client.Port = 587;

                //System.Net.NetworkCredential credentials = new System.Net.NetworkCredential("xxxxx", "yyyy");
                //client.UseDefaultCredentials = false;
                //client.Credentials = credentials;

                System.Net.Mail.MailMessage msg = new System.Net.Mail.MailMessage();
                msg.From = new MailAddress("adminDDI@cie.ci");
                msg.To.Add(new MailAddress(emailid));

                msg.Subject = subject;
                msg.IsBodyHtml = true;
                msg.Body = body;

                client.Send(msg);
            }
            catch (Exception ex)
            {

                msgErr = ex.Message;
            }
        }
    }
}
