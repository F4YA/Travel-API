using API.Interfaces;
using API.Models;
using System;
using API.DTOs;
using System.Collections.Generic;
using System.Linq;

namespace API.Services
{
    public class UserServices : IUserServices
    {
        private readonly IUser _userRepository;
        private readonly ISecurity _security;

        public UserServices(IUser userRepository, ISecurity security)
        {
            _userRepository = userRepository;
            _security = security;
        }

        //CREATE METHODS
        public UserDto create(User user)
        {
            if(user == null){
                throw new Exception("400: Objeto nulo"){ HResult = 400};
            }else if(string.IsNullOrEmpty(user.Id)){
                throw new Exception("400: Identificador não pode ser nulo"){HResult = 400};
            }else if(_userRepository.IsUserRegistered(user.Id).Result){
                throw new Exception("200: Identificador não disponivel"){ HResult = 200};
            }else if(string.IsNullOrEmpty(user.Email) && string.IsNullOrEmpty(user.Phone)){
                throw new Exception("400: Telefone e Email nulos"){HResult = 400};
            }else if(!string.IsNullOrEmpty(user.Email) && _userRepository.IsEmailRegistered(user.Email).Result){
                throw new Exception("200: Email já cadastrado"){ HResult = 200};
            }else if(!string.IsNullOrEmpty(user.Phone) && _userRepository.IsPhoneRegistered(user.Phone).Result){
                throw new Exception("200: Telefone já cadastrado"){ HResult = 200};
            }else if(user.Location == null){
                throw new Exception("400: A localização não pode ser nula"){HResult = 400};
            }else if(user.Interests == null || !user.Interests.Any()){
                throw new Exception("400: Os interesses não podem ser nulos"){HResult = 400};
            }else{
                user.Password = _security.hashPassword(user.Password);
                return _userRepository.create(user).Result;
            }
        }

        public void followUser(FollowDto obj)
        {
            if(_userRepository.IsUserRegistered(id: obj.FollowerId).Result 
            && _userRepository.IsUserRegistered(id: obj.FollowedId).Result){    
                _userRepository.followUser(obj);
            }else{throw new Exception("404: Usuario(s) não encontrado(s)"){ HResult = 404};}
        }
 
        //DELETE METHODS
        public void unfollowUser(FollowDto obj)
        {
            if(_userRepository.IsUserRegistered(id: obj.FollowerId).Result 
            && _userRepository.IsUserRegistered(id: obj.FollowedId).Result){
                _userRepository.unfollowUser(obj);
            }else
            {
                throw new Exception("404: Usuario(s) não encontrado(s)"){HResult = 404};
            }
        }
        public void deleteUser(DeleteUserDto obj)
        {
            if(_userRepository.IsUserRegistered(obj.Id).Result){
                var result = _security.comparePasswords(_userRepository.getPassword(obj.Id).Result, obj.Password);
                if(result){
                    _userRepository.deleteUser(obj);
                }else{
                    throw new Exception("400: Senha incorreta"){ HResult = 400};
                }
            }else
            {
                throw new Exception("404: Usuario não encontrado"){ HResult = 404};
            }
        }

        //GET METHODS
        public UserDto getUserByEmail(string email)
        {
            if(_userRepository.IsEmailRegistered(email).Result){
                return _userRepository.getUserByEmail(email).Result;
            }else{
                throw new Exception("404: Usuario não encontrado"){ HResult = 404};
            }
        }

        public UserDto getUserById(string id)
        {
            if(_userRepository.IsUserRegistered(id).Result){
                var result = _userRepository.getUserById(id).Result;
                return result;
            }else{throw new Exception("404: Usuario não encontrado"){ HResult = 404};}
        }

        private string getPassword(string id)
        {
            if(IsUserRegistered(id)){
                var result =  _userRepository.getPassword(id);
                if(!string.IsNullOrEmpty(result.Result)){
                    return result.Result;
                }else{throw new Exception("500: Senha não obtida"){ HResult = 500};}
            }else{throw new Exception("404: Usuario não encontrado"){ HResult = 404};}
        }

        private string getPasswordByEmail(string email)
        {
            if(IsEmailRegistered(email)){
                return _userRepository.getPasswordByEmail(email).Result;
            }else{throw new Exception("404: Usuario não encontrado"){ HResult = 404};}
        }

        public IEnumerable<UserDto> getFollowers(string id)
        {
           if(IsUserRegistered(id)){
               return _userRepository.getFollowers(id).Result;
           }else{throw new Exception("404: Usuario não encontrado"){ HResult = 404};}
        }

        public IEnumerable<UserDto> getFolloweds(string id)
        {
            if(IsUserRegistered(id)){
                return _userRepository.getFolloweds(id).Result;
            }else{throw new Exception("404: Usuario não encontrado"){ HResult = 404};}
        }

        
        //UPDATE METHODS
        public UserDto updateUserId(UserIdDto obj)
        {
            if(IsUserRegistered(obj.CurrentId))
            {
                if(!IsUserRegistered(obj.NewId))
                {
                    return _userRepository.updateUserId(obj).Result;
                }else{throw new Exception("200: Identificador não disponivel"){ HResult = 200};}
            }else{throw new Exception("404: Usuario não encontrado"){ HResult = 404};}
        }

