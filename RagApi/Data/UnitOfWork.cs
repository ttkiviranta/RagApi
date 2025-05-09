// RagApi/Data/UnitOfWork.cs
using System;
using System.Threading.Tasks;
using RagApi.Interfaces;
using RagApi.Interfaces.Repositories;
using RagApi.Data.Repositories;

namespace RagApi.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IVectorSearchService _searchService;

        private ICandidateRepository _candidateRepository;
        private IDocumentRepository _documentRepository;
        private IJobPostingRepository _jobPostingRepository;
        private IApplicationRepository _applicationRepository;

        public UnitOfWork(
            ApplicationDbContext context,
            IBlobStorageService blobStorageService,
            IVectorSearchService searchService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
            _searchService = searchService;
        }

        public ICandidateRepository Candidates =>
            _candidateRepository ??= new CandidateRepository(_context);

        public IDocumentRepository Documents =>
            _documentRepository ??= new DocumentRepository(_context, _blobStorageService, _searchService);

        public IJobPostingRepository JobPostings =>
            _jobPostingRepository ??= new JobPostingRepository(_context);

        public IApplicationRepository Applications =>
            _applicationRepository ??= new ApplicationRepository(_context);

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
