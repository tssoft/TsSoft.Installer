namespace TsSoft.Installer
{
    using System.Collections.Generic;
    using System.Linq;

    public class InstallRunner
    {
        public IInstallEnvironment InstallEnvironment { get; set; }

        public bool ExecuteBatch(IEnumerable<IInstallerTask> tasks)
        {
            var result = false;
            foreach (var task in tasks)
            {
                task.InstallEnvironment = InstallEnvironment;
            }
            var lockingTasks = tasks.Where(x => x is ILocking);
            foreach (var task in lockingTasks)
            {
                var lockingTask = task as ILocking;
                lockingTask.Lock();
            }

            try
            {
                foreach (var task in tasks)
                {
                    task.Backup();
                    try
                    {
                        task.Execute();
                        result = true;
                    }
                    catch
                    {
                        task.Rollback();
                    }
                }
            }
            finally
            {
                foreach (var task in lockingTasks)
                {
                    var lockingTask = task as ILocking;
                    lockingTask.Unlock();
                }
            }
            return result;
        }
    }
}