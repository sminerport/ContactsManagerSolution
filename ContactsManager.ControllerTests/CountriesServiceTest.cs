using AutoFixture;

using ContactsManager.Core.Domain.RepositoryContracts;

using Entities;

using FluentAssertions;

using Moq;

using ServiceContracts;
using ServiceContracts.DTO;

using Services;

namespace CRUDTests
{
    public class CountriesServiceTest
    {
        private readonly ICountriesRepository _countriesRepository;
        private readonly Mock<ICountriesRepository> _countriesRepositoryMock;
        private readonly ICountriesService _countriesService;
        private readonly IFixture _fixture;

        public CountriesServiceTest()
        {
            _fixture = new Fixture();
            _countriesRepositoryMock = new Mock<ICountriesRepository>();
            _countriesRepository = _countriesRepositoryMock.Object;
            _countriesService = new CountriesService(_countriesRepository);
        }

        #region AddCountry

        [Fact]
        public async Task AddCountry_NullCountry_ToBeArgumentNullException()
        {
            // Arrange
            CountryAddRequest? request = null;

            // Act
            Func<Task> action = async () =>
            {
                await _countriesService.AddCountry(request);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task AddCountry_CountryNameIsNull_ToBeArgumentException()
        {
            // Arrange
            CountryAddRequest? countryAddRequest = _fixture.Build<CountryAddRequest>()
                .With(temp => temp.CountryName, null as string)
                .Create();

            Func<Task> act = async () => await _countriesService.AddCountry(countryAddRequest);

            //Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task AddCountry_DuplicateCountryName_ToBeArgumentException()
        {
            // Arrange
            CountryAddRequest? request1 = _fixture.Build<CountryAddRequest>()
                .With(temp => temp.CountryName, "USA")
                .Create();

            CountryAddRequest? request2 = _fixture.Build<CountryAddRequest>()
                .With(temp => temp.CountryName, "USA")
                .Create();

            Country? country_from_add_1 = request1.ToCountry();
            Country? country_from_add_2 = request2.ToCountry();

            if (request1.CountryName != null && request2.CountryName != null)
            {
                _countriesRepositoryMock.Setup(temp => temp.GetCountryByCountryName(request1.CountryName))
                    .ReturnsAsync(country_from_add_1);
                _countriesRepositoryMock.Setup(temp => temp.GetCountryByCountryName(request2.CountryName))
                    .ReturnsAsync(country_from_add_2);
            }

            Func<Task> act = async () =>
            {
                await _countriesService.AddCountry(request1);
                await _countriesService.AddCountry(request2);
            };

            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task AddCountry_ProperCountryDetails_ToBeSuccessful()
        {
            // Arrange
            CountryAddRequest country_add_request = _fixture.Build<CountryAddRequest>()
                .With(temp => temp.CountryName, "Japan")
                .Create();

            Country country = country_add_request.ToCountry();

            CountryResponse country_response_expected = country.ToCountryResponse();

            _countriesRepositoryMock
                .Setup(temp => temp.GetCountryByCountryName(It.IsAny<string>()))
                .ReturnsAsync(null as Country);

            _countriesRepositoryMock
                .Setup(temp => temp.AddCountry(It.IsAny<Country>()))
                .ReturnsAsync(country);

            // Act
            CountryResponse country_response_from_add = await _countriesService.AddCountry(country_add_request);

            country_response_expected.CountryID = country_response_from_add.CountryID;

            // Assert
            country_response_from_add.CountryID.Should().NotBe(Guid.Empty);
            country_response_from_add.Should().Be(country_response_expected);
        }

        #endregion AddCountry

        #region GetAllCountries

        [Fact]
        public async Task GetAllCountries_EmptyList()
        {
            // Arrange
            _countriesRepositoryMock
                .Setup(temp => temp.GetAllCountries())
                .ReturnsAsync(new List<Country>());

            // Act
            List<CountryResponse> countries_from_get = await _countriesService.GetAllCountries();

            // Assert
            countries_from_get.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllCountries_WithFewCountries_ToBeSuccessful()
        {
            // Arrange
            List<Country> countries = new List<Country>() {
                _fixture.Build<Country>()
                .With(temp => temp.Persons, null as List<Person>)
                .Create(),
                _fixture.Build<Country>()
                .With(temp => temp.Persons, null as List<Person>)
                .Create(),
                _fixture.Build<Country>()
                .With(temp => temp.Persons, null as List<Person>)
                .Create(),
                _fixture.Build<Country>()
                .With(temp => temp.Persons, null as List<Person>)
                .Create(),
                _fixture.Build<Country>()
                .With(temp => temp.Persons, null as List<Person>)
                .Create(),
            };

            List<CountryResponse> expected_countries = countries
                .Select(temp => temp.ToCountryResponse())
                .ToList();

            _countriesRepositoryMock
                .Setup(temp => temp.GetAllCountries())
                .ReturnsAsync(countries);

            // Act
            List<CountryResponse> countries_list_from_get = await _countriesService.GetAllCountries();

            // Assert
            countries_list_from_get.Should().BeEquivalentTo(expected_countries);
        }

        #endregion GetAllCountries

        #region GetCountryByCountryID

        [Fact]
        public async Task GetCountryByCountryID_NullCountryID()
        {
            // Arrange
            Guid? countryID = null;

            //Act
            CountryResponse? country_response_from_get = await
            _countriesService.GetCountryByCountryID(countryID);

            //Assert
            country_response_from_get.Should().BeNull();
        }

        [Fact]
        public async Task GetCountryByCountryID_ValidCountryID()
        {
            // Arrange
            Country country = _fixture.Build<Country>()
                .With(temp => temp.Persons, null as List<Person>)
                .Create();

            CountryResponse country_response_expected = country.ToCountryResponse();

            _countriesRepositoryMock
                .Setup(temp => temp.GetCountryByCountryID(It.IsAny<Guid>()))
                .ReturnsAsync(country);

            // Act
            CountryResponse? country_response_from_get = await _countriesService.GetCountryByCountryID(country.CountryID);

            // Aseert
            country_response_from_get.Should().Be(country_response_expected);
        }

        #endregion GetCountryByCountryID
    }
}