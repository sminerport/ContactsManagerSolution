using ContactsManager.Core.Domain.RepositoryContracts;

using Entities;

using Microsoft.Extensions.Logging;

using Serilog;

using ServiceContracts;

namespace Services
{
    public class PersonsDeleterService : IPersonsDeleterService
    {
        #region Fields

        private readonly IPersonsRepository _personsRepository;
        private readonly ILogger<PersonsDeleterService> _logger;
        private readonly IDiagnosticContext _diagnosticContext;

        #endregion Fields

        #region Constructors

        public PersonsDeleterService(
            IPersonsRepository personsRepository,
            ILogger<PersonsDeleterService> logger,
            IDiagnosticContext diagnosticContext)
        {
            _personsRepository = personsRepository;
            _logger = logger;
            _diagnosticContext = diagnosticContext;
        }

        #endregion Constructors

        #region Delete Methods

        public async Task<bool> DeletePerson(Guid? personID)
        {
            if (personID == null)
            {
                throw new ArgumentNullException(nameof(personID));
            }

            Person? person = await _personsRepository.GetPersonByPersonID(personID.Value);

            if (person == null)
                return false;

            await _personsRepository.DeletePersonByPersonID(personID.Value);

            return true;
        }

        #endregion Delete Methods
    }
}