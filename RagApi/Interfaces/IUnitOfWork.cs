// RagApi/Interfaces/IUnitOfWork.cs
using System;
using System.Threading.Tasks;
using RagApi.Interfaces.Repositories;

namespace RagApi.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ICandidateRepository Candidates { get; }
        IDocumentRepository Documents { get; }
        IJobPostingRepository JobPostings { get; }
        IApplicationRepository Applications { get; }
        IUserRepository Users { get; }

        Task CommitAsync();
    }
}
