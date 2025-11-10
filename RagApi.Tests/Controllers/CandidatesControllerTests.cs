// RagApi.Tests/Controllers/CandidatesControllerTests.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RagApi.Api.Controllers;
using RagApi.Data;
using RagApi.Interfaces;
using RagApi.Mapping;
using RagApi.Models;
using RagApi.Models.Dto;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace RagApi.Tests.Controllers
{
    public class CandidatesControllerTests
    {
        private readonly Mock<ICandidateService> _mockCandidateService;
        private readonly Mock<IRequestContext> _mockRequestContext;
        private readonly Mock<ApplicationDbContext> _mockDbContext;
        private readonly Mock<ILogger<CandidatesController>> _mockLogger;
        private readonly Mock<CandidateMapper> _mockCandidateMapper;

        public CandidatesControllerTests()
        {
            _mockCandidateService = new Mock<ICandidateService>();
            _mockRequestContext = new Mock<IRequestContext>();
            _mockCandidateMapper = new Mock<CandidateMapper>();

            // DbContext voi olla hankala mockata suoraan, joten käytä approach:ia joka sopii projektiin
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _mockDbContext = new Mock<ApplicationDbContext>(dbContextOptions);

            _mockLogger = new Mock<ILogger<CandidatesController>>();

            // Aseta oletusarvoja mockien käyttäytymiseen
            _mockRequestContext.Setup(r => r.GetCurrentUserId()).Returns("test-user-id");
        }

        [Fact]
        public async Task GetAll_Should_Return_OkResult_With_Candidates()
        {
            // Arrange
            var candidates = new List<Candidate>
            {
                new Candidate { Id = "1", FirstName = "Test", LastName = "User", Email = "test@example.com" }
            };

            var candidateDtos = new List<CandidateResponseDto>
            {
                new CandidateResponseDto
                {
                    Id = "1",
                    FirstName = "Test",
                    LastName = "User",
                    Email = "test@example.com",
                    PhoneNumber = string.Empty,
                    LinkedInProfile = string.Empty,
                    CurrentPosition = string.Empty,
                    CurrentCompany = string.Empty,
                    ResumeDocumentId = null,
                    CoverLetterDocumentId = null,
                    Skills = string.Empty,
                    Location = string.Empty,
                    CreatedAt = DateTime.UtcNow
                }
            };

            _mockCandidateService.Setup(service => service.GetAllAsync())
                .ReturnsAsync(candidates);

            // Setup Mapperly mapper mock to return DTOs
            _mockCandidateMapper.Setup(m => m.MapToDto(It.IsAny<Candidate>()))
                .Returns<Candidate>(c => candidateDtos.First(dto => dto.Id == c.Id));

            var controller = new CandidatesController(
                _mockRequestContext.Object,
                _mockDbContext.Object,
                _mockCandidateService.Object,
                _mockCandidateMapper.Object,
                _mockLogger.Object);

            // Act
            var result = await controller.GetAll();

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var response = okResult.Value.Should().BeAssignableTo<dynamic>().Subject;

            // Check the response structure
            object data = response.GetType().GetProperty("data").GetValue(response, null);
            ((IEnumerable<CandidateResponseDto>)data).Should().HaveCount(1);
        }

        [Fact]
        public async Task Create_Should_Return_CreatedAtAction_Result()
        {
            // Arrange
            var dto = new CandidateCreateDto
            {
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                PhoneNumber = "1234567890",
                LinkedInProfile = "https://linkedin.com/in/testuser",
                CurrentPosition = "Developer",
                CurrentCompany = "Test Company",
                Skills = "C#,ASP.NET Core",
                Location = "Helsinki"
            };

            var createdCandidate = new Candidate
            {
                Id = "new-id",
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                PhoneNumber = "1234567890",
                LinkedInProfile = "https://linkedin.com/in/testuser",
                CurrentPosition = "Developer",
                CurrentCompany = "Test Company",
                Skills = "C#,ASP.NET Core",
                Location = "Helsinki",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var candidateDto = new CandidateResponseDto
            {
                Id = "new-id",
                FirstName = "Test",
                LastName = "User",
                Email = "test@example.com",
                PhoneNumber = "1234567890",
                LinkedInProfile = "https://linkedin.com/in/testuser",
                CurrentPosition = "Developer",
                CurrentCompany = "Test Company",
                ResumeDocumentId = null,
                CoverLetterDocumentId = null,
                Skills = "C#,ASP.NET Core",
                Location = "Helsinki",
                CreatedAt = DateTime.UtcNow
            };

            // Setup service mock
            _mockCandidateService.Setup(service => service.CreateAsync(It.IsAny<CandidateCreateDto>(), It.IsAny<string>()))
                .ReturnsAsync(createdCandidate);

            // Setup mapper mock
            _mockCandidateMapper.Setup(m => m.MapToDto(It.Is<Candidate>(c => c.Id == "new-id")))
                .Returns(candidateDto);

            var controller = new CandidatesController(
                _mockRequestContext.Object,
                _mockDbContext.Object,
                _mockCandidateService.Object,
                _mockCandidateMapper.Object,
                _mockLogger.Object);

            // Act
            var result = await controller.Create(dto);

            // Assert
            var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
            createdResult.ActionName.Should().Be(nameof(controller.GetById));
            createdResult.RouteValues["id"].Should().Be("new-id");

            // Verify the response structure
            var responseValue = createdResult.Value as dynamic;
            bool errorValue = responseValue.error;
            errorValue.Should().BeFalse();

            var dataObj = responseValue.data;
            ((object)dataObj).Should().NotBeNull();
            string id = dataObj.Id;
            id.Should().Be("new-id");
        }
    }
}