namespace TsSoft.Installer
{
    using System.Collections.Generic;
    using System.Linq;

    public class InstallRunner
    {
        public IInstallEnvironment InstallEnvironment { get; set; }

        public bool ExecuteBatch(IEnumerable<IInstallerTask> tasks)
        {
            foreach (var task in tasks)
            {
                task.InstallEnvironment = InstallEnvironment;
            }
            var lockingTasks = tasks.Where(x => x is ILocking).Select(x => x as ILocking);
            foreach (var task in lockingTasks)
            {
                task.Lock();
            }
            var result = true;
            try
            {
                var executedTasks = new List<IInstallerTask>();
                var taskEnumerator = tasks.GetEnumerator();
                while (result && taskEnumerator.MoveNext())
                {
                    var task = taskEnumerator.Current;
                    task.Backup();
                    try
                    {
                        executedTasks.Add(task);
                        var taskResult = task.Execute();
                        result = result && (taskResult || !task.FailOnError);
                        if (!result)
                        {
                            Rollback(executedTasks);
                        }
                    }
                    catch
                    {
                        if (task.FailOnError)
                        {
                            Rollback(executedTasks);
                        }
                        throw;
                    }
                }
            }
            finally
            {
                foreach (var task in lockingTasks)
                {
                    task.Unlock();
                }
            }
            return result;
        }

        private void Rollback(IEnumerable<IInstallerTask> executedTasks)
        {
            var reversedTasks = executedTasks.Reverse();
            foreach (var task in reversedTasks)
            {
                task.Rollback();
            }
        }
    }
}