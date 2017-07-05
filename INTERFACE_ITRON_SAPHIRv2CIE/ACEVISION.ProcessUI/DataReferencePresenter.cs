using InterfaceServices.BusinessRules;
using InterfaceServices.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACEVISION.ProcessUI
{
    public class DataReferencePresenter
    {
        BLL dr = new BLL();
        public List<ExploitationDTO> AllSite(ref string msgErr)
        {
            //string msgErr = string.Empty;
            try
            {
                return dr.GetAllExploitationAccessibles(string.Empty, ref msgErr).ToList();
            }
            catch (Exception ex)
            {

                msgErr = ex.Message;
                return null;
            }
           
        }

        public List<ExploitationDTO> AllExploiatations_ofSite(string codeSite,ref string msgErr)
        {
            //string msgErr = string.Empty;
            try
            {
                return dr.GetAllExploitationAccessibles_ofSIte(codeSite, ref msgErr).ToList();
            }
            catch (Exception ex)
            {

                msgErr = ex.Message;
                return null;
            }

        }

        //13122016
        public bool InsertData_FileExcel(ParamsFileDTO _paramsFileDTO, ref string msgErr)
        {
            try
            {
                return dr.InsertData_FileExcel(_paramsFileDTO, ref msgErr);
            }
            catch (Exception ex)
            {
                msgErr += ex.Message;
                return false;
            } 
            
        }

        public bool UpdateData_FileExcel(ParamsFileDTO _paramsFileDTO, ref string msgErr, bool isAdminExcel)
        {
            try
            {
                return dr.UpdateData_FileExcel(_paramsFileDTO, ref msgErr, isAdminExcel);
            }
            catch (Exception ex)
            {
                msgErr += ex.Message;
                return false;
            }

        }

        public string getPathFileExcel(ref string msgErr)
        {
            try
            {
                return dr.getPathFileExcel(ref msgErr);
            }
            catch (Exception ex)
            {
                
                msgErr = ex.Message;
                return null;
            }
        }

        public ParamsFileDTO get_ParamsFile(ref string msgErr)
        {
            try
            {
                return dr.get_ParamsFile(ref msgErr);
            }
            catch (Exception ex)
            {

                msgErr = ex.Message;
                return null;
            }
        }


        public EvolutionBaseDTO getVersionSoftWare(ref string msgErr)
        {
            try
            {
                return dr.getVersionSoftWare(ref msgErr);
            }
            catch (Exception ex)
            {

                msgErr = ex.Message;
                return null;
            }
        }
    }
}
