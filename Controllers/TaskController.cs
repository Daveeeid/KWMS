using KWMS.Data.Repositories;
using KWMS.Models;
using MySqlConnector;

namespace KWMS.Controllers
{
    public class TaskController : ManagerControllerBase
    {
        private readonly TaskRepository _tasks = new TaskRepository();
        private readonly WorkerRepository _workers = new WorkerRepository();

        public DataResult<List<CollectionTask>> GetTasks(CollectionTaskStatus? status)
        {
            int? companyId = CompanyId;
            if (companyId == null) return DataResult<List<CollectionTask>>.Fail(NoPermission);

            try
            {
                List<CollectionTask> list = _tasks.GetByCompany(companyId.Value);
                if (status != null) list = list.Where(t => t.Status == status).ToList();
                return DataResult<List<CollectionTask>>.Ok(list);
            }
            catch (MySqlException) { return DataResult<List<CollectionTask>>.Fail(DatabaseDown); }
        }

        // Rule 13: only active workers appear in the assignment list
        public DataResult<List<Worker>> GetAssignableWorkers()
        {
            int? companyId = CompanyId;
            if (companyId == null) return DataResult<List<Worker>>.Fail(NoPermission);

            try
            {
                List<Worker> active = _workers.GetByCompany(companyId.Value).Where(w => w.CanReceiveTasks).ToList();
                return DataResult<List<Worker>>.Ok(active);
            }
            catch (MySqlException) { return DataResult<List<Worker>>.Fail(DatabaseDown); }
        }

        public OperationResult AssignWorker(int taskId, int workerId)
        {
            int? companyId = CompanyId;
            if (companyId == null) return OperationResult.Fail(NoPermission);

            try
            {
                // Pre-check for a precise message. The repository SQL checks the same things again.
                Worker? worker = _workers.GetById(workerId, companyId.Value);
                if (worker == null) return OperationResult.Fail("The selected worker could not be found.");
                if (!worker.CanReceiveTasks) return OperationResult.Fail("Inactive workers cannot receive tasks.");

                if (!_tasks.AssignWorker(taskId, workerId, companyId.Value))
                    return OperationResult.Fail(
                        "This task can no longer be assigned. It may already be in progress, completed or cancelled.");

                return OperationResult.Ok($"The task was assigned to {worker.FullName}.");
            }
            catch (MySqlException) { return OperationResult.Fail(DatabaseDown); }
        }
    }
}