using System.Linq.Expressions;

using AutoFixture;

using ContactsManager.Core.Domain.RepositoryContracts;

using Entities;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Moq;

using Serilog;

using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

using Services;

using Xunit.Abstractions;

namespace CRUDTests
{
    public class PersonsServiceTest
    {
        #region Fields

        private readonly IPersonsGetterService _personsGetterService;
        private readonly IPersonsAdderService _personsAdderService;
        private readonly IPersonsDeleterService _personsDeleterService;
        private readonly IPersonsUpdaterService _personsUpdaterService;
        private readonly IPersonsSorterService _personsSorterService;

        private readonly IPersonsRepository _personsRepository;
        private readonly Mock<IPersonsRepository> _personsRepositoryMock;

        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFixture _fixture;

        private readonly ILogger<PersonsGetterService> _getterLogger;
        private readonly Mock<ILogger<PersonsGetterService>> _getterLoggerMock;

        private readonly ILogger<PersonsAdderService> _adderLogger;
        private readonly Mock<ILogger<PersonsAdderService>> _adderLoggerMock;

        private readonly ILogger<PersonsSorterService> _sorterLogger;
        private readonly Mock<ILogger<PersonsSorterService>> _sorterLoggerMock;

        private readonly ILogger<PersonsDeleterService> _deleterLogger;
        private readonly Mock<ILogger<PersonsDeleterService>> _deleterLoggerMock;

        private readonly ILogger<PersonsUpdaterService> _updaterLogger;
        private readonly Mock<ILogger<PersonsUpdaterService>> _updaterLoggerMock;

        private readonly IDiagnosticContext _diagnosticContext;
        private readonly Mock<IDiagnosticContext> _diagnosticContextMock;

        #endregion Fields

        #region Constructor

        public PersonsServiceTest(ITestOutputHelper testOutputHelper)
        {
            _fixture = new Fixture();
            _personsRepositoryMock = new Mock<IPersonsRepository>();
            _personsRepository = _personsRepositoryMock.Object;

            _diagnosticContextMock = new Mock<IDiagnosticContext>();
            _diagnosticContext = _diagnosticContextMock.Object;

            _getterLoggerMock = new Mock<ILogger<PersonsGetterService>>();
            _getterLogger = _getterLoggerMock.Object;

            _personsGetterService = new PersonsGetterService(
                _personsRepository,
                _getterLogger,
                _diagnosticContext);

            _adderLoggerMock = new Mock<ILogger<PersonsAdderService>>();
            _adderLogger = _adderLoggerMock.Object;

            _personsAdderService = new PersonsAdderService(
                _personsRepository,
                _adderLogger,
                _diagnosticContext);

            _sorterLoggerMock = new Mock<ILogger<PersonsSorterService>>();
            _sorterLogger = _sorterLoggerMock.Object;

            _personsSorterService = new PersonsSorterService(
                _personsRepository,
                _sorterLogger,
                _diagnosticContext);

            _updaterLoggerMock = new Mock<ILogger<PersonsUpdaterService>>();
            _updaterLogger = _updaterLoggerMock.Object;

            _personsUpdaterService = new PersonsUpdaterService(
                _personsRepository,
                _updaterLogger,
                _diagnosticContext);

            _deleterLoggerMock = new Mock<ILogger<PersonsDeleterService>>();
            _deleterLogger = _deleterLoggerMock.Object;

            _personsDeleterService = new PersonsDeleterService(
                _personsRepository,
                _deleterLogger,
                _diagnosticContext);

            _testOutputHelper = testOutputHelper;
        }

        #endregion Constructor

        #region AddPerson

        [Fact]
        public async Task AddPerson_NullPerson_ToBeArgumentNullException()
        {
            // Arrange
            PersonAddRequest? personAddRequest = null;

            // Act
            Func<Task> action = async () =>
            {
                await _personsAdderService.AddPerson(personAddRequest);
            };

            // Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task AddPerson_PersonNameIsNull_ToBeArgumentException()
        {
            // Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.PersonName, null as string)
                .Create();

            Person person = personAddRequest.ToPerson();

            _personsRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>())).ReturnsAsync(person);

            // Act
            Func<Task> action = async () =>
            {
                await _personsAdderService.AddPerson(personAddRequest);
            };

            // Assert
            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task AddPerson_FullPersonDetails_ToBeSuccessful()
        {
            // Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "someone@example.com")
                .Create();

            Person person = personAddRequest.ToPerson();
            PersonResponse person_response_expected = person.ToPersonResponse();

            _personsRepositoryMock
                .Setup(temp => temp.AddPerson(It.IsAny<Person>()))
                .ReturnsAsync(person);

            // Act
            PersonResponse person_response_from_add = await _personsAdderService.AddPerson(personAddRequest);
            person_response_expected.PersonID = person_response_from_add.PersonID;

            // Assert
            person_response_from_add.PersonID.Should().NotBe(Guid.Empty);
            person_response_from_add.Should().Be(person_response_expected);
        }

