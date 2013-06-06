namespace TsSoft.Installer
{
    public interface IInstallerTask
    {
        IInstallEnvironment InstallEnvironment { get; set; }

        bool FailOnError { get; }

        void Backup();

        bool Execute();

        void Rollback();
    }
}