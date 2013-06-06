namespace TsSoft.Installer.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class InstallRunnerTest
    {
        [TestMethod]
        public void TestNormalFlow()
        {
            var taskMock = new Mock<IInstallerTask>();
            taskMock.Setup(x => x.Backup());
            taskMock.Setup(x => x.Execute());
            var runner = new InstallRunner();
            var result = runner.ExecuteBatch(new[] { taskMock.Object });
            Assert.IsTrue(result);
            taskMock.Verify(x => x.Backup());
            taskMock.Verify(x => x.Execute());
            taskMock.Verify(x => x.Rollback(), Times.Never());
        }
    }
}