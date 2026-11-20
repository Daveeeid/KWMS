using KWMS.Data.Repositories;
using KWMS.Models;
using MySqlConnector;

namespace KWMS.Controllers
{

    public class SubscriptionController : ManagerControllerBase
    {
        private readonly SubscriptionRepository _subscriptions = new SubscriptionRepository();

        public DataResult<List<Subscription>> GetSubscribers(string? searchText, bool activeOnly)
        {
            int? companyId = CompanyId;
            if (companyId == null) return DataResult<List<Subscription>>.Fail(NoPermission);

            try
            {
                List<Subscription> list = _subscriptions.GetByCompany(companyId.Value, activeOnly);

                string search = (searchText ?? "").Trim();
                if (search.Length > 0)
                {
                    list = list.Where(s =>
                        s.ResidentName.Contains(search, StringComparison.OrdinalIgnoreCase)
                        || s.ServiceName.Contains(search, StringComparison.OrdinalIgnoreCase)
                        || s.ResidentLocation.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                return DataResult<List<Subscription>>.Ok(list);
            }
            catch (MySqlException) { return DataResult<List<Subscription>>.Fail(DatabaseDown); }
        }
    }
}