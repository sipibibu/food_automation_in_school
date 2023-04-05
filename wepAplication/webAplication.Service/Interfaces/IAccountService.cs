﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using webAplication.Domain;
using webAplication.Domain.Persons;
using webAplication.Service.Models;

namespace webAplication.Service.Interfaces
{
    public interface IAccountService
    {
        Task<BaseResponse<JwtSecurityTokenHandler>> RefreshToken(RegisterViewModel model);
        Task<BaseResponse<User.Entity>> Register(RegisterViewModel model);
        Task<BaseResponse<ClaimsIdentity>> Login(LoginViewModel model);
        //Task<BaseResponse<String>> SetEmail(string userId, string email);
        Task<BaseResponse<SchoolKid.Entity>> CreateSchoolKid(SchoolKid.Entity schoolKidPersonEntity);
        Task<BaseResponse<IEnumerable<SchoolKid.Entity>>> GetParentSchoolKids(string parentId);
        Task<BaseResponse<Parent.Entity>> PutSchoolKidsIntoParent(string trusteeId, string[] schoolKidIds);

        IEnumerable<Person> GetPersons(string role);
        BaseResponse<string> DeletePerson(string personId);
        void UpdatePerson(dynamic person);
        void UpdateUserLogin(User user, string login);
        public void UpdateUserPassword(User user,string password);

        public User GetUser(string id);
        public List<User> GetUsers();
        public User GetUserLocal(string id);


        //Task<BaseResponse<Person>> PutImage(string personId, string imageId);
    }
}
