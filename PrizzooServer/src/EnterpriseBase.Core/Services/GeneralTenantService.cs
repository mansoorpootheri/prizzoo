using Abp.Dependency;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseBase.Services
{
    public class GeneralTenantService : EnterpriseBaseDomainServiceBase
    {
        
        /// <summary>
        /// use this function to get next number for any class ex:item batchno,production process number,sales no etc..
        /// </summary>
        /// <typeparam name="T">Entity Type</typeparam>
        /// <typeparam name="TPrimaryKey">Primary Key Type</typeparam>
        /// <param name="fieldName">Field Name</param>
        /// <param name="prefix">Prefix of Number</param>
        /// <param name="padding">Padding of Number</param>
        /// <returns></returns>
        public async Task<string> GetNextNumber<T, TPrimaryKey>(string fieldName, string prefix = "", int padding = 6) where T : class, IEntity<TPrimaryKey>
        {
            var property = typeof(T).GetProperty(fieldName);
            if (property == null || property.PropertyType != typeof(string))
                throw new ArgumentException($"Field '{fieldName}' not found or not a string type");

            var repository = IocManager.Instance.Resolve<IRepository<T, TPrimaryKey>>();
            var query = repository.GetAll();
            
            var maxNumber = await Task.Run(() =>
            {
                return query.AsEnumerable()
                    .Select(entity => property.GetValue(entity)?.ToString())
                    .Where(value => !string.IsNullOrEmpty(value) && value.StartsWith(prefix))
                    .Select(value => ExtractNumber(value, prefix))
                    .Where(num => num.HasValue)
                    .DefaultIfEmpty(0)
                    .Max();
            });

            var nextNumber = (maxNumber ?? 0) + 1;
            return $"{prefix}{nextNumber.ToString().PadLeft(padding, '0')}";
        }

        private int? ExtractNumber(string value, string prefix)
        {
            var numberPart = value.Substring(prefix.Length);
            return int.TryParse(numberPart, out var number) ? number : (int?)null;
        }
    }
}