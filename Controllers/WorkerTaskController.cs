using KWMS.Data.Repositories;
using KWMS.Models;
using MySqlConnector;

namespace KWMS.Controllers
{

    public enum MyTaskFilter { ToDo, Completed, Cancelled, All }





    public class WorkerTaskController
    {
        private readonly WorkerRepository _workers = new WorkerRepository();
        private readonly TaskRepository _tasks = new TaskRepository();
        private readonly StatisticsRepository _statistics = new StatisticsRepository();

        private const string NoPermission = "You do not have permission to perform this action.";
        private const string DatabaseDown = "Cannot reach the database. Is MySQL running in XAMPP?";





        private Worker? GetCurrentWorker(out string error)
        {
            error = string.Empty;
            User? user = SessionManager.CurrentUser;

            if (user == null || user.Role != UserRole.CollectionWorker)
            {
                error = NoPermission;
                return null;
            }

            Worker? worker = _workers.GetByUserId(user.UserID);

            if (worker == null || worker.CompanyID != user.CompanyID)
            {
                error = "Your account has no valid worker profile. Please contact your manager.";
                return null;
            }

            if (worker.Status != RecordStatus.Active)
            {
                error = "Your worker profile is inactive. Please contact your manager.";
                return null;
            }

            return worker;
        }




        public DataResult<List<CollectionTask>> GetMyTasks(MyTaskFilter filter)
        {
            try
            {
                Worker? worker = GetCurrentWorker(out string error);
                if (worker == null) return DataResult<List<CollectionTask>>.Fail(error);

                IEnumerable<CollectionTask> tasks = _tasks.GetByWorker(worker.WorkerID);

                switch (filter)
                {
                    case MyTaskFilter.ToDo:

                        tasks = tasks
                            .Where(t => t.Status == CollectionTaskStatus.Assigned || t.Status == CollectionTaskStatus.InProgress)
                            .OrderBy(t => t.CollectionDate).ThenBy(t => t.TaskID);
                        break;
                    case MyTaskFilter.Completed:
                        tasks = tasks.Where(t => t.Status == CollectionTaskStatus.Completed);
                        break;
                    case MyTaskFilter.Cancelled:
                        tasks = tasks.Where(t => t.Status == CollectionTaskStatus.Cancelled);
                        break;
                }

                return DataResult<List<CollectionTask>>.Ok(tasks.ToList());
            }
            catch (MySqlException) { return DataResult<List<CollectionTask>>.Fail(DatabaseDown); }
        }

        public DataResult<WorkerStatistics> GetMyStatistics()
        {
            try
            {
                Worker? worker = GetCurrentWorker(out string error);
                if (worker == null) return DataResult<WorkerStatistics>.Fail(error);

                return DataResult<WorkerStatistics>.Ok(_statistics.GetWorkerStatistics(worker.WorkerID));
            }
            catch (MySqlException) { return DataResult<WorkerStatistics>.Fail(DatabaseDown); }
        }




        public OperationResult StartTask(int taskId)
        {
            try
            {
                Worker? worker = GetCurrentWorker(out string error);
                if (worker == null) return OperationResult.Fail(error);


                CollectionTask? task = _tasks.GetByWorker(worker.WorkerID).FirstOrDefault(t => t.TaskID == taskId);
                if (task == null)
                    return OperationResult.Fail("This task is not assigned to you.");

                if (!task.CanStart)
                    return OperationResult.Fail("This collection cannot be started: " + DescribeStatus(task.Status));


                if (!_tasks.Start(taskId, worker.WorkerID))
                    return OperationResult.Fail("The task changed while you were working on it. Please refresh the list.");

                return OperationResult.Ok("The collection is now in progress.");
            }
            catch (MySqlException) { return OperationResult.Fail(DatabaseDown); }
        }




        public OperationResult CompleteTask(int taskId)
        {
            try
            {
                Worker? worker = GetCurrentWorker(out string error);
                if (worker == null) return OperationResult.Fail(error);

                CollectionTask? task = _tasks.GetByWorker(worker.WorkerID).FirstOrDefault(t => t.TaskID == taskId);
                if (task == null)
                    return OperationResult.Fail("This task is not assigned to you.");

                if (!task.CanComplete)
                    return OperationResult.Fail("This collection cannot be completed: " + DescribeStatus(task.Status));


                if (!_tasks.Complete(taskId, worker.WorkerID))
                    return OperationResult.Fail("The task changed while you were working on it. Please refresh the list.");

                return OperationResult.Ok("The collection was marked as completed. Well done!");
            }
            catch (MySqlException) { return OperationResult.Fail(DatabaseDown); }
        }


        private static string DescribeStatus(CollectionTaskStatus status) => status switch
        {
            CollectionTaskStatus.Pending => "it has not been assigned to you yet.",
            CollectionTaskStatus.Assigned => "it must be started first.",
            CollectionTaskStatus.InProgress => "it is already in progress.",
            CollectionTaskStatus.Completed => "it is already completed.",
            CollectionTaskStatus.Cancelled => "it was cancelled.",
            _ => "its status does not allow this."
        };
    }
}