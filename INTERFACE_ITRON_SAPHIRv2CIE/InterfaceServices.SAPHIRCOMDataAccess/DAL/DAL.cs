using INOVA.ISF.DATACESS;
using InterfaceServices.SAPHIRCOMDataAccess.SAPHIRCOM;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace InterfaceServices.SAPHIRCOMDataAccess
{
    public class DAL<T> : DACBase<T, SAPHIRCOMDataContext>
        where T : class, new()
    {
        /// <summary>
        /// Creates a new instance of ComptageHTAEntity
        /// </summary>
        /// <param name="connectionString">The connection string to the database that is going to be used.</param>
        public DAL(string connectionString)
            : base(connectionString)
        {
            ConfigDAC.TimeOUT = 240;
        }

        /// <summary>
        /// Creates a new instance of ComptageHTAEntity
        /// </summary>
        public DAL()
            : base(DASAPHIRCOM.DASAPHIRCOMConnexionString.DatabaseConnectionString)
        {

            ConfigDAC.TimeOUT = 240;
        }

        /// <summary>
        /// Adds a new record to the DB
        /// </summary>
        /// <param name="entity"> T </param>
        /// <returns><see cref="System.Object"/> </returns>
        public object InserRow(T entity)
        {
            return base.Create(entity);
        }


        /// <summary>
        /// Recupere la liste de tous les ligne de l'entite T
        /// </summary>
        /// <returns>collection of the T,
        /// <see cref="System.System.Collections.Generic.IList<T>"/></returns>
        public IList<T> FindAll()
        {
            return base.Read();
        }

        /// <summary>
        /// Recupere tous les lignes de l'entite T par un critère 
        /// donné et en spécifiant le type de chargement de données
        /// </summary>
        /// <param name="options"><see
        /// cref="System.Data.Linq.DataLoadOptions"/></param>
        /// <param name="query">Select Query</param>
        /// <returns>collection de l'entite  T ,
        /// <see cref="System.System.Collections.Generic.IList<T>"/></returns>
        public IList<T> FindAll(DataLoadOptions options)
        {
            return base.Read(options, null);
        }


        /// <summary>
        /// Recupere tous les lignes de l'entite T par un critère 
        /// donné et en spécifiant le type de chargement de données
        /// </summary>
        /// <param name="options"><see
        /// cref="System.Data.Linq.DataLoadOptions"/></param>
        /// <returns>collection de l'entite  T ,
        /// <see cref="System.System.Collections.Generic.IList<T>"/></returns>
        public IList<T> Find(DataLoadOptions options, Expression<Func<T, bool>> query)
        {
            return base.Read(options, query);
        }


        /// <summary>
        /// Select From DB on the defined query
        /// </summary>
        /// <param name="options"><see
        /// cref="System.Data.Linq.DataLoadOptions"/></param>
        /// <param name="query">Select Query</param>
        /// <param name="from">for pagination Purposes, starting Index</param>
        /// <param name="to">for pagination Purposes, End Index</param>
        /// <returns>collection of the current type,
        /// <see cref="System.System.Collections.Generic.IList<T>"/></returns>
        /// <remarks>if "to" parameter was passed as 0,
        /// it will be defaulted to 100, you can replace it by
        /// a valued defined in the config, and another point
        /// of interest, if from > to, from will be
        /// reseted to 0.
        /// 
        /// if there is no query defined, all results will be
        /// returned, and also if there is no load data options
        /// defined, the results will contain only the entity specified
        /// with no nested data (objects) within that entity.
        /// </remarks>
        public IList<T> Find(DataLoadOptions options, Expression<Func<T, bool>> query, int from, int to)
        {
            return base.Read(options, query, from, to);
        }



        /// <summary>
        /// Recupere tous les lignes de l'entite T par un critère donné 
        /// </summary>
        /// <param name="options"><see
        /// cref="System.Data.Linq.DataLoadOptions"/></param>
        /// <param name="query">Select Query</param>
        /// <returns>collection de l'entite  T 
        /// <see cref="System.System.Collections.Generic.IList<T>"/></returns>        
        public IList<T> Find(Expression<Func<T, bool>> query)
        {
            return base.Read(query);
        }


        /// <summary>
        /// Supprimme l'entité definit par le critère
        /// </summary>
        /// <param name="query">Requête de suppression</param>
        public void DeleteRow(Expression<Func<T, bool>> query)
        {
            base.Delete(query);
        }



        /// <summary>
        /// Met a jour  l'entité T  passé en paramètre
        /// </summary>
        /// <param name="entity">Entité qui contient les données a mettre à jour</param>
        /// <param name="query">Requête pour recuperer l'objet T  a mettre à jour 
        /// <remarks>this method will do dynamic property mapping between the passed entity
        /// and the entity retrieved from DB upon the query defined,
        /// ONLY ValueTypes and strings are
        /// mapped between both entities, NO nested objects will be mapped, you have to do
        /// the objects mapping nested in your entity before calling this method</remarks>
        public void UpdateRow(T entity, Expression<Func<T, bool>> query)
        {
            base.Update(entity, query);
        }



    }
}
