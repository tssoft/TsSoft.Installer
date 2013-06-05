namespace TsSoft.Installer
{
    public interface IInstallerTask
    {
        public IInstallEnvironment InstallEnvironment { get; set; }

        void Backup();

        void Update();

        void Rollback();
    }
}