        #endregion AddPerson

        #region GetPersonPersonID

        [Fact]
        public async Task GetPersonByPersonID_NullPersonID_ToBeNull()
        {
            // Arrange
            Guid? personID = null;

            // Act
            PersonResponse? person_response_from_get = await _personsGetterService.GetPersonByPersonID(personID);

            // Assert
            //Assert.Null(person_response_from_get);
            person_response_from_get.Should().BeNull();
        }

        [Fact]
        public async Task GetPersonByPersonID_WithPersonID_ToBeSuccessful()
        {
            // Arrange
            Person person = _fixture.Build<Person>()
                .With(temp => temp.Email, "email@sample.com")
                .With(temp => temp.Country, null as Country)
                .Create();

            PersonResponse person_response_expected = person.ToPersonResponse();

            _personsRepositoryMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(person);

            // Act
            PersonResponse? person_response_from_get = await _personsGetterService.GetPersonByPersonID(person.PersonID);

            person_response_from_get.Should().Be(person_response_expected);
        }

        #endregion GetPersonPersonID

        #region GetAllPersons

        [Fact]
        public async Task GetAllPersons_EmptyList()
        {
            // Arrange
            _personsRepositoryMock
                .Setup(temp => temp.GetAllPersons())
                .ReturnsAsync(new List<Person>());

            // Act
            List<PersonResponse> persons_from_get = await _personsGetterService.GetAllPersons();

            // Assert
            persons_from_get.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllPersons_WithFewPersons_ToBeSuccessful()
        {
            // Arrange
            List<Person> persons = new List<Person>() {
            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_1@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_2@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_3@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
            };

            List<PersonResponse> person_response_list_expected = persons
                .Select(temp => temp.ToPersonResponse())
                .ToList();

            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse person_response_from_add in person_response_list_expected)
            {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }

            _personsRepositoryMock
                .Setup(temp => temp.GetAllPersons())
                .ReturnsAsync(persons);

            // Act
            List<PersonResponse> persons_list_from_get = await _personsGetterService.GetAllPersons();

            _testOutputHelper.WriteLine("Actual: ");
            foreach (PersonResponse person_from_get in persons_list_from_get)
            {
                _testOutputHelper.WriteLine(person_from_get.ToString());
            }

            // Assert
            persons_list_from_get.Should().BeEquivalentTo(person_response_list_expected);
        }

        #endregion GetAllPersons

        #region GetFilteredPersons

        [Fact]
        public async Task GetFilteredPersons_EmptySearchText_ToBeSuccessful()
        {
            // Arrange
            List<Person> persons = new List<Person>() {
            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_1@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_2@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_3@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
            };

            List<PersonResponse> person_response_list_expected = persons
                .Select(temp => temp.ToPersonResponse())
                .ToList();

            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse person_response_from_add in person_response_list_expected)
            {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }

            _personsRepositoryMock
                .Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(persons);

            // Act
            List<PersonResponse> persons_list_from_search = await _personsGetterService.GetFilteredPersons(nameof(Person.PersonName), "");

            _testOutputHelper.WriteLine("Actual: ");
            foreach (PersonResponse person_from_search in persons_list_from_search)
            {
                _testOutputHelper.WriteLine(person_from_search.ToString());
            }

            persons_list_from_search.Should().BeEquivalentTo(person_response_list_expected);
        }

        [Fact]
        public async Task GetFilteredPersons_SearchByPersonName_ToBeSuccessful()
        {
            List<Person> persons = new List<Person>() {
            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_1@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_2@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_3@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
            };

            List<PersonResponse> person_response_list_expected = persons
                .Select(temp => temp.ToPersonResponse())
                .ToList();

            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse person_response_from_add in person_response_list_expected)
            {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }

            _personsRepositoryMock
                .Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(persons);

            // Act
            List<PersonResponse> persons_list_from_search = await _personsGetterService.GetFilteredPersons(nameof(Person.PersonName), "sa");

            _testOutputHelper.WriteLine("Actual: ");
            foreach (PersonResponse person_from_search in persons_list_from_search)
            {
                _testOutputHelper.WriteLine(person_from_search.ToString());
            }

