using Microsoft.AspNetCore.Http;
using Narije.Core.DTOs.Enum;
using Narije.Core.DTOs.ViewModels.FoodPrice;
using Narije.Core.Entities;
using Narije.Infrastructure.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Narije.Infrastructure.Helpers
{
    public class LogHistoryHelper
    {
        private readonly NarijeDBContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LogHistoryHelper(NarijeDBContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }
        public static Dictionary<string, object> GetEntityChanges<TRequest, TEntity>(TRequest request, TEntity entity)
        {
            var changes = new Dictionary<string, object>();

            var entityProperties = typeof(TEntity).GetProperties()
                                                   .ToDictionary(p => p.Name.ToLower(), p => p, StringComparer.OrdinalIgnoreCase);

            foreach (var property in typeof(TRequest).GetProperties())
            {
                var newValue = property.GetValue(request);

                if (entityProperties.TryGetValue(property.Name.ToLower(), out var entityProperty))
                {
                    var currentValue = entityProperty.GetValue(entity);

                    if (!Equals(newValue, currentValue))
                    {
                        changes[$"before_{property.Name}"] = currentValue;

                        changes[property.Name] = newValue;
                    }
                }
            }

            return changes;
        }

        public async Task AddLogHistoryAsync(string name,int id, EnumLogHistroyAction action, EnumLogHistorySource source, string record , bool save )
        {
            try
            {
 var logEntry = new LogHistory
            {
                EntityName = name, 
                EntityId = id,
                UserId = TokenHelper.GetUserId(_httpContextAccessor), 
                Source = (int)source,
                Action = (int)action, 
                Changed = record,
                DateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time")),
            };

            await _dbContext.LogHistory.AddAsync(logEntry);
            if (save)
            {
                await _dbContext.SaveChangesAsync();
            }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
           
        }
    }
}
