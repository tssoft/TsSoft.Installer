namespace TsSoft.Installer
{
    using System.Collections.Generic;
    using System.Linq;

    public class InstallRunner
    {
        public IEnumerable<IInstallerTask> Tasks { get; set; }

        public IInstallEnvironment InstallEnvironment { get; set; }

        public bool Update()
        {
            var result = false;
            foreach (var task in Tasks)
            {
                task.InstallEnvironment = InstallEnvironment;
            }
            var lockingTasks = Tasks.Where(x => x is ILocking);
            foreach (var task in lockingTasks)
            {
                var lockingTask = task as ILocking;
                lockingTask.Lock();
            }

            try
            {
                foreach (var task in Tasks)
                {
                    task.Backup();
                    try
                    {
                        task.Update();
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