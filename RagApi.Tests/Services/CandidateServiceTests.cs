// RagApi.Tests/Services/CandidateServiceTests.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using RagApi.Interfaces;
using RagApi.Interfaces.Repositories;
using RagApi.Models;
using RagApi.Models.Dto;
using RagApi.Services;
using Xunit;
using FluentAssertions;

namespace RagApi.Tests.Services
{
    public class CandidateServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ICandidateRepository> _mockCandidateRepository;
        private readonly Mock<IDocumentService> _mockDocumentService;

        public CandidateServiceTests()
        {
            _mockCandidateRepository = new Mock<ICandidateRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockDocumentService = new Mock<IDocumentService>();

            _mockUnitOfWork.Setup(uow => uow.Candidates).Returns(_mockCandidateRepository.Object);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Candidates()
        {
            // Arrange
            var candidates = new List<Candidate>
            {
                new Candidate { Id = "1", FirstName = "John", LastName = "Doe", Email = "john@example.com" },
                new Candidate { Id = "2", FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" }
            };

            _mockCandidateRepository.Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(candidates);

            var service = new CandidateService(_mockUnitOfWork.Object, _mockDocumentService.Object);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(c => c.Email == "john@example.com");
            result.Should().Contain(c => c.Email == "jane@example.com");
        }

        [Fact]
        public async Task CreateAsync_Should_Add_Candidate_And_Return_It()
        {
            // Arrange
            var dto = new CandidateCreateDto
            {
                FirstName = "New",
                LastName = "Candidate",
                Email = "new@example.com"
            };

            Candidate savedCandidate = null;

            _mockCandidateRepository.Setup(repo => repo.AddAsync(It.IsAny<Candidate>()))
                .Callback<Candidate>(c => savedCandidate = c)
                .Returns(Task.CompletedTask);

            var service = new CandidateService(_mockUnitOfWork.Object, _mockDocumentService.Object);

            // Act
            var result = await service.CreateAsync(dto, null);

            // Assert
            _mockCandidateRepository.Verify(repo => repo.AddAsync(It.IsAny<Candidate>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(), Times.Once);

            result.Should().NotBeNull();
            result.FirstName.Should().Be("New");
            result.LastName.Should().Be("Candidate");
            result.Email.Should().Be("new@example.com");
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Existing_Candidate()
        {
            // Arrange
            var existingCandidate = new Candidate
            {
                Id = "candidate-1",
                FirstName = "Original",
                LastName = "Name",
                Email = "original@example.com"
            };

            var dto = new CandidateUpdateDto
            {
                FirstName = "Updated",
                LastName = "Name",
                Email = "updated@example.com"
            };

            _mockCandidateRepository.Setup(repo => repo.GetByIdAsync("candidate-1"))
                .ReturnsAsync(existingCandidate);

            var service = new CandidateService(_mockUnitOfWork.Object, _mockDocumentService.Object);

            // Act
            var result = await service.UpdateAsync("candidate-1", dto);

            // Assert
            result.Should().NotBeNull();
            result.FirstName.Should().Be("Updated");
            result.Email.Should().Be("updated@example.com");

            _mockCandidateRepository.Verify(repo => repo.UpdateAsync(It.IsAny<Candidate>()), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.CommitAsync(), Times.Once);
        }
    }
}
