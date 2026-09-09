using AutoMapper;
using BookStoreProject.Controllers;
using BookStoreProject.DTOs;
using BookStoreProject.Repository;
using BookStoreProject.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BookStoreProject.Tests.ControllerTests
{
    [TestClass]
    public class AdminControllerTests
    {
        private Mock<IAdminOrderRepo> _adminRepoMock = null!;
        private Mock<IMapper> _mapperMock = null!;
        private AdminController _controller = null!;

        [TestInitialize]
        public void Setup()
        {
            _adminRepoMock = new Mock<IAdminOrderRepo>();
            _mapperMock = new Mock<IMapper>();

            _controller = new AdminController(
                _adminRepoMock.Object,
                _mapperMock.Object);

            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());
        }

        // ==========================================================
        // INDEX - SUCCESS
        // ==========================================================

        [TestMethod]
        public async Task Index_ShouldReturnView_WhenDashboardDataExists()
        {
            var dashboard = new AdminDashboardVM
            {
                TotalRevenue = 50000,
                TotalOrders = 100,
                TotalBooksSold = 250
            };

            var dashboardDto = new AdminDashboardDto
            {
                TotalRevenue = 50000,
                TotalOrders = 100,
                TotalBooksSold = 250
            };

            _adminRepoMock
                .Setup(x => x.GetDashboardDataAsync())
                .ReturnsAsync(dashboard);

            _mapperMock
                .Setup(x => x.Map<AdminDashboardDto>(dashboard))
                .Returns(dashboardDto);

            var result = await _controller.Index();

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);
            Assert.IsNotNull(viewResult.Model);

            Assert.AreSame(
                dashboardDto,
                viewResult.Model);
        }

        [TestMethod]
        public async Task Index_ShouldReturnCorrectDashboardData()
        {
            var dashboard = new AdminDashboardVM
            {
                TotalRevenue = 75000,
                TotalOrders = 150,
                TotalBooksSold = 400
            };

            var dashboardDto = new AdminDashboardDto
            {
                TotalRevenue = 75000,
                TotalOrders = 150,
                TotalBooksSold = 400
            };

            _adminRepoMock
                .Setup(x => x.GetDashboardDataAsync())
                .ReturnsAsync(dashboard);

            _mapperMock
                .Setup(x => x.Map<AdminDashboardDto>(dashboard))
                .Returns(dashboardDto);

            var result = await _controller.Index();

            var viewResult = result as ViewResult;
            var model = viewResult!.Model as AdminDashboardDto;

            Assert.IsNotNull(model);

            Assert.AreEqual(75000, model.TotalRevenue);
            Assert.AreEqual(150, model.TotalOrders);
            Assert.AreEqual(400, model.TotalBooksSold);
        }

        // ==========================================================
        // NULL DASHBOARD
        // ==========================================================

        [TestMethod]
        public async Task Index_ShouldReturnEmptyDashboard_WhenDashboardDataIsNull()
        {
            _adminRepoMock
                .Setup(x => x.GetDashboardDataAsync())
                .ReturnsAsync((AdminDashboardVM?)null);

            var result = await _controller.Index();

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);

            var model = viewResult.Model as AdminDashboardDto;

            Assert.IsNotNull(model);

            Assert.AreEqual(0, model.TotalRevenue);
            Assert.AreEqual(0, model.TotalOrders);
            Assert.AreEqual(0, model.TotalBooksSold);

            Assert.AreEqual(
                "Unable to load dashboard data.",
                _controller.TempData["Error"]);
        }

        // ==========================================================
        // MAPPING RETURNS NULL
        // ==========================================================

        [TestMethod]
        public async Task Index_ShouldReturnEmptyDashboard_WhenMappingReturnsNull()
        {
            var dashboard = new AdminDashboardVM
            {
                TotalRevenue = 10000,
                TotalOrders = 20,
                TotalBooksSold = 50
            };

            _adminRepoMock
                .Setup(x => x.GetDashboardDataAsync())
                .ReturnsAsync(dashboard);

            _mapperMock
                .Setup(x => x.Map<AdminDashboardDto>(dashboard))
                .Returns((AdminDashboardDto?)null);

            var result = await _controller.Index();

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);

            var model = viewResult.Model as AdminDashboardDto;

            Assert.IsNotNull(model);

            Assert.AreEqual(0, model.TotalRevenue);
            Assert.AreEqual(0, model.TotalOrders);
            Assert.AreEqual(0, model.TotalBooksSold);

            Assert.AreEqual(
                "Unable to prepare dashboard data.",
                _controller.TempData["Error"]);
        }

        // ==========================================================
        // REPOSITORY EXCEPTION
        // ==========================================================

        [TestMethod]
        public async Task Index_ShouldReturnEmptyDashboard_WhenRepositoryThrowsException()
        {
            _adminRepoMock
                .Setup(x => x.GetDashboardDataAsync())
                .ThrowsAsync(
                    new Exception("Database connection failed"));

            var result = await _controller.Index();

            var viewResult = result as ViewResult;

            Assert.IsNotNull(viewResult);

            var model = viewResult.Model as AdminDashboardDto;

            Assert.IsNotNull(model);

            Assert.AreEqual(0, model.TotalRevenue);
            Assert.AreEqual(0, model.TotalOrders);
            Assert.AreEqual(0, model.TotalBooksSold);

            Assert.AreEqual(
                "An error occurred while loading the admin dashboard.",
                _controller.TempData["Error"]);
        }

        // ==========================================================
        // REPOSITORY CALL VERIFICATION
        // ==========================================================

        [TestMethod]
        public async Task Index_ShouldCallGetDashboardDataAsync_Once()
        {
            var dashboard = new AdminDashboardVM
            {
                TotalRevenue = 1000,
                TotalOrders = 10,
                TotalBooksSold = 20
            };

            var dashboardDto = new AdminDashboardDto
            {
                TotalRevenue = 1000,
                TotalOrders = 10,
                TotalBooksSold = 20
            };

            _adminRepoMock
                .Setup(x => x.GetDashboardDataAsync())
                .ReturnsAsync(dashboard);

            _mapperMock
                .Setup(x => x.Map<AdminDashboardDto>(dashboard))
                .Returns(dashboardDto);

            await _controller.Index();

            _adminRepoMock.Verify(
                x => x.GetDashboardDataAsync(),
                Times.Once);
        }

        // ==========================================================
        // MAPPER CALL VERIFICATION
        // ==========================================================

        [TestMethod]
        public async Task Index_ShouldMapDashboardData_Once()
        {
            var dashboard = new AdminDashboardVM
            {
                TotalRevenue = 1000,
                TotalOrders = 10,
                TotalBooksSold = 20
            };

            var dashboardDto = new AdminDashboardDto
            {
                TotalRevenue = 1000,
                TotalOrders = 10,
                TotalBooksSold = 20
            };

            _adminRepoMock
                .Setup(x => x.GetDashboardDataAsync())
                .ReturnsAsync(dashboard);

            _mapperMock
                .Setup(x => x.Map<AdminDashboardDto>(dashboard))
                .Returns(dashboardDto);

            await _controller.Index();

            _mapperMock.Verify(
                x => x.Map<AdminDashboardDto>(dashboard),
                Times.Once);
        }

        // ==========================================================
        // MAPPER SHOULD NOT BE CALLED WHEN REPOSITORY RETURNS NULL
        // ==========================================================

        [TestMethod]
        public async Task Index_ShouldNotCallMapper_WhenDashboardDataIsNull()
        {
            _adminRepoMock
                .Setup(x => x.GetDashboardDataAsync())
                .ReturnsAsync((AdminDashboardVM?)null);

            await _controller.Index();

            _mapperMock.Verify(
                x => x.Map<AdminDashboardDto>(
                    It.IsAny<AdminDashboardVM>()),
                Times.Never);
        }

        // ==========================================================
        // MAPPER SHOULD NOT BE CALLED WHEN REPOSITORY THROWS
        // ==========================================================

        [TestMethod]
        public async Task Index_ShouldNotCallMapper_WhenRepositoryThrowsException()
        {
            _adminRepoMock
                .Setup(x => x.GetDashboardDataAsync())
                .ThrowsAsync(new Exception());

            await _controller.Index();

            _mapperMock.Verify(
                x => x.Map<AdminDashboardDto>(
                    It.IsAny<AdminDashboardVM>()),
                Times.Never);
        }
    }
}