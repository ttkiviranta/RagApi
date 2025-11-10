// RagApi.Tests/Repositories/CandidateRepositoryTests.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RagApi.Data;
using RagApi.Data.Repositories;
using RagApi.Models;
using Xunit;
using FluentAssertions;

namespace RagApi.Tests.Repositories
{
    public class CandidateRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public CandidateRepositoryTests()
        {
            // Use InMemory database for testing
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"CandidateTestDb_{Guid.NewGuid()}")
                .Options;

            // Seed the database with test data
            SeedDatabase();
        }

        // In CandidateRepositoryTests.cs
        private void SeedDatabase()
        {
            using var context = new ApplicationDbContext(_options);

            // Clear the database before seeding
            context.Candidates.RemoveRange(context.Candidates);
            context.SaveChanges();

            // Add test data
            context.Candidates.Add(new Candidate
            {
                Id = "candidate-1",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "1234567890",
                LinkedInProfile = "https://linkedin.com/in/johndoe",
                Skills = "C#,Azure,.NET",
                CurrentCompany = "TechCorp",
                CurrentPosition = "Software Engineer",
                Location = "New York",  // Add this missing required field
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            context.Candidates.Add(new Candidate
            {
                Id = "candidate-2",
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                PhoneNumber = "0987654321",
                LinkedInProfile = "https://linkedin.com/in/janesmith",
                Skills = "JavaScript,React,TypeScript",
                CurrentCompany = "TechCorp",
                CurrentPosition = "Software Engineer",
                Location = "Seattle",  // Add this missing required field
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            context.SaveChanges();
        }



        [Fact]
        public async Task GetAllAsync_ShouldReturnAllCandidates()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var repository = new CandidateRepository(context);

            // Act
            var candidates = await repository.GetAllAsync();

            // Assert
            candidates.Should().HaveCount(2);
            candidates.Should().Contain(c => c.Email == "john.doe@example.com");
            candidates.Should().Contain(c => c.Email == "jane.smith@example.com");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCandidate_WhenExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var repository = new CandidateRepository(context);

            // Act
            var candidate = await repository.GetByIdAsync("candidate-1");

            // Assert
            candidate.Should().NotBeNull();
            candidate.FirstName.Should().Be("John");
            candidate.LastName.Should().Be("Doe");
            candidate.Email.Should().Be("john.doe@example.com");
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldReturnCandidate_WhenExists()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var repository = new CandidateRepository(context);

            // Act
            var candidate = await repository.GetByEmailAsync("jane.smith@example.com");

            // Assert
            candidate.Should().NotBeNull();
            candidate.FirstName.Should().Be("Jane");
            candidate.LastName.Should().Be("Smith");
            candidate.Email.Should().Be("jane.smith@example.com");
        }

        [Fact]
 
        public async Task AddAsync_ShouldAddNewCandidate()
        {
            using var context = new ApplicationDbContext(_options);
            var repository = new CandidateRepository(context);

            var newCandidate = new Candidate
            {
                Id = "candidate-3",
                FirstName = "Robert",
                LastName = "Johnson",
                Email = "robert@example.com",
                PhoneNumber = "1234567890",
                LinkedInProfile = "https://linkedin.com/in/robertjohnson",
                Location = "Chicago",
                // Add these missing required fields:
                CurrentCompany = "ABC Corp",
                CurrentPosition = "Software Developer",
                Skills = "Java,Python,SQL",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await repository.AddAsync(newCandidate);
            await repository.SaveChangesAsync();

            var savedCandidate = await repository.GetByIdAsync("candidate-3");
            savedCandidate.Should().NotBeNull();
            savedCandidate.FirstName.Should().Be("Robert");
            savedCandidate.LastName.Should().Be("Johnson");
            savedCandidate.Email.Should().Be("robert@example.com");
        }
    }
}