        public UserDto updateUserName(UserNameDto obj)
        {
            if(IsUserRegistered(obj.Id)){
                return _userRepository.updateUserName(obj).Result;
            }else{throw new Exception("404: Usuario não encontrado"){ HResult = 404};}
        }

        public UserDto updateUserEmail(UserEmailDto obj)
        {
            if(IsUserRegistered(obj.Id)){
                    if(!IsEmailRegistered(obj.NewEmail)){
                        var result = _security.comparePasswords(_userRepository.getPassword(obj.Id).Result, obj.Password);
                        if(result){
                            return _userRepository.updateUserEmail(obj).Result;
                        }else {throw new Exception("400: Senha incorreta"){ HResult = 400};}
                    }else {throw new Exception("200: Email já cadastrado"){ HResult = 200};}
            }else {throw new Exception("404: Usuario não encontrado"){ HResult = 404};}
        }

        public UserDto updateUserPhone(UserPhoneDto obj)
        {
            if(IsUserRegistered(id: obj.Id)){
                if(string.IsNullOrEmpty(obj.NewPhone)){
                    if(!_userRepository.IsPhoneRegistered(obj.NewPhone).Result){
                        var result = _security.comparePasswords(_userRepository.getPassword(obj.Id).Result, obj.Password);
                        if(result){
                            return _userRepository.updateUserPhone(obj).Result;
                        }else {throw new Exception("400: Senha incorreta"){ HResult = 400};}
                    }else {throw new Exception("200: Telefone já cadastrado"){ HResult = 200};}
                }else{throw new Exception("400: Telefone nulo"){HResult = 400};}
            }else {throw new Exception("404: Usuario não encontrado"){ HResult = 404};}
        }

        public UserDto updateUserPassword(UserPasswordDto obj)
        {
            if(IsUserRegistered(id: obj.Id)){
                var result = _security.comparePasswords(_userRepository.getPassword(obj.Id).Result, obj.Password);
                if(result){
                    obj.NewPassword = _security.hashPassword(obj.NewPassword);
                    return _userRepository.updateUserPassword(obj).Result;
                }else {throw new Exception("400: Senha incorreta"){HResult = 400};}
            }else {throw new Exception("404: Usuario não encontrado"){ HResult = 404};}
        }

        public void updateUserProfileImageUrl(string userId, string newImageUrl)
        {
            if(IsUserRegistered(userId)){
                _userRepository.updateUserProfileImageUrl(userId, newImageUrl);
            }else {throw new Exception("404: Usuario não encontrado"){ HResult = 404};}
        }

        public void updateUserBannerUrl(string userId, string newBannerUrl)
        {
            if(IsUserRegistered(userId)){
                _userRepository.updateUserBannerUrl(userId, newBannerUrl);
            }else {throw new Exception("404: Usuario não encontrado"){ HResult = 404};}
        }

        //VALIDATION METHODS
        public bool IsUserRegistered(string id){
            if(!string.IsNullOrEmpty(id)){
                return _userRepository.IsUserRegistered(id).Result;
            }else {throw new Exception("400: Identificador nulo"){HResult = 400};}
        }

        public bool IsEmailRegistered(string email){
            if(!string.IsNullOrEmpty(email)){
                return _userRepository.IsEmailRegistered(email).Result;
            }else {throw new Exception("400: Email nulo"){HResult = 400};}
        }

        public bool IsPhoneRegistered(string phone){
            if(!string.IsNullOrEmpty(phone)){
                return _userRepository.IsPhoneRegistered(phone).Result;
            }else {throw new Exception("400: Telefone nulo"){HResult = 400};}
        }

        public UserDto login(Login obj)
        {
            var password = getPasswordByEmail(obj.Email);
            var result = _security.comparePasswords(hash: password, password: obj.Password);
            if(!string.IsNullOrEmpty(password)){
                if(result){
                    return getUserByEmail(obj.Email);
                }else {throw new Exception("400: Email ou senha incorretos"){ HResult = 400};}
            }else {throw new Exception(){HResult = 500};} 
        }

        public UserDto loginWithFacebook(Login obj)
        {
            if(!string.IsNullOrEmpty(obj.Email)){
                return getUserByEmail(obj.Email);
            }else{throw new Exception("400: Email nulo"){HResult = 400};}
        }

        public UserDto loginWithGoogle(Login obj)
        {
            if(!string.IsNullOrEmpty(obj.Email) && !string.IsNullOrEmpty(obj.Token)){
                return getUserByEmail(obj.Email);
            }else{throw new Exception("400: Email nulo"){HResult = 400};}
        }

        public void resetPassword(string email, string password){
            if(IsEmailRegistered(email)){
                _userRepository.resetUserPassword(email, password);
            }else{throw new Exception("404: Usuario não encontrado"){HResult = 404};}
        }
    }
}


