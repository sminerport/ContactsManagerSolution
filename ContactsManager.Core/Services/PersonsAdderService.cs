using ContactsManager.Core.Domain.RepositoryContracts;

using Entities;

using Microsoft.Extensions.Logging;

using Serilog;

using ServiceContracts;
using ServiceContracts.DTO;

using Services.Helpers;

namespace Services
{
    public class PersonsAdderService : IPersonsAdderService
    {
        #region Fields

        private readonly IPersonsRepository _personsRepository;
        private readonly ILogger<PersonsAdderService> _logger;
        private readonly IDiagnosticContext _diagnosticContext;

        #endregion Fields

        #region Constructors

        public PersonsAdderService(
            IPersonsRepository personsRepository,
            ILogger<PersonsAdderService> logger,
            IDiagnosticContext diagnosticContext)
        {
            _personsRepository = personsRepository;
            _logger = logger;
            _diagnosticContext = diagnosticContext;
        }

        #endregion Constructors

        #region Add Methods

        public async Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest)
        {
            // Validation: personAddRequest parameter can't be null
            if (personAddRequest == null)
                throw new ArgumentNullException(nameof(personAddRequest));

            ValidationHelper.ModelValidation(personAddRequest);

            Person person = personAddRequest.ToPerson();

            person.PersonID = Guid.NewGuid();

            await _personsRepository.AddPerson(person);

            return person.ToPersonResponse();
        }

        #endregion Add Methods
    }
}