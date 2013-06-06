namespace TsSoft.Installer.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;

    [TestClass]
    public class InstallRunnerTest
    {
        [TestMethod]
        public void TestNormalFlow()
        {
            var taskMock = new Mock<IInstallerTask>();
            taskMock.Setup(x => x.Backup());
            taskMock.Setup(x => x.Execute()).Returns(true);
            var runner = new InstallRunner();
            var result = runner.ExecuteBatch(new[] { taskMock.Object });
            Assert.IsTrue(result);
            taskMock.Verify(x => x.Backup());
            taskMock.Verify(x => x.Execute());
            taskMock.Verify(x => x.Rollback(), Times.Never());
        }

        [TestMethod]
        public void TestTaskReturnsFalse()
        {
            var taskMock = new Mock<IInstallerTask>();
            taskMock.Setup(x => x.Backup());
            taskMock.Setup(x => x.Execute()).Returns(false);
            taskMock.SetupGet(x => x.FailOnError).Returns(false);
            var runner = new InstallRunner();
            var result = runner.ExecuteBatch(new[] { taskMock.Object });
            Assert.IsTrue(result);
            taskMock.Verify(x => x.Backup());
            taskMock.Verify(x => x.Execute());
            taskMock.Verify(x => x.Rollback(), Times.Never());
        }

        [TestMethod]
        public void TestRollbackOnTaskReturnsFalse()
        {
            var taskMock = new Mock<IInstallerTask>();
            taskMock.Setup(x => x.Backup());
            taskMock.Setup(x => x.Execute()).Returns(false);
            taskMock.SetupGet(x => x.FailOnError).Returns(true);
            var runner = new InstallRunner();
            var result = runner.ExecuteBatch(new[] { taskMock.Object });
            Assert.IsFalse(result);
            taskMock.Verify(x => x.Backup());
            taskMock.Verify(x => x.Execute());
            taskMock.Verify(x => x.Rollback());
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TestRollbackOnException()
        {
            var taskMock = new Mock<IInstallerTask>();
            taskMock.SetupGet(x => x.FailOnError).Returns(true);
            taskMock.Setup(x => x.Backup());
            taskMock.Setup(x => x.Execute()).Throws(new Exception());
            taskMock.Setup(x => x.Rollback());
            var runner = new InstallRunner();
            try
            {
                var result = runner.ExecuteBatch(new[] { taskMock.Object });
            }
            catch
            {
                taskMock.Verify(x => x.Backup());
                taskMock.Verify(x => x.Execute());
                taskMock.Verify(x => x.Rollback());
                throw;
            }
        }

        [TestMethod]
        public void TestRollbackMultipleTasks()
        {
            int callOrder = 0; 
            var task1Mock = new Mock<IInstallerTask>();
            task1Mock.Setup(x => x.Backup());
            task1Mock.Setup(x => x.Execute()).Returns(true);
            task1Mock.Setup(x => x.Rollback()).Callback(() => Assert.AreEqual(1, callOrder++));
            task1Mock.SetupGet(x => x.FailOnError).Returns(true);
            var task2Mock = new Mock<IInstallerTask>();
            task2Mock.Setup(x => x.Backup());
            task2Mock.Setup(x => x.Execute()).Returns(false);
            task2Mock.Setup(x => x.Rollback()).Callback(() => Assert.AreEqual(0, callOrder++));
            task2Mock.SetupGet(x => x.FailOnError).Returns(true);
            var task3Mock = new Mock<IInstallerTask>();
            task3Mock.Setup(x => x.Backup());
            task3Mock.Setup(x => x.Execute()).Returns(false);
            task3Mock.SetupGet(x => x.FailOnError).Returns(true);
            var runner = new InstallRunner();
            var result = runner.ExecuteBatch(new[] { task1Mock.Object, task2Mock.Object, task3Mock.Object });
            Assert.IsFalse(result);
            task1Mock.Verify(x => x.Backup());
            task1Mock.Verify(x => x.Execute());
            task1Mock.Verify(x => x.Rollback());
            task2Mock.Verify(x => x.Backup());
            task2Mock.Verify(x => x.Execute());
            task2Mock.Verify(x => x.Rollback());
            task3Mock.Verify(x => x.Backup(), Times.Never());
            task3Mock.Verify(x => x.Execute(), Times.Never());
            task3Mock.Verify(x => x.Rollback(), Times.Never());
        }

    }
}