            persons_list_from_search.Should().BeEquivalentTo(person_response_list_expected);
        }

        #endregion GetFilteredPersons

        #region GetSortedPersons

        [Fact]
        public async Task GetSortedPersons_ToBeSuccessful()
        {
            // Arrange
            List<Person> persons = new List<Person>() {
            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_1@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_2@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),

            _fixture.Build<Person>()
                .With(temp => temp.Email, "someone_3@example.com")
                .With(temp => temp.Country, null as Country)
                .Create(),
            };

            List<PersonResponse> person_response_list_expected = persons
                .Select(temp => temp.ToPersonResponse())
                .ToList();

            _personsRepositoryMock
                .Setup(temp => temp.GetAllPersons())
                .ReturnsAsync(persons);

            _testOutputHelper.WriteLine("Expected: ");
            foreach (PersonResponse person_response_from_add in person_response_list_expected)
            {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }

            List<PersonResponse> allPersons = await _personsGetterService.GetAllPersons();

            // Act
            List<PersonResponse> persons_list_from_sort = await _personsSorterService.GetSortedPersons(
                allPersons: allPersons,
                sortBy: nameof(Person.PersonName),
                sortOrder: SortOrderOptions.DESC);

            _testOutputHelper.WriteLine("Actual: ");
            foreach (PersonResponse person_from_sort in persons_list_from_sort)
            {
                _testOutputHelper.WriteLine(person_from_sort.ToString());
            }

            persons_list_from_sort.Should().BeInDescendingOrder(temp => temp.PersonName);
        }

        #endregion GetSortedPersons

        #region UpdatePerson

        [Fact]
        public async Task UpdatePerson_NullPerson_ToBeArgumentNullException()
        {
            // Arrange
            PersonUpdateRequest? person_update_request = null;

            Func<Task> action = async () =>
            {
                // Act
                await _personsUpdaterService.UpdatePerson(person_update_request);
            };

            // Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task UpdatePerson_InvalidPersonID_ToBeArgumentException()
        {
            // Arrange
            PersonUpdateRequest? person_update_request = _fixture.Create<PersonUpdateRequest>();

            Func<Task> action = async () =>
            {
                // Act
                await _personsUpdaterService.UpdatePerson(person_update_request);
            };

            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task UpdatePerson_PersonNameIsNull_ToBeArgumentException()
        {
            // Arrange
            Person person = _fixture.Build<Person>()
                .With(temp => temp.PersonName, null as string)
                .With(temp => temp.Email, "someone@example.com")
                .With(temp => temp.Gender, "Male")
                .With(temp => temp.Country, null as Country)
                .Create();

            PersonResponse person_response_expected = person.ToPersonResponse();

            PersonUpdateRequest person_update_request = person_response_expected.ToPersonUpdateRequest();

            person_update_request.PersonName = null;

            // Assert
            Func<Task> action = async () =>
            {
                await _personsUpdaterService.UpdatePerson(person_update_request);
            };

            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task UpdatePerson_PersonFullDetails_ToBeSuccessful()
        {
            // Arrange
            Person person = _fixture.Build<Person>()
                .With(temp => temp.Email, "someone@example.com")
                .With(temp => temp.Country, null as Country)
                .With(temp => temp.Gender, "Male")
                .Create();

            PersonResponse person_response_expected = person.ToPersonResponse();

            PersonUpdateRequest person_update_request = person_response_expected.ToPersonUpdateRequest();

            person_update_request.PersonName = "William";
            person_update_request.Email = "william@example.com";

            _personsRepositoryMock.Setup(temp => temp.UpdatePerson(It.IsAny<Person>())).ReturnsAsync(person);
            _personsRepositoryMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(person);

            // Act
            PersonResponse person_response_from_update = await _personsUpdaterService.UpdatePerson(person_update_request);

            PersonResponse? person_response_from_get = await _personsGetterService.GetPersonByPersonID(person_response_from_update.PersonID);

            // Assert
            person_response_from_update.Should().Be(person_response_from_get);
        }

        #endregion UpdatePerson

        #region DeletePerson

        [Fact]
        public async Task DeletePerson_ValidPersonID_ToBeSuccessful()
        {
            // Arrange
            Person person = _fixture.Build<Person>()
                .With(temp => temp.PersonName, "Jones")
                .With(temp => temp.Email, "someone@example.com")
                .With(temp => temp.Country, null as Country)
                .With(temp => temp.Gender, "female")
                .Create();

            _personsRepositoryMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(person);

            _personsRepositoryMock.Setup(temp => temp.DeletePersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(true);

            // Act
            bool isDeleted = await _personsDeleterService.DeletePerson(person.PersonID);

            // Assert
            isDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task DeletePerson_InvalidPersonID_ToBeSuccessful()
        {
            // Arrange
            Person person = _fixture.Build<Person>()
                .With(temp => temp.PersonName, "Jones")
                .With(temp => temp.Email, "someone@example.com")
                .With(temp => temp.Country, null as Country)
                .Create();

            _personsRepositoryMock.Setup(temp => temp.GetPersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(null as Person);
            _personsRepositoryMock.Setup(temp => temp.DeletePersonByPersonID(It.IsAny<Guid>())).ReturnsAsync(false);

            bool isDeleted = await _personsDeleterService.DeletePerson(Guid.NewGuid());

            isDeleted.Should().BeFalse();
        }

        #endregion DeletePerson
    }
}