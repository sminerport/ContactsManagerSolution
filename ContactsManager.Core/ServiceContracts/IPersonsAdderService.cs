using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace ServiceContracts
{
    /// <summary>
    /// Represents business logic for manipulating Person entity
    /// </summary>
    public interface IPersonsAdderService
    {
        /// <summary>
        /// Adds a person object to the list of persons
        /// </summary>
        /// <param name="personAddRequest">Person object to be added</param>
        /// <returns>Returns the person object after adding it (including newly generated person id)</returns>
        Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest);
    }
}